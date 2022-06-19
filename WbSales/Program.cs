using HtmlAgilityPack;
using Newtonsoft.Json;
using Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace WbSales
{
    class Program
    {
        private static int _page;
        private static TimeSpan _sleepTime;
        private static string _imgUrl;
        public static string _xmlPath;
        private static List<Root> _deserializedProducts;
        private static XmlDocument _xDoc;
        private static bool _isXmlDocExists;

        private static void OnStart()
        {
            _page = 1;
            _sleepTime = TimeSpan.FromMinutes(5);
            _imgUrl = "https://images.wbstatic.net/c516x688/new/urlNum/productId-1.jpg";
            _xmlPath = Directory.GetCurrentDirectory() + "\\TgProducts.xml";
            _isXmlDocExists = File.Exists(_xmlPath);
            if (_isXmlDocExists)
            {
                _xDoc = new();
                _xDoc.Load(_xmlPath);
            }
        }

        static void Main(string[] args)
        {
            OnStart();
            while (true)
            {
                try
                {
                    Console.Beep();
                    Console.WriteLine("Start bot {0:MM/dd/yy H:mm:ss zzz}\n", DateTime.Now);
                    _deserializedProducts = GetJsonPages();
                    FilterProducts(_deserializedProducts);
                    if (_isXmlDocExists)
                    {
                        RemoveMissingProductFromDoc(_deserializedProducts);
                    }
                    else
                    {
                        CreateXmlDoc();
                    }
                    SendProducts(_deserializedProducts);
                    SaveXmlDoc(_deserializedProducts);
                    Console.WriteLine("End {0:MM/dd/yy H:mm:ss zzz}\n", DateTime.Now);
                    _page = 1;
                    Console.WriteLine($"Sleep to {_sleepTime} minutes");
                    Thread.Sleep(_sleepTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Message: " + ex);
                }
                finally
                {
                    var sleepTime = TimeSpan.FromMinutes(5);
                    Console.WriteLine($"Sleep to {sleepTime}");
                }
            }
        }

        private static void SaveXmlDoc(List<Root> deserializedProducts)
        {
            if (deserializedProducts?.Count > 0)
            {
                XmlElement xRoot = _xDoc.DocumentElement;
                foreach (var productsPage in deserializedProducts)
                {
                    if (productsPage.data?.products?.Count > 0)
                    {
                        foreach (var product in productsPage.data.products)
                        {
                            //add product
                            XmlElement productEl = _xDoc.CreateElement(string.Empty, "product", string.Empty);
                            xRoot.AppendChild(productEl);
                            //add id
                            XmlElement id = _xDoc.CreateElement(string.Empty, "id", string.Empty);
                            XmlText idText = _xDoc.CreateTextNode(product.id.ToString());
                            id.AppendChild(idText);
                            productEl.AppendChild(id);
                            //add name
                            XmlElement name = _xDoc.CreateElement(string.Empty, "name", string.Empty);
                            XmlText nameText = _xDoc.CreateTextNode(product.name + " " + product.brand);
                            name.AppendChild(nameText);
                            productEl.AppendChild(name);
                            //add price
                            XmlElement price = _xDoc.CreateElement(string.Empty, "price", string.Empty);
                            XmlText priceText = _xDoc.CreateTextNode(product.salePriceU.ToString());
                            price.AppendChild(priceText);
                            productEl.AppendChild(price);
                        }
                    }
                }
                _xDoc.Save(_xmlPath);
            }
        }

        private static void FilterProducts(List<Root> deserializedProducts)
        {
            //TODO: Create Filter from JSON
            Filter filter = new()
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

            deserializedProducts
                .ForEach(x => x.data.products
                    .RemoveAll(product => product.SalePrice() > filter.Price
                    || !filter.Sizes.Any(filterSize => product.sizes.Any(pSize => pSize.name == filterSize))
                    || !filter.Brands.Any(brand => brand == product.brand)));
        }

        private static void CreateXmlDoc()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmldeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmldeclaration, root);
            XmlElement xProducts = doc.CreateElement(string.Empty, "products", string.Empty);
            doc.AppendChild(xProducts);
            _xDoc = doc;
        }

        private static void RemoveMissingProductFromDoc(List<Root> deserializedProducts)
        {
            if (_deserializedProducts?.Count > 0)
            {
                XmlElement xRoot = _xDoc.DocumentElement;
                List<XmlElement> elementsForRemove = new();
                foreach (XmlElement element in xRoot)
                {
                    string id = element["id"].InnerText;
                    string price = element["price"].InnerText;
                    try
                    {
                        // удаление товара, если он отсутствует или изменилась его цена.
                        //if (deserializedProducts.Any(x => x.data?.products?.Any(x => x.id.ToString() == id) ?? false))
                        //if (deserializedProducts.Any(x => x.data.products.Any(y => y.id.ToString() == id && y.salePriceU.ToString() == price)))

                        Product findedProduct = null;
                        foreach (var product in deserializedProducts)
                        {
                            findedProduct = product.data.products.FirstOrDefault(y => y.id.ToString() == id && y.salePriceU.ToString() == price);
                            if(findedProduct != null)
                            {
                                break;
                            }
                        }

                        if (findedProduct != null)
                        {
                            //удаляем товар из входящего списка, если такой уже есть в файле
                            deserializedProducts.ForEach(x => x.data.products.Remove(findedProduct));
                        }
                        else
                        {
                            elementsForRemove.Add(element);
                        }
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }

                elementsForRemove.ForEach(x => xRoot.RemoveChild(x));
            }
        }

        private static void SendProducts(List<Root> deserializedProducts)
        {
            if (deserializedProducts?.Count > 0)
            {
                foreach (var productsPage in deserializedProducts)
                {
                    if (productsPage.data?.products?.Count > 0)
                    {
                        foreach (var product in productsPage.data.products)
                        {
                            string imgUrl = GetProductImgById(product);
                            Console.WriteLine("Запуск бота");
                            var tgMessageSender = new TgMessageSender(product, imgUrl);
                            tgMessageSender.SendMsg();
                        }
                    }
                }
            }
        }

        private static List<Root> GetJsonPages()
        {
            try
            {
                Console.WriteLine($"Start {typeof(GetRequest)}\n");
                bool requestSuccess = true;
                List<Root> productList = new();
                while (requestSuccess)
                {
                    Console.Write($"Start get page {_page}");
                    var getRequest = new GetRequest($"https://wbxcatalog-ru.wildberries.ru/men_shoes/catalog?appType=1&couponsGeo=12,3,18,15,21&curr=rub&dest=-1029256,-102269,-1278703,-1255563&emp=0&kind=1&lang=ru&locale=ru&page={_page}&pricemarginCoeff=1.0&reg=0&regions=68,64,83,4,38,80,33,70,82,86,75,30,69,48,22,1,66,31,40,71&sort=popular&spp=0&stores=117673,122258,122259,125238,125239,125240,6159,507,3158,117501,120602,120762,6158,121709,124731,159402,2737,130744,117986,1733,686,132043&subject=104;105;128;130;232;396;1382;1586");
                    getRequest.Accept = "*/*";
                    getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.54 Safari/537.36";
                    getRequest.Referer = $"https://www.wildberries.ru/catalog/obuv/muzhskaya/kedy-i-krossovki?sort=popular&page={_page}";
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
                    if (string.IsNullOrEmpty(response))
                    {
                        requestSuccess = false;
                        Console.WriteLine(" ended or failed");
                    }
                    else
                    {
                        Console.WriteLine(" success");
                        Root products = JsonConvert.DeserializeObject<Root>(response);
                        if (products != null && products.data?.products?.Count > 0)
                        {
                            productList.Add(products);
                        }
                        _page++;
                    }
                }
                return productList;
            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    Console.WriteLine(errInner);
                    //this will call ToString() on the inner execption and get you message, stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        private static bool CheckSavedProduct(Product product)
        {
            string path = Program._xmlPath;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                foreach (XmlElement element in xRoot)
                {
                    string id = element["id"].InnerText;
                    string price = element["price"].InnerText;
                    if (id == product.id.ToString())
                    {
                        if (price == product.salePriceU.ToString())
                            return true;
                        else
                        {
                            xRoot.RemoveChild(element);
                            xDoc.Save(path);
                        }
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
            var getRequest = new GetRequest($"https://www.wildberries.ru/catalog/obuv/muzhskaya/kedy-i-krossovki?sort=popular&cardsize=c516x688&page={_page}&fsize=37404;56157;56158;38386");
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
                        if (newPrice <= 10_000)
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
