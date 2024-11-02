using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using TheMarauderMap.Entities;
using TheMarauderMap.Repositories;
using Instrument = TheMarauderMap.Responses.Instrument;

namespace TheMarauderMap.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly ILogger<FilterController> logger;
        private readonly IStockRepository stockRepository;

        public FilterController(ILogger<FilterController> logger, IStockRepository stockRepository)
        {
            this.logger = logger;
            this.stockRepository = stockRepository;
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
    }
}
