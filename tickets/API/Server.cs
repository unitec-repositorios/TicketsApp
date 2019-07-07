using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;

namespace tickets.API
{
    public class Server
    {
       // const string BASE_ADDRESS = "https://cap.unitec.edu";
        const string BASE_ADDRESS = AppSettings.BASE_ADDRESS;
        private readonly RestClient client;
        public Server()
        {
            client = new RestClient(BASE_ADDRESS);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        }

        public async Task<string> getDetailsTicket(string id)
        {
            HttpClient client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            var uri = BASE_ADDRESS + "/print.php?track=" + id+"&e="+user.Email;
            var response = await client.GetByteArrayAsync(uri);
            Encoding encoder = Encoding.GetEncoding(AppSettings.Encoding);
            string html= encoder.GetString(response, 0, response.Length - 1);
            if (html.IndexOf("<b>Error:</b>") != -1)
            {
                return "Error";
            }   
            return html;
        }



        public async Task<List<DateTime>> getDateMessage(string id)
        {
            List<DateTime> fechas= new List<DateTime>();
            HttpClient client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            response.Content.Headers.ContentType.CharSet= AppSettings.Encoding;
            string html = await response.Content.ReadAsStringAsync();
            int posFecha = 0;
            int pos = 0;
            while (pos != -1)
            {
                string search = "<td class=\"tickettd\">2";
                pos = html.IndexOf(search);
                posFecha = pos - 1 + search.Length;
                if (pos > -1)
                {
                    string fecha = getTextAux('<', html, posFecha);
                    fechas.Add(DateTime.Parse(fecha));
                    pos = posFecha + 1;
                    html = html.Substring(pos);
                }
            }
            return fechas;
        }

        public string GetBaseAdress()
        {
            return BASE_ADDRESS;
        }

        public async Task<string> getRefresh()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(BASE_ADDRESS + "/ticket.php");
            string html = await response.Content.ReadAsStringAsync();
            string search = "\"Refresh\" value=" + '"';
            int posRefresh = html.IndexOf(search) + search.Length;
            string refresh = getTextAux('"', html, posRefresh);
            return refresh;
        }

        public async Task<int> countResponse(string id)
        {
            HttpClient _client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
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
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            string search = "Creado en: </td>";
            int size = search.Count();
            int begin = size + html.IndexOf(search);
            string date = "";
            char val = html[begin];
            if (html.IndexOf(search) > -1)
            {
                date = getTextAux('/', html, begin);
                string[] array = date.Split('>');
                date = array[1];
                date = date.Remove(date.Length - 1);
                return date;

            }

            return "error al recibir la fecha";
        }

      

        public async Task<string> getUpdateDate(string id)
        {
           
            HttpClient _client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("HTMLLLLLLL: " + html);
            string search = "�ltima actualizacion: </td>";
            int size = search.Count();
            Console.WriteLine("INT SIZE: " + size);
            int begin = size + html.IndexOf(search);
            Console.WriteLine("INT BEGIN: " + begin);
            string date = "";
            char val = html[begin];
            Console.WriteLine("HTML INDEX OF SEARCH: "+html.IndexOf(search));
            if (html.IndexOf(search,0) > -1)
            {
                date = getTextAux('/', html, begin);
                string[] array = date.Split('>');
                date = array[1];
                date = date.Remove(date.Length - 1);
                return date;

            }
            else
            {
               return "error";
            }
            
        }
        public async Task<bool> getOpenTicket(string id)
        {
            HttpClient _client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            return html.IndexOf("resolved") == -1;
        }

        public async Task changeStatusTicket(string id)
        {
            HttpClient client = new HttpClient();
            User user = await App.Database.GetCurrentUser();
            HttpResponseMessage response = await client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            int posRefresh = html.IndexOf("Refresh=");
            int posToken =  html.IndexOf("token=");
            string refresh = getTextAux('a', html, posRefresh);
            string token = getTextAux('"',html,posToken);
            string s = await getOpenTicket(id) ? "3" : "1"; // 1 to open and 3 to close
            string link = BASE_ADDRESS + "/change_status.php?track=" + id + "&s=" + s + "&" + refresh + "&" + token;
            response = await client.GetAsync(link);
        }

        public string getTextAux(char delimiter,string text,int pos)
        {
            string txt = "";
            int textdebug = text.Length;
            int textdebug2 = pos;
            char val = text[pos];
            
            while (val != delimiter)
            {
                txt += val;
                pos++;
                val = text[pos];
            }
            return txt;
        }

