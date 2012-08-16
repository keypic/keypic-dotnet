using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using System.Xml;
using System.Web;

namespace KeypicLib
{
    public class KeypicClient : IKeypicClient
    {
        static IKeypicClient instance = null;

        string host = null;

        bool debug = false;

        string formID = null;
        string token = null;
        string lastRawDataRecived = "";
        string lasteError = "";
        bool isErrorExist;

        const ResponseType responseType = ResponseType.XML;
        const string contentType = "multipart/form-data;";
        const string boundary = "BaNN04x";
        const string accept = "iso-8859-1";

        private KeypicClient()
        {
            Object section = ConfigurationManager.GetSection("keypicConfiguration");

            if (section != null) {
                KeypicConfiguration config = (KeypicConfiguration)section;

                host = config.Host;
                debug = config.Debug;
                formID = config.FormID;
            }
        }

        public static IKeypicClient GetInstance()
        {
            return instance != null ? instance : new KeypicClient();
        }

        public string Version
        {
            get
            {
                return this.GetType().Assembly.GetName().Version.ToString();
            }
        }

        public string UserAgent
        {
            get
            {
                // Get the common language runtime version.
                Version clrVer = Environment.Version;

                return String.Format("Keypic ASP.NET {0} Component, Version: {1}", clrVer.ToString(), Version);
            }
        }

        public bool Debug
        {
            get
            {
                return debug;
            }
            set
            {
                debug = value;
            }
        }

