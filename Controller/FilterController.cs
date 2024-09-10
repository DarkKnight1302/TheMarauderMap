using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            using (StreamReader r = new StreamReader("StockName.json"))
            {
                string json = r.ReadToEnd();
                List<Instrument> items = JsonConvert.DeserializeObject<List<Instrument>>(json);
                filteredList = items.Where(x => (x.Segment.Equals("NSE_EQ") && x.InstrumentType.Equals("EQ"))).ToList();
            }
            List<Stock> StocksToAdd = new List<Stock>();
            foreach (Instrument item in filteredList)
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
                }
            }
            await this.stockRepository.UpdateStockPrice(StocksToAdd);
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
