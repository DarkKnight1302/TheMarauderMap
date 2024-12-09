using HtmlAgilityPack;
using System.Security.Policy;
using TheMarauderMap.Utilities;

namespace MarauderMapTest
{
    [TestClass]
    public class UnitTest1
    {
        HtmlDocument htmlDocument = null;

        [TestInitialize]
        public void Init()
        {
            var Webget = new HtmlWeb();
            this.htmlDocument = Webget.Load("https://www.screener.in/company/DSSL/");
        }

        [TestMethod]
        public void TestValidStock()
        {
            var Webget = new HtmlWeb();
            var invalid = Webget.Load("https://www.screener.in/company/DSSI/");
            Assert.IsFalse(HtmlParser.IsValidStockDoc(invalid));
            Assert.IsTrue(HtmlParser.IsValidStockDoc(this.htmlDocument));
        }

        [TestMethod]
        public void TestHtmlParser()
        {
            List<int> values = HtmlParser.ExtractSectionForRevenue(this.htmlDocument, "quarters");
            List<int> expected = new List<int> { 139, 160, 209, 160, 246, 172, 226, 296, 220, 227, 282, 321, 306 };
            CollectionAssert.AreEqual(expected, values);
        }

        [TestMethod]
        public void TestHtmlParser1()
        {
            List<int> values = HtmlParser.ExtractSectionForProfit(htmlDocument, "quarters");
            List<int> expected = new List<int> { 3, 3, 6, 5, 9, 8, 12, 14, 13, 13, 14, 18, 18 };
            CollectionAssert.AreEqual(expected, values);
        }

        [TestMethod]
        public void TestHtmlParser2()
        {
            List<int> values = HtmlParser.ExtractSectionForRevenue(htmlDocument, "profit-loss");
            List<int> expected = new List<int> { 59, 77, 105, 124, 158, 211, 304, 328, 436, 654, 804, 1024, 1136 };
            CollectionAssert.AreEqual(expected, values);
        }

        [TestMethod]
        public void TestHtmlParser3()
        {
            List<int> values = HtmlParser.ExtractSectionForProfit(htmlDocument, "profit-loss");
            List<int> expected = new List<int> { 1, 1, 1, 1, 1, 2, 4, 6, 9, 16, 33, 54, 63 };
            CollectionAssert.AreEqual(expected, values);
        }
    }
}