        public string FormID
        {
            get
            {
                return formID;
            }
            set
            {
                formID = value;
            }
        }

        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
            }
        }

        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }

        public bool CheckFormID()
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("FormID", FormID);

            IDictionary<string, string> result = Invoke("checkFormID", parameters);

            if (isErrorExist == false && result["status"] == "response" && result["report"] == "OK") {
                return true;
            }

            return false;
        }

        public string GetToken(string clientEmailAddress, string clientUsername, string clientMessage, string clientFingerprint, int quantity)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("FormID", formID);
            parameters.Add("Quantity", quantity.ToString());
            parameters.Add("ClientUsername", clientUsername);
            parameters.Add("ClientEmailAddress", clientEmailAddress);
            parameters.Add("ClientMessage", clientMessage);
            parameters.Add("ClientFingerprint", clientFingerprint);

            AddServerVariablesValue(parameters);

            IDictionary<string, string> result = Invoke("RequestNewToken", parameters);

            if (isErrorExist == false && result["status"] == "new_token") {
                return result["Token"];
            }

            return null;
        }

        public string GetImage(int width, int height)
        {
            return string.Format("<a href=\"{0}?RequestType=getClick&Token={1}&target=\"_blank\">" +
                "<img src=\"{0}?RequestType=getImage&Token={1}&WeightHeight={2}x{3}&Debug={4}\" alt=\"Form protected by Keypic\" /></a>",
                host, token, width, height, debug ? 1 : 0);
        }

        public string GetFrame(int width, int height)
        {
            string url = string.Format("{0}?RequestType=getiFrame&WeightHeight={1}x{2}&Token={3}", host, width, height, token);

            return string.Format("<iframe src=\"{0}\" width=\"{1}\" height=\"{2}\" frameborder=\"0\" style=\"border: 1px solid #ffffff; background-color: #ffffff;\"" +
                "marginwidth=\"0\" marginheight=\"0\" vspace=\"0\" hspace=\"0\" allowtransparency=\"true\" scrolling=\"no\">" +
                "<p>Your browser does not support iframes.</p></iframe>",
                url, width, height);
        }

        public int IsSpam(string clientEmailAddress, string clientUsername, string clientMessage, string clientFingerprint)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("Token", token);
            parameters.Add("FormID", formID);
            parameters.Add("ClientUsername", clientUsername);
            parameters.Add("ClientEmailAddress", clientEmailAddress);
            parameters.Add("ClientMessage", clientMessage);
            parameters.Add("ClientFingerprint", clientFingerprint);

            AddServerVariablesValue(parameters);

            IDictionary<string, string> result = Invoke("RequestValidation", parameters);

            return (isErrorExist == true && result["spam"]!="") ? -1 : Convert.ToInt32(result["spam"]);
        }

        public bool ReportSpam()
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("Token", token);
            parameters.Add("FormID", formID);
            IDictionary<string, string> result = Invoke("ReportSpam", parameters);

            if (isErrorExist == false && result["status"] == "response" && result["report"] == "OK") {
                return true;
            }

            return false;
        }

        private void AddServerVariablesValue(IDictionary<string, string> parameters)
        {
            HttpRequest request = HttpContext.Current.Request;

            parameters.Add("ServerName", request.ServerVariables["HTTP_HOST"]);

            if (request.ServerVariables["HTTP_HOST"].IndexOf("localhost") == 0)
                parameters.Add("ClientIP", "127.0.0.1");
            else
                parameters.Add("ClientIP", request.ServerVariables["REMOTE_ADDR"]);

            parameters.Add("ClientUserAgent", request.ServerVariables["HTTP_USER_AGENT"]);
            parameters.Add("ClientAccept", request.ServerVariables["HTTP_ACCEPT"]);
            parameters.Add("ClientAcceptEncoding", request.ServerVariables["HTTP_ACCEPT_ENCODING"]);
            parameters.Add("ClientAcceptLanguage", request.ServerVariables["HTTP_ACCEPT_LANGUAGE"]);
            parameters.Add("ClientAcceptCharset", request.ServerVariables["HTTP_ACCEPT_CHARSET"]);
            parameters.Add("ClientHttpReferer", request.ServerVariables["HTTP_REFERER"]);
        }

        private IDictionary<string, string> Invoke(string method, IDictionary<string, string> parameters)
        {
            IDictionary<String, String> result = null;
            MemoryStream dataStream = new MemoryStream();
            HttpWebRequest webRequest;

            try {
                string url = host;

                if (debug)
                    url += url.IndexOf("?") >= 0 ? "&debug=1" : "?debug=1";

                parameters.Add("RequestType", method);
                parameters.Add("ResponseType", ((int)responseType).ToString());

                WriteRequestBody(dataStream, parameters);

                webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.AllowWriteStreamBuffering = true;
                webRequest.SendChunked = false;

                webRequest.UserAgent = UserAgent;
                webRequest.Accept = accept;
                webRequest.ContentType = contentType + (boundary.Length > 0 ? "boundary=" + boundary : "");
                webRequest.Method = "POST";
                webRequest.ContentLength = dataStream.Length;

                Stream requestStream = webRequest.GetRequestStream();
                byte[] buffer = new byte[1024];
                int readCount;
                dataStream.Position = 0;
                while ((readCount = dataStream.Read(buffer, 0, buffer.Length)) != 0) {
                    requestStream.Write(buffer, 0, readCount);
                }

                requestStream.Close();
                dataStream.Close();

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webRequest.HaveResponse) {
                    Stream responseStream = webResponse.GetResponseStream();
                    result = ReadDataFromStream(responseStream);
                    responseStream.Close();
                }

            } finally {
                dataStream.Dispose();
            }

            return result;
        }

        private void WriteRequestBody(Stream stream, IDictionary<string, string> parameters)
        {
            byte[] boundarybytes = Encoding.ASCII.GetBytes(String.Format("\r\n--{0}\r\n", boundary));

            stream.Write(boundarybytes, 0, boundarybytes.Length);

            foreach (KeyValuePair<String, String> item in parameters) {
                byte[] dataByte = Encoding.ASCII.GetBytes(
                    String.Format("\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\";\r\n\r\n{2}",
                    boundary, item.Key, item.Value));

                stream.Write(dataByte, 0, dataByte.Length);
            }

            stream.Write(boundarybytes, 0, boundarybytes.Length);
        }

        private IDictionary<string, string> ReadDataFromStream(Stream stream)
        {
            lastRawDataRecived = "";
            lasteError = "";
            isErrorExist = false;

            IDictionary<String, String> result = new Dictionary<string, string>();

            string content;
            using (StreamReader reader = new StreamReader(stream)) {
                content = reader.ReadToEnd();
            }

            lastRawDataRecived = content;

            if (content != null) {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlNode rootNote = doc.SelectSingleNode("root");
                foreach (XmlNode childNode in rootNote.ChildNodes) {
                    if (childNode.NodeType == XmlNodeType.Element) {
                        result.Add(childNode.Name, childNode.InnerText);
                    }
                }
            }

            if (result["status"] == "error") {
                isErrorExist = true;
                lasteError = result["error"];
            }

            return result;
        }


        public string GetLastDtaRecived()
        {
            return lastRawDataRecived;
        }


        public string GetLastError()
        {
            return lasteError;
        }


        public bool IsErrorExist()
        {
            return isErrorExist;
        }
    }
}