        public string getItemOfHTML(string htmlsource,string item,string nextTag="",string delimitador="<")
        {
            Console.WriteLine("\nHTML Source:\n" + htmlsource);
            string txt = "";
            if (!htmlsource.Contains(item))
            {
                return "<Error>";
            }
            int posItem = htmlsource.IndexOf(item) + item.Length ;
            char val = htmlsource[posItem];
            if (nextTag != "")
            {
                while (val != nextTag[0])
                {
                    posItem++;
                    val = htmlsource[posItem];
                }
            }
          
            posItem =posItem + nextTag.Length;
            val = htmlsource[posItem];
            while (val != delimitador[0])
            {
                txt += val;
                posItem++;
                val = htmlsource[posItem]; 
            }
            return txt;
        }

        private string getItemValue(HtmlDocument _document,Dictionary<string,object> _parserServer, string _key)
        {
            var _temp = _document.DocumentNode.SelectNodes((_parserServer[_key]).ToString()).FirstOrDefault().InnerText;
            Console.WriteLine("\nConfig key "+_temp);
            return _temp;
        }

        public async Task<Ticket> GetTicket(string id)
        {
            User user = await App.Database.GetCurrentUser();
            var request = new RestRequest($"/print.php?track={id}+&e={user.Email}", Method.GET);
            var response = client.Execute(request); 
            if (response.StatusCode!=HttpStatusCode.OK || response.Content.IndexOf("<b>Error:</b>") != -1)
            {
                return null;
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content);
            Dictionary<string, object> configParser = AppSettings.getConfigurationParser("print.php");
            if (configParser == null) return null;
            return new Ticket()
            {
                
                ID              =   getItemValue(document,configParser,"ID de seguimiento"),
                Subject         =   getItemValue(document,configParser, "Tema"),
                UserID          =   int.Parse(getItemValue(document,configParser,"Número de Cuenta")),
                Classification  =   getItemValue(document,configParser,"Clasificación"),
                LastUpdate      =   DateTime.ParseExact(getItemValue(document,configParser,"Actualizar"), "yyyy-MM-dd HH:mm:ss", null),
                CreationDate    =   DateTime.ParseExact(getItemValue(document,configParser,"Creado en"), "yyyy-MM-dd HH:mm:ss", null),
                Message         =   getItemValue(document,configParser,"Mensaje"),
                Open            =   !(getItemValue(document,configParser,"Estado del ticket").Contains("Resuelto"))

               
            };



        }


        public async Task<string> getTicket(string id)
        {
            var uri = BASE_ADDRESS + "/ticket.php?track=" + id;
            HttpClient _client = new HttpClient();
            var response = await _client.GetByteArrayAsync(uri);
            var test = _client.GetStringAsync(uri);
            Console.WriteLine("Method GET ");
            Console.WriteLine(test.ToString());
            Encoding encoder = Encoding.GetEncoding(AppSettings.Encoding);
            string value = encoder.GetString(response, 0, response.Length - 1);
            return value;
        }

        public async Task<string> submitTicket(string number, string subject, string message, string priority, string qualification, List<(string, byte[])> files)
        {
            Ticket ticket = new Ticket();
            User user = await App.Database.GetCurrentUser();
            Dictionary<string, string> document = new Dictionary<string, string>
            {
                {"perfil", user.Profile },
                {"campus",user.Campus },
                {"area",ticket.Area },
                {"category",ticket.Category },
                {"name",user.Name },
                {"email",user.Email },
                {"priority",ticket.Priority.ToString()},
                {"custom3",user.Account },
                {"custom1",user.Campus },
                {"custom2",user.Profile },
                {"custom3",ticket.UserID.ToString()},
                {"custom4",user.Career},
                {"custom5",ticket.Classification},
                {"custom6",user.PhoneNumber},
                {"custom7",user.PersonalMail},
                {"subject",ticket.Subject},
                {"messege",ticket.Message}
            };
            var request = new RestRequest("/index.php?a=add",Method.POST);
            var responseR =client.Execute(request);


            ///<sumary>Cookie</sumary>
            var cookie = responseR.Cookies.FirstOrDefault();
            request.AddCookie(cookie.Name,cookie.Value);


            var html = @"" + BASE_ADDRESS + "/index.php?a=add";
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //httpClient.BaseAddress = new Uri("https://178.128.75.38/");
            
            HttpResponseMessage capture = await httpClient.GetAsync(html);
           
            MultipartFormDataContent form = new MultipartFormDataContent();

            String res = capture.Headers.ElementAt(3).Value.ElementAt(0).ToString();
           
            String[] tokens = res.Split(';');
            String cookieS = tokens[0];

            String[] tokensValue = cookieS.Split('=');
            String valueCookie = tokensValue[1];

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await capture.Content.ReadAsStringAsync());

