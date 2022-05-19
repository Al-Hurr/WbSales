using HtmlAgilityPack;
using Newtonsoft.Json;
using Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace WbSales
{
    class Program
    {
        private const float _maxPrice = 4500;
        private const string _imgUrl = "https://images.wbstatic.net/c516x688/new/urlNum/productId-1.jpg";
        public static readonly string _xmlPath = Directory.GetCurrentDirectory() + "\\TgProducts.xml";
        static void Main(string[] args)
        {
            try
            {
                //LoadHtml();

                LoadJson();

                Console.WriteLine("End");
                Console.ReadLine();
            }
            catch
            {
                throw;
            }
        }

        private static void LoadJson()
        {
            var getRequest = new GetRequest("https://wbxcatalog-ru.wildberries.ru/men_shoes/catalog?appType=1&couponsGeo=12,3,18,15,21&curr=rub&dest=-1029256,-102269,-1278703,-1255563&emp=0&kind=1&lang=ru&locale=ru&pricemarginCoeff=1.0&reg=0&regions=68,64,83,4,38,80,33,70,82,86,75,30,69,48,22,1,66,31,40,71&spp=0&stores=117673,122258,122259,125238,125239,125240,6159,507,3158,117501,120602,120762,6158,121709,124731,159402,2737,130744,117986,1733,686,132043&subject=104;105;128;130;232;396;1382;1586");
            getRequest.Accept = "*/*";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.54 Safari/537.36";
            getRequest.Referer = "https://www.wildberries.ru/catalog/obuv/muzhskaya/kedy-i-krossovki";
            getRequest.Host = "wbxcatalog-ru.wildberries.ru";
            getRequest.Keepalive = true;

            //getRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            getRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9");
            getRequest.Headers.Add("Origin", "https://www.wildberries.ru");
            getRequest.Headers.Add("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"101\", \"Google Chrome\";v=\"101\"");
            getRequest.Headers.Add("sec-ch-ua-mobile", "?0");
            getRequest.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            getRequest.Headers.Add("Sec-Fetch-Dest", "empty");
            getRequest.Headers.Add("Sec-Fetch-Mode", "cors");
            getRequest.Headers.Add("Sec-Fetch-Site", "same-site");
            var cookieContainer = new CookieContainer();

            string response = getRequest.Run(cookieContainer);

            Root products = JsonConvert.DeserializeObject<Root>(response);

            //TODO: Create Filter from JSON
            Filter filter = new Filter
            {
                Price = 4500,
                Brands = new List<string>
                    {
                        "New balance" ,
                        "adidas",
                        "TIMBERLAND",
                        "ASICS",
                        "Saucony Originals",
                        "Under Armour",
                        "Reebok",
                        "Nike",
                        "PUMA",
                        "FILA",
                        "Tommy Hilfiger"
                    },
                Sizes = new List<string>
                    {
                        "41,5", "42", "42,5", "43"
                    }
            };
            foreach (var product in products.data.products)
            {
                if (product.salePriceU / 100 <= filter.Price 
                    && filter.Sizes.Any(x => product.sizes.Any(y => y.name == x))
                    && filter.Brands.Any(x => product.brand == x))
                {
                    bool isXmlDocExists = File.Exists(_xmlPath);
                    if (!isXmlDocExists)
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        XmlElement root = doc.DocumentElement;
                        doc.InsertBefore(xmlDeclaration, root);
                        XmlElement xProducts = doc.CreateElement(string.Empty, "products", string.Empty);
                        doc.AppendChild(xProducts);
                    }

                    if (!isXmlDocExists || !IsProductExists(product))
                    {
                        string imgUrl = GetProductImgById(product);
                        var tgMessageSender = new TgMessageSender(product, imgUrl);
                        tgMessageSender.SendMsg();
                    }
                }
            }
        }

        private static bool IsProductExists(Product product)
        {
            string path = Program._xmlPath;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlElement xRoot = xDoc.DocumentElement;
            if(xRoot != null)
            {
                foreach (XmlElement element in xRoot)
                {
                    string id = element["id"].InnerText;
                    string price = element["price"].InnerText;
                    if(id == product.id.ToString() && price == product.salePriceU.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string GetProductImgById(Product product)
        {
            string productId = product.id.ToString();
            Regex regex = new Regex("[0-9]");
            string urlNum = regex.Replace(productId, "0", 4, productId.Length - 4);
            return _imgUrl.Replace("urlNum", urlNum).Replace("productId", productId);
        }

        private static void LoadHtml()
        {
            var getRequest = new GetRequest("https://www.wildberries.ru/catalog/obuv/muzhskaya/kedy-i-krossovki?sort=popular&cardsize=c516x688&page=1&fsize=37404;56157;56158;38386");
            getRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.54 Safari/537.36";
            getRequest.Referer = "https://www.wildberries.ru/";
            //getRequest.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Host = "www.wildberries.ru";
            getRequest.Keepalive = true;
            getRequest.Timeout = 10000;

            //getRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            getRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9");
            getRequest.Headers.Add("Origin", "https://www.wildberries.ru");
            getRequest.Headers.Add("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"101\", \"Google Chrome\";v=\"101\"");
            getRequest.Headers.Add("sec-ch-ua-mobile", "?0");
            getRequest.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            getRequest.Headers.Add("Sec-Fetch-Dest", "document");
            getRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            getRequest.Headers.Add("Sec-Fetch-Site", "same-origin");
            getRequest.Headers.Add("Sec-Fetch-User", "?1");
            getRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
            var cookieContainer = new CookieContainer();

            string response = getRequest.Run(cookieContainer);

            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(response);
            HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='product-card j-card-item']");

            if (htmlNodes != null && htmlNodes.Count > 0)
            {
                List<ParsedProduct> products = new(htmlNodes.Count);

                foreach (HtmlNode node in htmlNodes)
                {
                    string newPriceText = node.SelectNodes("//ins[@class='lower-price']")[0].InnerText;
                    //string newPriceText = node.;
                    if (float.TryParse(newPriceText.Replace("₽", "").Replace(" ", ""), out float newPrice))
                    {
                        if (newPrice <= _maxPrice)
                        {
                            ParsedProduct product = new ParsedProduct();
                            product.NewPrice = newPrice;
                            product.Name = $"{node.SelectNodes("//strong[@class='brand-name']")[0].InnerText} {node.SelectSingleNode("//span[@class='goods-name']").InnerText}";
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Parse string to float failed. String text: {newPriceText}");
                    }
                }
            }
        }
    }
}
