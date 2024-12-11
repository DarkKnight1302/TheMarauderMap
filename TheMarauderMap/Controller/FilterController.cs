using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Models;
using TheMarauderMap.Repositories;
using TheMarauderMap.Utilities;
using Instrument = TheMarauderMap.Responses.Instrument;

namespace TheMarauderMap.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly ILogger<FilterController> logger;
        private readonly IStockRepository stockRepository;
        private readonly IStockFundamentalsRepository stockFundamentalsRepository;
        private readonly IScreenerClient screenerClient;

        public FilterController(
            ILogger<FilterController> logger,
            IStockRepository stockRepository,
            IStockFundamentalsRepository stockFundamentalsRepository,
            IScreenerClient screenerClient)
        {
            this.logger = logger;
            this.stockRepository = stockRepository;
            this.stockFundamentalsRepository = stockFundamentalsRepository;
            this.screenerClient = screenerClient;
        }

        [HttpGet]
        [Route("stocks")]
        public async Task<IActionResult> FilterStocks()
        {
            var filteredList = new List<Instrument>();
            using (StreamReader r = new StreamReader("complete.json"))
            {
                string json = r.ReadToEnd();
                List<Instrument> items = JsonConvert.DeserializeObject<List<Instrument>>(json);
                filteredList = items.Where(x => (x.Segment.Equals("NSE_EQ") || x.Segment.Equals("BSE_EQ"))).ToList();
                filteredList = filteredList.Where(x => !x.Name.Contains("%")).ToList();
                filteredList = filteredList.Where(x => !x.Name.Any(Char.IsDigit)).ToList();
            }
            ConcurrentBag<Stock> StocksToAdd = new ConcurrentBag<Stock>();
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 2;
            Parallel.ForEach(filteredList, parallelOptions, async item =>
            {
                bool exists = await this.stockRepository.StockExists(item.InstrumentKey);
                if (!exists)
                {
                    Stock stock = new Stock()
                    {
                        Id = item.InstrumentKey,
                        Uid = item.InstrumentKey,
                        Name = item.Name,
                        TradingSymbol = item.TradingSymbol
                    };
                    StocksToAdd.Add(stock);
                    this.logger.LogInformation($"Stock added {stock.Id} : {stock.Name}");
                }
            });
            await this.stockRepository.UpdateStockPrice(StocksToAdd.ToList());
            string jsonString = JsonConvert.SerializeObject(filteredList, Formatting.Indented);

            // Write the JSON string to a file using StreamWriter
            string filePath = "StockName.json";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(jsonString);
            }
            return Ok();
        }

        [HttpGet]
        [Route("stockFundamentals")]
        public async Task<IActionResult> FeedFundamentals()
        {
            var allStocks = await this.stockRepository.GetAllStocks();
            int timePeriod = 1000;
            List<string> stockIds = allStocks.Select(x => x.TradingSymbol).ToList();
            var stockFundamentalMap = await this.stockFundamentalsRepository.GetStockFundamentals(stockIds);
            foreach(Stock stock in allStocks)
            {
                try
                {
                    if (stockFundamentalMap.ContainsKey(stock.TradingSymbol))
                    {
                        continue;
                    }
                    await Task.Delay(timePeriod);
                    StockFundamentalsResp stockFundamentals = await this.screenerClient.GetStockFundamentals(stock.TradingSymbol, stock.Name);
                    if (stockFundamentals == null)
                    {
                        this.logger.LogError($"Stock fundamentals not found for {stock.Name} : {stock.TradingSymbol}");
                        await this.stockRepository.DeleteStock(stock.Id);
                        continue;
                    }
                    this.logger.LogInformation($"Saving stock Fundamentals for stock {stock.TradingSymbol} - {JsonUtil.SerializeObject(stockFundamentals)}");
                    await this.stockFundamentalsRepository.UpsertStockFundamentals(stockFundamentals, stock.TradingSymbol, stock.Name);
                } catch (Exception)
                {
                    this.logger.LogError("Found exception increasing time period");
                    timePeriod += 200;
                }
            }
            return Ok();
        }

        [HttpGet]
        [Route("removeDuplicates")]
        public async Task<IActionResult> RemoveDuplicates()
        {
            var allStocks = await this.stockRepository.GetAllStocks();
            Dictionary<string, List<Stock>> stockDictionary = new Dictionary<string, List<Stock>>();
            foreach (Stock st in allStocks)
            {
                if (stockDictionary.ContainsKey(st.TradingSymbol))
                {
                    stockDictionary[st.TradingSymbol].Add(st);
                } else
                {
                    stockDictionary[st.TradingSymbol] = [st];
                }
            }
            foreach(var kv in stockDictionary)
            {
                if (kv.Value.Count > 1)
                {
                    logger.LogError($"Duplicate stock found for {kv.Key}");
                    bool deleted = false;
                    foreach(Stock duplicate in kv.Value)
                    {
                        if (duplicate.Id.StartsWith("BSE"))
                        {
                            logger.LogError($"Delete duplicate stock {duplicate.Id}");
                            await this.stockRepository.DeleteStock(duplicate.Id);
                            deleted = true;
                            break;
                        }
                    }
                    if (!deleted)
                    {
                        Stock last = kv.Value[kv.Value.Count - 1];
                        logger.LogError($"Delete duplicate stock {last.Id}");
                        await this.stockRepository.DeleteStock(last.Id);
                    }
                }
            }
            return Ok();
        }
    }
}
