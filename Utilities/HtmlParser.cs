using HtmlAgilityPack;

namespace TheMarauderMap.Utilities
{
    public static class HtmlParser
    {
        public static List<int> ExtractSection(string url, string sectionId)
        {
            List<int> sectionValues = new List<int>();
            var Webget = new HtmlWeb();
            HtmlDocument doc = Webget.Load(url);
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//html//body//main//section");
            foreach(var node in nodeCollection)
            {
                if (node.GetAttributeValue("id", "none").Equals(sectionId))
                {
                    var childNodes = node.ChildNodes;
                    foreach(var divNode in childNodes)
                    {
                        if (divNode.GetAttributeValue("class", "none").Equals("responsive-holder fill-card-width"))
                        {
                            var divChildNodes = divNode.ChildNodes;
                            foreach(var dc in divChildNodes)
                            {
                                if (dc.Name.Equals("table"))
                                {
                                    Console.Write(dc);
                                    var stripeNodes = dc.SelectSingleNode("//tbody//tr").ChildNodes;
                                    foreach(var stripeNode in stripeNodes)
                                    {
                                        if (stripeNode.Name.Equals("td") && !stripeNode.GetAttributeValue("class", "").Equals("text"))
                                        {
                                            int data = int.Parse(stripeNode.InnerText);
                                            sectionValues.Add(data);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return sectionValues;
        }
    }
}
