using HtmlAgilityPack;

namespace TheMarauderMap.Utilities
{
    public static class HtmlParser
    {
        public static bool IsValidStockDoc(HtmlDocument doc)
        {
            var title = doc.DocumentNode.SelectSingleNode("//html//head//title");
            if (title.InnerText.Equals("Error 404: Page Not Found - Screener"))
            {
                return false;
            }
            return true;
        }

        public static List<int> ExtractSectionForRevenue(HtmlDocument doc, string sectionId)
        {
            List<int> sectionValues = new List<int>();
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
                                    var bodyNode = dc.Element("tbody");
                                    var stripeNodes = bodyNode.Element("tr").ChildNodes;
                                    foreach(var stripeNode in stripeNodes)
                                    {
                                        if (stripeNode.Name.Equals("td") && !stripeNode.GetAttributeValue("class", "").Equals("text"))
                                        {
                                            int data = NumberUtility.CleanNumberString(stripeNode.InnerText);
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

        public static List<int> ExtractSectionForProfit(HtmlDocument doc, string sectionId)
        {
            List<int> sectionValues = new List<int>();
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//html//body//main//section");
            foreach (var node in nodeCollection)
            {
                if (node.GetAttributeValue("id", "none").Equals(sectionId))
                {
                    var childNodes = node.ChildNodes;
                    foreach (var divNode in childNodes)
                    {
                        if (divNode.GetAttributeValue("class", "none").Equals("responsive-holder fill-card-width"))
                        {
                            var divChildNodes = divNode.ChildNodes;
                            foreach (var dc in divChildNodes)
                            {
                                if (dc.Name.Equals("table"))
                                {
                                    var bodyNode = dc.Element("tbody");
                                    var stripeNodes = bodyNode.Elements("tr");
                                    var profitNode = stripeNodes.ElementAt(9);
                                    var profitChildNodes = profitNode.ChildNodes;
                                    foreach(var profitChild in profitChildNodes)
                                    {
                                        if (profitChild.Name.Equals("td") && !profitChild.GetAttributeValue("class", "").Equals("text"))
                                        {
                                            int data = NumberUtility.CleanNumberString(profitChild.InnerText);
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