            var node = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='token']");

            Console.WriteLine("Node Name: " + node.Name + "\n" + node.GetAttributeValue("value", "0"));

            string token = node.GetAttributeValue("value", "0");

            Encoding encoder = Encoding.GetEncoding(AppSettings.Encoding);

            form.Headers.Add("Cookie", cookieS);
            form.Headers.ContentType.CharSet = AppSettings.Encoding;
            form.Add(new StringContent(user.Name, encoder), "name");
            form.Add(new StringContent(user.Email, encoder), "email");
            form.Add(new StringContent(user.Account, encoder), "custom3");
            form.Add(new StringContent(user.Campus, encoder), "custom1");
            form.Add(new StringContent(user.Profile, encoder), "custom2");
            form.Add(new StringContent(user.Career, encoder), "custom4");
            form.Add(new StringContent(qualification, encoder), "custom5");
            form.Add(new StringContent(user.PhoneNumber, encoder), "custom15");
            form.Add(new StringContent(number, encoder), "custom20");
            form.Add(new StringContent("1", encoder), "category");
            form.Add(new StringContent(priority, encoder), "priority");
            form.Add(new StringContent(subject, encoder), "subject");
            form.Add(new StringContent(message, encoder), "message");
           
            for (int x = 0; x < files.Count; x++)
            {
                form.Add(new ByteArrayContent(files[x].Item2, 0, files[x].Item2.Length), "attachment[" + (x + 1) + "]", files[x].Item1);
            }
            form.Add(new StringContent(token, encoder), "token");
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

        private int GetClasificacion(string clasificacion)
        {
            switch (clasificacion)
            {
                case "Solicitud":   return 1;
                case "Información": return 2;
                case "Queja":       return 3;
                case "Reclamo":     return 4;
                default:            return 5;
            }
           
        }
        private int GetPrioridad(string prioridad)
        {
            switch (prioridad) {
                case "Alto":    return 1;
                case "Medio":   return 2;
                case "Bajo":    return 3;
              
                default:        return 3;
            }
        }


        public async Task<string> replyTicket(string message, List<(string, byte[])> files, string ticketID)
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
            MultipartFormDataContent form = new MultipartFormDataContent();
            //boundary
            string boundaryStringLine = "\r\n--" + boundaryString + "\r\n";
            byte[] boundaryStringLineBytes = ascii.GetBytes(boundaryStringLine);
            //message
            string messageInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "message", message);
            Console.WriteLine("\n\nMensaje:\n" + messageInput);
            byte[] messageInputBytes = ascii.GetBytes(messageInput);
            //files
            for (int x = 0; x < files.Count; x++)
            {
                form.Add(new ByteArrayContent(files[x].Item2, 0, files[x].Item2.Length), "attachment[" + (x + 1) + "]", files[x].Item1);
            }
            /* string filesInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "attachments", files);
             byte[] filesInputBytes = ascii.GetBytes(filesInput);*/
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
                //+ filesInputBytes.Length
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
                //s.Write(filesInputBytes, 0, filesInputBytes.Length);
                //s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(ticketInputBytes, 0, ticketInputBytes.Length);
                s.Write(boundaryStringLineBytes, 0, boundaryStringLineBytes.Length);
                s.Write(tokenInputBytes, 0, tokenInputBytes.Length);
                s.Write(lastBoundaryStringLineBytes, 0, lastBoundaryStringLineBytes.Length);
            }

            //response 
            WebResponse response = requestToServer.GetResponse();
            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            HttpResponseMessage responsee = await _client.PostAsync(BASE_ADDRESS + "/reply_ticket.php", form);
            //response.Headers.Add(

            responsee.EnsureSuccessStatusCode();
            _client.Dispose();
            string sd = await responsee.Content.ReadAsStringAsync();

            var result = new HtmlDocument();
            result.LoadHtml(sd);
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