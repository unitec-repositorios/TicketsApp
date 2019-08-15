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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using RestSharp;
using tickets.Models;

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



     
        public async Task<Ticket> GetTicket(string id)
        {
            User user = await App.Database.GetCurrentUserAsync();
            var request = new RestRequest($"/print.php?track={id}+&e={user.Email}", Method.GET);
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK || response.Content.IndexOf("<b>Error:</b>") != -1)
            {
                return null;
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content);
            Dictionary<string, object> configParser = AppSettings.getConfigurationParser("print.php");
            if (configParser == null) return null;
            var _estadoTicket = getItemValue(document, configParser, "Estado del ticket");
            
            Ticket _ticket= new Ticket()
            {
                ID = getItemValue(document, configParser, "ID de seguimiento"),
                Subject = getItemValue(document, configParser, "Tema"),
                UserID = int.Parse(getItemValue(document, configParser, "Número de Cuenta")),
                Classification = getItemValue(document, configParser, "Clasificación"),
                LastUpdate = DateTime.ParseExact(getItemValue(document, configParser, "Actualizar"), "dd/MM/yyyy HH:mm:ss", null),
                CreationDate = DateTime.ParseExact(getItemValue(document, configParser, "Creado en"), "dd/MM/yyyy HH:mm:ss", null),
                Message = getItemValue(document, configParser, "Mensaje"),
                Estado = _estadoTicket
            };
            _ticket.Check();
            return _ticket;

        }

        public async Task<string> SendTicket(Ticket _ticket, List<(string, byte[])> _files)
        {
            User user = App.Database.GetCurrentUser();
            var _perfil = getIdPerfil(user.Profile);
            var _campus = getIdCampus(user.Campus);

            var request = new RestRequest($"/index.php?a=add&perfil={_perfil}&campus={_campus}&area={_ticket.Area}&category={_ticket.Category}", Method.POST);
            var responseR = client.Execute(request);




            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(responseR.Content);
            var configParser = AppSettings.getConfigurationParser("index.php");

            //Token
            var tokenValue = doc.DocumentNode.SelectSingleNode((configParser["Token"]).ToString()).Attributes["value"].Value;

            //Cookie
            var cookie = responseR.Cookies.FirstOrDefault();

            //Body Request
            Dictionary<string, object> document = new Dictionary<string, object>
            {
                {"name",user.Name },
                {"email",user.Email },
                {"priority",_ticket.Priority},
                {"custom1",user.Campus },
                {"custom2",user.Profile },
                {"custom3",user.Account },


                {"custom4",user.Career},
                {"custom5",_ticket.Classification},
                {"custom6",user.PhoneNumber},
                {"custom7",user.PersonalMail==null?user.Email:user.PersonalMail},
                {"subject",_ticket.Subject},
                {"message",_ticket.Message},
                {"category",_ticket.Category },
                {"token",tokenValue},
                {"hx",3 },
                {"hy",""}

            };

            MultipartFormDataContent form = new MultipartFormDataContent();

            //GET CHARSET OF SERVER
            string charset = new StringContent(responseR.Content).Headers.ContentType.CharSet;

            //Set Encoding al formulario
            var encoder = Encoding.GetEncoding(charset);
            form.Headers.ContentType.CharSet = charset;
            Console.WriteLine("Charset: " + encoder.EncodingName + "\t" + charset);

            //Agregando cookie
            form.Headers.Add("Cookie", $"{cookie.Name}={cookie.Value}");

            foreach (var item in document)
            {
                //Agregando contenido al formulario
                form.Add(new StringContent(item.Value.ToString(), encoder), item.Key);
            }

            int indexFile = 1;

            //Agregando los Archivos al Formulario
            foreach (var file in _files)
            {
                form.Add(new ByteArrayContent(file.Item2, 0, file.Item2.Length), "attachment[" + (indexFile) + "]", file.Item1);
                indexFile++;
            }
            string responseResult = "error";

            //Enviando la peticion al servidor
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{BASE_ADDRESS}/submit_ticket.php", form);
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var resultData = await response.Content.ReadAsStringAsync();
                        response.Dispose();
                        doc.LoadHtml(resultData);
                        var success = doc.DocumentNode.SelectSingleNode("//div[@class='success']");
                        if (success != null)
                        {
                            var ID_TICKET = success.SelectSingleNode("//b[2]");
                            Console.WriteLine("TICKET ENVIADO, SU ID = " + ID_TICKET.InnerText);
                            responseResult = ID_TICKET.InnerText;
                            return ID_TICKET.InnerText;   // return ID del ticket creado.
                        }
                    }
                }
            }
            return responseResult;  //return "error" si no se creo el ticket

        }

        public async Task<List<Message>> GetMessages(string _idTicket)
        {
            User _user = await App.Database.GetCurrentUserAsync();
            var request = new RestRequest($"/ticket.php?track={_idTicket}+&e={_user.Email}", Method.GET);
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK || response.Content.IndexOf("<b>Error:</b>") != -1)
            {
                return null;
            }
            List<Message> mensajes = new List<Message>();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content);
            int cont = 1;
            while (getMessageText(cont, document)!=null)
            {
                mensajes.Add(new Message
                {
                    Autor = getMessageFrom(cont, document),
                    Date = getMessageDate(cont,document),
                    Text = getMessageText(cont,document)

                });
                cont++;
            }
         

            return mensajes;
        }

        private Dictionary<string, string> GetGestiones()
        {
            User _user = App.Database.GetCurrentUser();
            var perfil = getIdPerfil(_user.Profile);
            var campus = getIdCampus(_user.Campus);
            var request = new RestRequest($"/index.php?a=add&perfil={perfil}&campus={campus}", Method.GET);
            var response = client.Execute(request);
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Content);
            var categorias = doc.DocumentNode.SelectNodes("//*[@id=\"ul_category\"]//li");
            Dictionary<string, string> gestiones = new Dictionary<string, string>();
            foreach (var item in categorias)
            {
                gestiones.Add(item.InnerText.Substring(2), item.FirstChild.Attributes["href"].Value.Replace("amp;", ""));
            }

            return gestiones;
        }

        private Dictionary<string, string> GetComoPodemosAyudarte(string url)
        {
            var request = new RestRequest($"{url}", Method.GET);
            var response = client.Execute(request);
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Content);
            var categorias = doc.DocumentNode.SelectNodes("//*[@id=\"ul_category\"]//li");
            Dictionary<string, string> comoPodemosAyudarte = new Dictionary<string, string>();
            foreach (var item in categorias)
            {
                comoPodemosAyudarte.Add(item.InnerText.Substring(8), item.FirstChild.Attributes["href"].Value.Replace("amp;", ""));
            }
            return comoPodemosAyudarte;
        }


        private int getIdCampus(string campusName)
        {
            int idCampus = AppSettings.getIdCampus(campusName);
            if (idCampus!=0)
                return idCampus;
            return 3;

        }

        private int getIdPerfil(string perfilName)
        {
            switch (perfilName)
            {
                case "Administrativo":  return 1;
                case "Docente":         return 2;
                case "Alumno":          return 3;
                default:
                    return 3;
            }
        }

        private string getItemValue(HtmlDocument _document, Dictionary<string, object> _parserServer, string _key)
        {
            var _temp = _document.DocumentNode.SelectNodes((_parserServer[_key]).ToString()).FirstOrDefault().InnerText;
            return _temp;
        }

        private List<(object,string,List<(object,string)>)> GetAreas_Categorias()
        {
            List<(object, string, List<(object, string)>)> _tempList = new List<(object, string, List<(object, string)>)>();
            foreach(var item in GetGestiones())
            {
                List<(object, string)> _tempList2 = new List<(object, string)>();
                foreach(var item2 in GetComoPodemosAyudarte(item.Value))
                {
                    var _value = GetDictionaryItems(item2.Value)["category"];
                    _tempList2.Add((_value, item2.Key));
                }
                var _valueT = GetDictionaryItems(item.Value)["area"];
                _tempList.Add((_valueT,item.Key,_tempList2));
            }
            return _tempList;
        }

        public Dictionary<string,List<string>> getDictionaryAreasXCategorias()
        {
            Dictionary<string, List<string>> _temp = new Dictionary<string, List<string>>();
            foreach(var item in GetGestiones())
            {
                List<string> _tempList = new List<string>();     
                foreach(var item2 in GetComoPodemosAyudarte(item.Value))
                {
                    _tempList.Add(item2.Key);
                }
                _temp.Add(item.Key, _tempList);
            }
            return _temp;
        }

        public Dictionary<string,object> GetDictionaryAreas()
        {
            return new Dictionary<string, object>(GetAreas_Categorias().ToDictionary(x => x.Item2, x => x.Item1));
        }

        public Dictionary<string,object> GetDictionaryCategory(string area)
        {
            return new Dictionary<string, object>(GetAreas_Categorias().FirstOrDefault(x => x.Item2 == area).Item3.ToDictionary(item => item.Item2, item => item.Item1));
        }

        private Dictionary<string,object> GetDictionaryItems(string url)
        {
           
            var request = new RestRequest(url, Method.GET,DataFormat.Json);
            return request.Parameters.ToDictionary(x => x.Name, x => x.Value);
        }

        public string GetValueArea(string area)
        {
            return GetDictionaryAreas()[area].ToString();
        }

        public string GetValueCategoria(string area,string categoria)
        {
            return GetDictionaryCategory(area)[categoria].ToString();
        }

        public async Task<string> getDetailsTicket(string id)
        {
            HttpClient client = new HttpClient();
            User user = await App.Database.GetCurrentUserAsync();

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
            User user = await App.Database.GetCurrentUserAsync();
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

        public async Task<string> getRefreshCode()
        {

            var request = new RestRequest("/ticket.php");
            var response = client.Execute(request);
            var document = new HtmlDocument();
            document.Load(response.Content);
            var element = document.DocumentNode.SelectSingleNode("//input[@name='Refresh']");
            return element.Attributes["value"].Value;
        }

        public async Task<int> countResponse(string id)
        {
            HttpClient _client = new HttpClient();
            User user = await App.Database.GetCurrentUserAsync();
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
            User user = await App.Database.GetCurrentUserAsync();
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
            User user = await App.Database.GetCurrentUserAsync();
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
            User user = await App.Database.GetCurrentUserAsync();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            return html.IndexOf("resolved") == -1;
        }

        public async Task changeStatusTicket(string id)
        {
            HttpClient client = new HttpClient();
            User user = await App.Database.GetCurrentUserAsync();
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

        public async Task<string> submitTicket(string number, string subject, string message, string priority, string qualification, Ticket _ticket, List<(string, byte[])> files)
        {
           
            User user = await App.Database.GetCurrentUserAsync();
            

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
           // form.Add(new StringContent(number, encoder), "custom20");
            form.Add(new StringContent(_ticket.Category, encoder), "category");
            form.Add(new StringContent(_ticket.Priority.ToString(), encoder), "priority");
            form.Add(new StringContent(_ticket.Subject, encoder), "subject");
            form.Add(new StringContent(_ticket.Message, encoder), "message");
           
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

        private string getMessageFrom(int index, HtmlDocument document)
        {
            var element = document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]/td[2]/table//tr//table//tr//table//tr[2]//td[2]");
            if (index == 1)
                return element.InnerText;
            element = document.DocumentNode.SelectSingleNode($"//html/body//table//tr[2]//table[2]//tr[2]//table//tr[{index}]//table//tr//table//tr[2]/td[2]");
            if (element != null)
                return element.InnerText;
            else
                return null;

        }
        private string getMessageText(int n, HtmlDocument document)
        {
            var element = document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]//table//tr[{n}]//p[2]");
            if (element != null)
                return element.InnerText.Replace("&nbsp;", "");
            else
                return null;
        }
        private DateTime getMessageDate(int index, HtmlDocument document)
        {
            var element = document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]/td[2]/table//tr//table//tr//table//tr//td[2]");
            if (index == 1)
                return DateTime.ParseExact(element.InnerText, "dd/MM/yyyy HH:mm:ss", null);

            element = document.DocumentNode.SelectSingleNode($"//html/body//table//tr[2]//table[2]//tr[2]//table//tr[{index}]//table//tr//table//td[2]");
            if (element != null)
                return DateTime.ParseExact(element.InnerText, "dd/MM/yyyy HH:mm:ss", null);
            else
                return DateTime.MinValue;
        }
    }
}