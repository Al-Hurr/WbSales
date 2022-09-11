using Newtonsoft.Json;
using Support;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using WbSales.JsonToSharpClasses;

namespace WbSales
{
    class Program
    {
        private static int _page;
        private static TimeSpan _sleepTime;
        private static string _imgUrl;
        public static string _xmlPath;
        private static XmlDocument _xDoc;
        private static Filter _filter;
        private static bool _isXmlDocExists;

        private static void OnStart()
        {
            _page = 1;
            _sleepTime = TimeSpan.FromMinutes(30);
            _imgUrl = "https://images.wbstatic.net/c516x688/new/urlNum/productId-1.jpg";
            _xmlPath = Directory.GetCurrentDirectory() + "\\TgProducts.xml";
            //_xmlPath = Directory.GetCurrentDirectory() + "\\TgProducts_test.xml";
            _isXmlDocExists = File.Exists(_xmlPath);
            if (_isXmlDocExists)
            {
                _xDoc = new();
                _xDoc.Load(_xmlPath);
            }
            TgMessageSender.StartListening();
        }

        static void Main(string[] args)
        {
            OnStart();
            List<Root> deserializedProducts;
            while (true) ;
            while (false)
            {
                try
                {
                    //Console.Beep();
                    Console.WriteLine("Start bot {0:MM/dd/yy H:mm:ss zzz}\n", DateTime.Now);
                    deserializedProducts = GetJsonPages();
                    //_deserializedProducts = GetJsonPagesTest();
                    FilterProducts(deserializedProducts);
                    if (_isXmlDocExists)
                    {
                        RemoveMissingProductFromDoc(deserializedProducts);
                    }
                    else
                    {
                        CreateXmlDoc();
                    }
                    SendProducts(deserializedProducts);
                    SaveXmlDoc(deserializedProducts);
                    Console.WriteLine("End {0:MM/dd/yy H:mm:ss zzz}\n", DateTime.Now);
                    Console.WriteLine($"Sleep to {_sleepTime} minutes");
                    Thread.Sleep(_sleepTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Message: " + ex);
                    Console.WriteLine($"Sleep to 10 seconds");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }

        #region Get Json Pages Test
        private static List<Root> GetJsonPagesTest()
        {
            //TODO: 2 листа, в каждом листе 5 продуктов
            return new List<Root>
            {
                new Root
                {
                    data = new Data
                    {
                        products = new List<Product>
                        {
                            new Product
                            {
                                id = 1,
                                brand = "brand1",
                                name = "name1",
                                priceU = 299_900,
                                salePriceU = 231_900,
                            },
                            new Product
                            {
                                id = 2,
                                brand = "brand2",
                                name = "name2",
                                priceU = 299_900,
                                salePriceU = 131_900,
                            },
                            new Product
                            {
                                id = 3,
                                brand = "brand3",
                                name = "name3",
                                priceU = 499_900,
                                salePriceU = 1_131_900,
                            },
                            new Product
                            {
                                id = 4,
                                brand = "brand4",
                                name = "name4",
                                priceU = 599_900,
                                salePriceU = 1_100_900,
                            },
                            new Product
                            {
                                id = 5,
                                brand = "brand5",
                                name = "name5",
                                priceU = 255_555,
                                salePriceU = 400_900,
                            },
                        }
                    }
                },
                new Root
                {
                    data = new Data
                    {
                        products = new List<Product>
                        {
                            new Product
                            {
                                id = 6,
                                brand = "brand6",
                                name = "name6",
                                priceU = 700_000,
                                salePriceU = 600_000,
                            },
                            new Product
                            {
                                id = 7,
                                brand = "brand7",
                                name = "name7",
                                priceU = 550_000,
                                salePriceU = 500_900,
                            },
                            new Product
                            {
                                id = 8,
                                brand = "brand8",
                                name = "name8",
                                priceU = 190_000,
                                salePriceU = 131_900,
                            },
                            new Product
                            {
                                id = 9,
                                brand = "brand9",
                                name = "name9",
                                priceU = 666_666,
                                salePriceU = 777_777,
                            },
                            new Product
                            {
                                id = 12,
                                brand = "brand12",
                                name = "name12",
                                priceU = 122_222,
                                salePriceU = 333_333,
                            },
                        }
                    }
                }
            };
        }
        #endregion

        #region Save Xml Doc
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
                _isXmlDocExists = true;
            }
        }
        #endregion

        #region Filter Products
        private static void FilterProducts(List<Root> deserializedProducts)
        {
            try
            {
                //TODO: Create Filter from JSON
                Console.Write($"Start FilterProducts");

                if (_filter == null)
                {
                    _filter = new()
                    {
                        PriceLow = int.Parse(ConfigurationManager.AppSettings.Get("PriceLow")),
                        PriceHight = int.Parse(ConfigurationManager.AppSettings.Get("PriceHight")),
                        Brands = ConfigurationManager.AppSettings.Get("Brands").Split('/').ToList<string>(),
                        Sizes = ConfigurationManager.AppSettings.Get("Sizes").Split('/').ToList<string>()
                    };
                }

                deserializedProducts
                    .ForEach(x => x.data.products
                        .RemoveAll(product =>
                        (product.SalePrice < _filter.PriceLow || product.SalePrice > _filter.PriceHight)
                        || !_filter.Sizes.Any(filterSize => product.sizes.Any(pSize => pSize.name == filterSize))
                        || !_filter.Brands.Any(brand => brand == product.brand)));

                Console.WriteLine($" success");
                Console.WriteLine($"Количество товаров после фильтрации: {deserializedProducts.Sum(x => x.data.products.Count)}");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Create Xml Doc
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
        #endregion

        #region Remove Missing Product From Doc
        private static void RemoveMissingProductFromDoc(List<Root> deserializedProducts)
        {
            Console.WriteLine($"Удаление законченных товаров из файла");
            Console.WriteLine($"Удаление имеющихся товаров из входящего списка");
            if (deserializedProducts?.Count > 0)
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
                        Product findedProduct = null;
                        foreach (var product in deserializedProducts)
                        {
                            findedProduct = product.data.products.FirstOrDefault(y => y.id.ToString() == id && y.salePriceU.ToString() == price);
                            if (findedProduct != null)
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
                Console.WriteLine($"Количество удаленных товаров из файла: {elementsForRemove.Count}");
                Console.WriteLine($"Количество товаров после проверки на наличие в файле: {deserializedProducts.Sum(x => x.data.products.Count)}");
            }
        }
        #endregion

        #region Send Products
        private static void SendProducts(List<Root> deserializedProducts)
        {
            Console.WriteLine("Отправка товаров");
            if (deserializedProducts?.Count > 0)
            {
                bool isSendStarted = false;
                foreach (var productsPage in deserializedProducts)
                {
                    if (productsPage.data?.products?.Count > 0)
                    {
                        if (!isSendStarted)
                        {
                            isSendStarted = TgMessageSender.SendTextMsg();
                        }
                        foreach (var product in productsPage.data.products)
                        {
                            string imgUrl = GetProductImgById(product);
                            var tgMessageSender = new TgMessageSender(product, imgUrl);
                            tgMessageSender.SendMsg();
                        }
                    }
                }
            }

            Console.WriteLine("Отправка товаров success");
        }
        #endregion

        #region Get Json Pages
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
                        Root products = JsonConvert.DeserializeObject<Root>(response);
                        if (products != null && products.data?.products?.Count > 0)
                        {
                            productList.Add(products);
                            // Для теста!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            return productList;
                        }
                        _page++;
                        Console.WriteLine(" success");
                    }
                }

                Console.WriteLine($"Всего товаров: {productList.Sum(x => x.data.products.Count)}");
                Console.WriteLine($"Всего страниц: {productList.Count}");
                _page = 1;
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
        #endregion

        #region Get Product Image From Id
        public static string GetProductImgById(Product product)
        {
            string productId = product.id.ToString();
            Regex regex = new("[0-9]");
            string urlNum = regex.Replace(productId, "0", 4, productId.Length - 4);
            return _imgUrl.Replace("urlNum", urlNum).Replace("productId", productId);
        }
        #endregion
    }
}
