using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Support
{
    public class GetRequest
    {
        HttpWebRequest _request;
        string _adress;

        public string Response { get; set; }
        public bool Keepalive { get; set; }
        public string Accept { get; set; }
        public string Host { get; set; }
        public string Useragent { get; set; }
        public string Referer { get; set; }
        public WebProxy Proxy { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Connection { get; set; }
        public int Timeout { get; set; }

        public GetRequest(string address)
        {
            this._adress = address;
            this.Headers = new Dictionary<string, string>();
        }

        public void Run()
        {
            _request = (HttpWebRequest)WebRequest.Create(_adress);
            _request.Method = "Get";

            try
            {
                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null) Response = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception)
            {

            }
        }

        public string Run(CookieContainer cookieContainer)
        {
            _request = (HttpWebRequest)WebRequest.Create(_adress);
            _request.Method = "Get";
            _request.CookieContainer = cookieContainer;
            if (!string.IsNullOrEmpty(Accept))
                _request.Accept = this.Accept;
            if (!string.IsNullOrEmpty(Host))
                _request.Host = this.Host;
            if (!string.IsNullOrEmpty(Referer))
                _request.Referer = this.Referer;
            if (!string.IsNullOrEmpty(Useragent))
                _request.UserAgent = this.Useragent;
            _request.KeepAlive = this.Keepalive;
            if (this.Timeout > 0)
                _request.Timeout = this.Timeout;

            foreach (var header in this.Headers)
            {
                this._request.Headers.Add(header.Key, header.Value);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    using StreamReader sr = new(stream);
                    this.Response = sr.ReadToEnd();
                    return this.Response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }
    }
}
