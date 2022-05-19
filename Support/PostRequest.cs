using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Support
{
    public class PostRequest
    {
        HttpWebRequest _request;
        string _adress;

        public string Response { get; set; }
        public string Accept { get; set; }
        public string Host { get; set; }
        public string ContentType { get; set; }
        public string Data { get; set; }
        public string Useragent { get; set; }
        public string Referer { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public PostRequest(string address)
        {
            this._adress = address;
            this.Headers = new Dictionary<string, string>(); 
        }

        public void Run()
        {
            _request = (HttpWebRequest)WebRequest.Create(_adress);
            _request.Method = "Post";

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

        public void Run(CookieContainer cookieContainer)
        {
            _request = (HttpWebRequest)WebRequest.Create(_adress);
            _request.Method = "Post";
            _request.CookieContainer = cookieContainer;
            _request.Accept = this.Accept;
            _request.Host = this.Host;
            _request.ContentType = this.ContentType;
            _request.Referer = this.Referer;
            _request.UserAgent = this.Useragent;

            byte[] sendData = Encoding.UTF8.GetBytes(Data);
            _request.ContentLength = sendData.Length;
            Stream sendStream = _request.GetRequestStream();
            sendStream.Write(sendData, 0, sendData.Length);
            sendStream.Close();

            foreach(var header in this.Headers)
            {
                this._request.Headers.Add(header.Key, header.Value);
            }

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
    }
}
