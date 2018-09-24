using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace tickets.API
{
    public class Server
    {
        const string BASE_ADDRESS = "https://178.128.75.38";

        public Server()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public async Task<int> countResponse(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id);
            string html = await response.Content.ReadAsStringAsync();
            string searchSS = "<td class=\"ticketrow\">";
            int count = 0;
            int size = searchSS.Count();
            while (html.IndexOf(searchSS) > -1)
            {
                count++;
                int begin = size + html.IndexOf(searchSS);
                html = html.Substring(begin);
            }
            return count;
        }

        public async Task<string> getInitDate(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id);
            string html = await response.Content.ReadAsStringAsync();
            string search = "Creado en: </td>";
            int size = search.Count();
            int begin = size + html.IndexOf(search);
            string date = "";
            char val = html[begin];
            if (html.IndexOf(search) > -1)
            {
                while (val != '/')
                {
                    date += val;
                    begin++;
                    val = html[begin];
                }
                string[] array = date.Split('>');
                date = array[1];
                date = date.Remove(date.Length - 1);
                return date;

            }

            return "error";
        }

        public async Task<string> getUpdateDate(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id);
            string html = await response.Content.ReadAsStringAsync();
            string search = "�ltima actualizacion: </td>";
            int size = search.Count();
            int begin = size + html.IndexOf(search);
            string date = "";
            char val = html[begin];
            if (html.IndexOf(search) > -1)
            {
                while (val != '/')
                {
                    date += val;
                    begin++;
                    val = html[begin];
                }
                string[] array = date.Split('>');
                date = array[1];
                date = date.Remove(date.Length - 1);
                return date;

            }
            return "error";
        }


        public async Task<string> getTicket(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id);
            string value = await response.Content.ReadAsStringAsync();
            return value;
        }

        public async Task<string> submitTicket(string number, string subject, string message, string priority, string qualification)
        {
            User user = await App.Database.GetCurrentUser();

            var html = @"" + BASE_ADDRESS + "/index.php?a=add";
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //httpClient.BaseAddress = new Uri("https://178.128.75.38/");
            HttpResponseMessage capture = await httpClient.GetAsync(html);
            MultipartFormDataContent form = new MultipartFormDataContent();

            String res = capture.Headers.ElementAt(3).Value.ElementAt(0).ToString();
            String[] tokens = res.Split(';');
            String cookie = tokens[0];

            String[] tokensValue = cookie.Split('=');
            String valueCookie = tokensValue[1];

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await capture.Content.ReadAsStringAsync());

            var node = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='token']");

            Console.WriteLine("Node Name: " + node.Name + "\n" + node.GetAttributeValue("value", "0"));

            string token = node.GetAttributeValue("value", "0");


            form.Headers.Add("Cookie", cookie);
            form.Add(new StringContent(user.Name), "name");
            form.Add(new StringContent(user.Email), "email");
            form.Add(new StringContent(user.Campus), "custom1");
            form.Add(new StringContent(user.Profile), "custom2");
            form.Add(new StringContent(user.Account), "custom3");
            form.Add(new StringContent(user.Career), "custom4");
            form.Add(new StringContent(qualification), "custom5");
            form.Add(new StringContent(user.PhoneNumber), "custom15");
            form.Add(new StringContent(number), "custom20");
            form.Add(new StringContent("1"), "category");
            form.Add(new StringContent(priority), "priority");
            form.Add(new StringContent(subject), "subject");
            form.Add(new StringContent(message), "message");
            //form.Add(new ByteArrayContent(new byte[0]), "attachment[1]");
            form.Add(new StringContent(token), "token");
            HttpResponseMessage response = await httpClient.PostAsync(BASE_ADDRESS + "/submit_ticket.php", form);
            //response.Headers.Add(

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = await response.Content.ReadAsStringAsync();

            var result = new HtmlDocument();
            result.LoadHtml(sd);

            var success = result.DocumentNode.SelectSingleNode("//div[@class='success']");
            if (success == null)
            {
                return "error";
            }
            else
            {
                var ticketId = success.SelectSingleNode("//b[2]");
                Console.WriteLine("TICKET ENVIADO, SU ID = " + ticketId.InnerText);
                return ticketId.InnerText;
            }
        }
        public async Task<string> replyTicket(string message, string ticketID)
        {
            //catch the cookie
            HttpClient _client = new HttpClient();
            HttpResponseMessage capture = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + ticketID);
            String res = capture.Headers.ElementAt(3).Value.ElementAt(0).ToString();
            String[] tokens = res.Split(';');
            String cookie = tokens[0];

            //catch the token
            String html = await capture.Content.ReadAsStringAsync();
            string searchSS = "name=\"token\" value=\"";
            int size = searchSS.Count();
            int begin = size + html.IndexOf(searchSS);
            string token = "";
            char val = html[begin];
            while (val != '\"')
            {
                token += val;
                begin++;
                val = html[begin];
            }

            //value cookie
            String[] tokensValue = cookie.Split('=');
            String valueCookie = tokensValue[1];

            //headers
            HttpWebRequest requestToServer = (HttpWebRequest)WebRequest.Create(BASE_ADDRESS + "/reply_ticket.php");
            String boundaryString = "----WebKitFormBoundary" + valueCookie;
            requestToServer.AllowReadStreamBuffering = false;
            requestToServer.Method = WebRequestMethods.Http.Post;
            requestToServer.Headers.Add("Cookie", cookie);
            requestToServer.ContentType = "multipart/form-data; boundary=" + boundaryString;

            //generate body
            ASCIIEncoding ascii = new ASCIIEncoding();
            //boundary
            string boundaryStringLine = "\r\n--" + boundaryString + "\r\n";
            byte[] boundaryStringLineBytes = ascii.GetBytes(boundaryStringLine);
            //message
            string messageInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "message", message);
            byte[] messageInputBytes = ascii.GetBytes(messageInput);
            //ticketID
            string ticketInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "orig_track", ticketID);
            byte[] ticketInputBytes = ascii.GetBytes(ticketInput);
            //token
            string tokenInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "token", token);
            byte[] tokenInputBytes = ascii.GetBytes(tokenInput);
            //boundary final
            string lastBoundaryStringLine = "\r\n--" + boundaryString + "--";
            byte[] lastBoundaryStringLineBytes = ascii.GetBytes(lastBoundaryStringLine);

            //size buffer
            long totalRequestBodySize = boundaryStringLineBytes.Length * 3
                + messageInputBytes.Length
                + ticketInputBytes.Length
                + tokenInputBytes.Length
                + lastBoundaryStringLineBytes.Length;

            requestToServer.ContentLength = totalRequestBodySize;

            //white body
            using (Stream s = requestToServer.GetRequestStream())
            {
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(messageInputBytes, 0, messageInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(ticketInputBytes, 0, ticketInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(tokenInputBytes, 0, tokenInputBytes.Length);
                s.Write(lastBoundaryStringLineBytes, 0, lastBoundaryStringLineBytes.Length);
            }

            //response 
            WebResponse response = requestToServer.GetResponse();
            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            //catch ticketID
            String responseHtml = responseReader.ReadToEnd();
            string searchR = "�xito:</b>";
            if (responseHtml.IndexOf(searchR) > -1)
            {
                return "ok";
            }
            else
            {
                return "error";
            }
        }
    }
}
