using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace tickets.API
{
    public class Server
    {

        public Server()
        {

        }
        public async Task<int> countResponse(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync("http://178.128.75.38/ticket.php?track=" + id);
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
            HttpResponseMessage response = await _client.GetAsync("http://178.128.75.38/ticket.php?track=" + id);
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
                date = date.Remove(date.Length-1);
                return date;

            }
            
            return"error";
        }

        public async Task<string> getUpdateDate(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync("http://178.128.75.38/ticket.php?track=" + id);
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
                date = date.Remove(date.Length-1);
                return date;

            }
            return "error";
        }


        public async Task<string> getTicket(string id)
        {
            HttpClient _client = new HttpClient();
            HttpResponseMessage response = await _client.GetAsync("http://178.128.75.38/ticket.php?track=" + id);
            string value = await response.Content.ReadAsStringAsync();
            return value;
        }

        public async Task<string> submitTicket(string number,string subject, string message, string priority,string qualification)
        {
            User user = await App.Database.GetCurrentUser();

            //catch the cookie
            HttpClient _client = new HttpClient();
            HttpResponseMessage capture = await _client.GetAsync("http://178.128.75.38/index.php?a=add");
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
            HttpWebRequest requestToServer = (HttpWebRequest)WebRequest.Create("http://178.128.75.38/submit_ticket.php");
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
            //name
            string nameInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "name", user.Name);
            byte[] nameInputBytes = ascii.GetBytes(nameInput);
            //email
            string emailInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "email", user.Email);
            byte[] emailInputBytes = ascii.GetBytes(emailInput);
            //custom1
            string custom1Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom1", user.Campus);
            byte[] custom1InputBytes = ascii.GetBytes(custom1Input);
            //custom2
            string custom2Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom2", user.Profile);
            byte[] custom2InputBytes = ascii.GetBytes(custom2Input);
            //custom3
            string custom3Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom3", user.Account);
            byte[] custom3InputBytes = ascii.GetBytes(custom3Input);
            //custom4
            string custom4Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom4", user.Career);
            byte[] custom4InputBytes = ascii.GetBytes(custom4Input);
            //custom5
            string custom5Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom5", qualification);
            byte[] custom5InputBytes = ascii.GetBytes(custom5Input);
            //custom15
            string custom15Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom15", user.PhoneNumber);
            byte[] custom15InputBytes = ascii.GetBytes(custom15Input);
            //custom20
            string custom20Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom20", number);
            byte[] custom20InputBytes = ascii.GetBytes(custom20Input);
            //category
            string categoryInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "category", "Default");
            byte[] categoryInputBytes = ascii.GetBytes(categoryInput);
            //priority
            string priorityInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "priority", priority);
            byte[] priorityInputBytes = ascii.GetBytes(priorityInput);
            //subject
            string subjectInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "subject", subject);
            byte[] subjectInputBytes = ascii.GetBytes(subjectInput);
            //message
            string messageInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "message", message);
            byte[] messageInputBytes = ascii.GetBytes(messageInput);
            //token
            string tokenInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "token", token);
            byte[] tokenInputBytes = ascii.GetBytes(tokenInput);
            //boundary final
            string lastBoundaryStringLine = "\r\n--" + boundaryString + "--";
            byte[] lastBoundaryStringLineBytes = ascii.GetBytes(lastBoundaryStringLine);

            //size buffer
            long totalRequestBodySize = boundaryStringLineBytes.Length * 14
                + nameInputBytes.Length
                + emailInputBytes.Length
                + custom1InputBytes.Length
                + custom2InputBytes.Length
                + custom3InputBytes.Length
                + custom4InputBytes.Length
                + custom5InputBytes.Length
                + custom15InputBytes.Length
                + custom20InputBytes.Length
                + categoryInputBytes.Length
                + priorityInputBytes.Length
                + subjectInputBytes.Length
                + messageInputBytes.Length
                + tokenInputBytes.Length
                + lastBoundaryStringLineBytes.Length;
            requestToServer.ContentLength = totalRequestBodySize;

            //white dody
            using (Stream s = requestToServer.GetRequestStream())
            {
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(nameInputBytes, 0, nameInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(emailInputBytes, 0, emailInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom1InputBytes, 0, custom1InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom2InputBytes, 0, custom2InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom3InputBytes, 0, custom3InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom4InputBytes, 0, custom4InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom5InputBytes, 0, custom5InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom15InputBytes, 0, custom15InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(custom20InputBytes, 0, custom20InputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(categoryInputBytes, 0, categoryInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(priorityInputBytes, 0, priorityInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(subjectInputBytes, 0, subjectInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(messageInputBytes, 0, messageInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(tokenInputBytes, 0, tokenInputBytes.Length);
                s.Write(lastBoundaryStringLineBytes, 0, lastBoundaryStringLineBytes.Length);
            }

            //response 
            WebResponse response = requestToServer.GetResponse();
            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            //catch ticketID
            String responseHtml = responseReader.ReadToEnd();
            string searchR = "Ticket ID: <b>";
            int sizeR = searchR.Count();
            int beginR = sizeR + responseHtml.IndexOf(searchR);
            string ticketID = "";
            char valR = responseHtml[beginR];
            if (responseHtml.IndexOf(searchR) > -1)
            {
                while (valR != '<')
                {
                    ticketID += valR;
                    beginR++;
                    valR = responseHtml[beginR];
                }
                return ticketID;
                
            }
            else
            {
                return "error";

            }
        }
        public async Task<string> replyTicket(string message, string ticketID)
        {
            //catch the cookie
            HttpClient _client = new HttpClient();
            HttpResponseMessage capture = await _client.GetAsync("http://178.128.75.38/ticket.php?track=" + ticketID);
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
            HttpWebRequest requestToServer = (HttpWebRequest)WebRequest.Create("http://178.128.75.38/reply_ticket.php");
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
