using Newtonsoft.Json;
using Support;
using System;
using System.Net;
using WbSales.JsonToSharpClasses;

namespace WbSales
{
    public class ProductByVendorCodeHandler
    {
        public static ProductByVendorCode TestVendorCode(string venderCode)
        {
            ProductByVendorCode productByVendorCode = null;
            try
            {
                if (string.IsNullOrEmpty(venderCode))
                {
                    return null;
                }
                //venderCode = venderCode.Trim();
                Console.Write($"Start {typeof(ProductByVendorCodeHandler)}");
                //string vendorCode = "53489278";
                bool requestSuccess = true;
                while (requestSuccess)
                {
                    var getRequest = new GetRequest($"https://card.wb.ru/cards/detail?spp=23&regions=68,64,83,4,38,80,33,70,82,86,30,69,22,66,31,40,1,48&pricemarginCoeff=1.0&reg=1&appType=1&emp=0&locale=ru&lang=ru&curr=rub&couponsGeo=12,7,3,6,18,22,21&dest=-1075831,-79374,-367666,-2133466&nm={venderCode}");
                    getRequest.Accept = "*/*";
                    getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
                    getRequest.Referer = $"https://www.wildberries.ru/catalog/{venderCode}/detail.aspx?targetUrl=EX";
                    getRequest.Host = "card.wb.ru";
                    getRequest.Keepalive = true;

                    //getRequest.Headers.Add("Accept-Encoding", "gzip");
                    getRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                    getRequest.Headers.Add("Origin", "https://www.wildberries.ru");
                    getRequest.Headers.Add("sec-ch-ua", "\"Google Chrome\";v=\"105\", \"Not)A; Brand\";v=\"8\", \"Chromium\";v=\"105\"");
                    getRequest.Headers.Add("sec-ch-ua-mobile", "?0");
                    getRequest.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                    getRequest.Headers.Add("Sec-Fetch-Dest", "empty");
                    getRequest.Headers.Add("Sec-Fetch-Mode", "cors");
                    getRequest.Headers.Add("Sec-Fetch-Site", "cross-site");
                    var cookieContainer = new CookieContainer();

                    string response = getRequest.Run(cookieContainer);

                    if (string.IsNullOrEmpty(response))
                    {
                        requestSuccess = false;
                        Console.WriteLine(" ended or failed");
                    }
                    else
                    {
                        productByVendorCode = JsonConvert.DeserializeObject<ProductByVendorCode>(response);
                        Console.WriteLine(" success");
                        return productByVendorCode;
                    }
                }
            }
            catch (AggregateException err)
            {
                Console.WriteLine(err.ToString());
                //foreach (var errInner in err.InnerExceptions)
                //{
                //    Console.WriteLine(errInner);
                //    //this will call ToString() on the inner execption and get you message, stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                //}
            }
            catch
            {
                throw;
            }

            return productByVendorCode;
        }
    }
}
