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
      
        private Configuraciones _configuracion = new Configuraciones();

        public Server()
        {
            client = new RestClient(BASE_ADDRESS);
            HttpClient httpClient = new HttpClient();
          
            //CurrentUser = App.Database.GetCurrentUser();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        }

        /// <summary>
        /// GET Method: Obtiene un apartir del ID
        /// </summary>
        /// <param name="id"> ID del Ticket a buscar </param>
        /// <param name="_root"> La ruta de la pagina donde va a buscar {ticket.php , print.php}   </param>
        /// <returns> Retorna un objeto tipo Ticket si lo encontro, de lo contrario retorna null </returns>
        public async Task<Ticket> GetTicket(string id, string _root = "ticket.php")
        {
            Dictionary<string, object> configuracion = AppSettings.getConfigurationParser(_root);
            if (configuracion == null)
                return null;
            User user = await App.Database.GetCurrentUserAsync();
            var request = new RestRequest($"/{configuracion["root"]}?track={id}+&e={user.Email}", Method.GET);
            var response = await client.ExecuteTaskAsync(request);          
               if (response.StatusCode != HttpStatusCode.OK || !response.Content.Contains(id))
               {
                   return null;
               }

               switch (_root)
               {
                  case "ticket.php": return GetTicket_Ticket(response);
                  case "print.php" : return GetTicket_Print(response, configuracion);
                  default: return null;
               } 
       
        }


        private Ticket GetTicket_Ticket(IRestResponse response)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content);
            var nodes = document.DocumentNode.SelectNodes("//td[2]");
            Dictionary<int, string> data = new Dictionary<int, string>();
            int index = 0;
            foreach (var item in nodes)
            {
                data.Add(index, item.InnerText);
                index++;
            }
            var ticket= new Ticket()
            {
                Subject = document.DocumentNode.SelectSingleNode("//h3").InnerText,
                ID = data[3].Split(' ')[0],
                Estado = data[4].Split(' ')[0],
                CreationDate = DateTime.ParseExact(data[5], "dd/MM/yyyy HH:mm:ss", null),
                LastUpdate = DateTime.ParseExact(data[6], "dd/MM/yyyy HH:mm:ss", null),
                UltimaRespuesta = data[7],
                Category = data[8],
                Respuestas = int.Parse(data[9]),
                Priority = data[10],
                UserID = int.Parse(data[20]),
                Classification = data[22],
                Message = document.DocumentNode.SelectSingleNode("//p[2]").InnerText.Replace("&nbsp;", "")
            };
            ticket.Check();
            return ticket;
        }

        /// <summary>
        /// Obtiene un Ticket de la direccion de ticket.php
        /// </summary>
        /// <param name="response"> IRestResponse </param> 
        /// <param name="configuracion"> JSON:  Cofiguracion del Parser (XPath) </param>
        /// <returns> retorna un objeto de tipo Ticket </returns>
        private Ticket GetTicket_Ticket(IRestResponse response, Dictionary<string, object> configuracion)
        {
            Console.WriteLine("ticket.php");
            var document = new HtmlDocument();
            document.LoadHtml(response.Content.Replace("\r\n", "").Replace("\t", ""));
            var node = document.DocumentNode.SelectSingleNode("//body//table//tr[2]");

            var _estadoTicket = GetItemValueNode(node, configuracion, "Estado del ticket");             ///Estado del ticket
           
            Ticket _ticket = new Ticket(){
                  ID = GetItemValueNode(node, configuracion, "ID de seguimiento").Split(' ')[0],          ///ID TICKET
                  Subject = GetItemValueNode(node, configuracion, "Tema"),                                ///Asunto
                  Estado = _estadoTicket,                                                                 ///Estado del ticket

                  CreationDate = DateTime.ParseExact(GetItemValueNode(node, configuracion, "Creado en"), "dd/MM/yyyy HH:mm:ss", null),  ///Fecha de Creacion
                  LastUpdate = DateTime.ParseExact(GetItemValueNode(node, configuracion, "Actualizar"), "dd/MM/yyyy HH:mm:ss", null),   ///Fecha Ultima Actualizacion

                  UltimaRespuesta = GetItemValueNode(node, configuracion, "Última Respuesta"),            ///Ultima Respuesta
                  Respuestas = int.Parse(GetItemValueNode(node, configuracion, "Respuestas")),            ///Respuestas

                  Category = GetItemValueNode(node, configuracion, "Categoria"),                          ///Categoria
                  Priority = GetItemValueNode(node, configuracion, "Prioridad"),                          ///Prioridad
                  Classification = GetItemValueNode(node, configuracion, "Clasificación"),                ///Clasificacion

                  UserID = int.Parse(GetItemValueNode(node, configuracion, "Número de Cuenta")),          ///Número de Cuenta | No. de talento Humano

                  Message = GetItemValueNode(node, configuracion, "Mensaje").Replace("&nbsp;","")         ///Primer Mensaje

              };
          
            return _ticket;
        }

       



        /// <summary>
        /// Obtiene un ticket apartir de la ruta de print.php
        /// </summary>
        /// <param name="document"> Documento HTML </param>
        /// <param name="configuracion"> JSON: Configuracion del Parser </param>
        /// <returns></returns>
        private Ticket GetTicket_Print(IRestResponse response, Dictionary<string, object> configuracion)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content);
            var _estadoTicket = GetItemValue(document, configuracion, "Estado del ticket");             ///Estado del ticket

            Ticket _ticket = new Ticket()
            {

                ID = GetItemValue(document, configuracion, "ID de seguimiento").Split(' ')[0],          ///ID TICKET
                Subject = GetItemValue(document, configuracion, "Tema"),                                ///Asunto
                Estado = _estadoTicket,                                                                 ///Estado del ticket

                CreationDate = DateTime.ParseExact(GetItemValue(document, configuracion, "Creado en"), "dd/MM/yyyy HH:mm:ss", null),  ///Fecha de Creacion
                LastUpdate = DateTime.ParseExact(GetItemValue(document, configuracion, "Actualizar"), "dd/MM/yyyy HH:mm:ss", null),   ///Fecha Ultima Actualizacion

                UltimaRespuesta = GetItemValue(document, configuracion, "Última Respuesta"),            ///Ultima Respuesta

                Category = GetItemValue(document, configuracion, "Categoria"),                          ///Categoria
                Classification = GetItemValue(document, configuracion, "Clasificación"),                ///Clasificacion

                UserID = int.Parse(GetItemValue(document, configuracion, "Número de Cuenta")),          ///Número de Cuenta | No. de talento Humano

                Message = GetItemValue(document, configuracion, "Mensaje"),                              ///Primer Mensaje

            };
            _ticket.Check();
            return _ticket;
        }



        public async Task<string> SendTicket(Ticket _ticket, List<(string, byte[])> _files)
        {
            User user =await App.Database.GetCurrentUserAsync();
            var _perfil = _configuracion.GetIdPerfil(user.Profile);
            var _campus = _configuracion.getIdCampus(user.Campus);

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
                {"custom7",user.PersonalEMail ?? user.Email},
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

        public  async Task<List<Message>> GetMessages(string _idTicket)
        {
            User _user = await App.Database.GetCurrentUserAsync();
            var request = new RestRequest($"/print.php?track={_idTicket}+&e={_user.Email}", Method.GET);
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK || !response.Content.Contains(_idTicket))
            {
                return null;
            }   
            return GetMessages_Print(response);
        }

        private List<Message> GetMessages_Print(IRestResponse response)
        {

            List<Message> mensajes = new List<Message>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response.Content.Replace("\n\r", ""));

            var body = document.DocumentNode.SelectSingleNode("//body");
            var messageContent = document.DocumentNode.SelectNodes("//p");
            int index = 1;
            var owner = "";

            do
            {
                var heads = body.SelectNodes($"table[{index}]//td");
                if (heads == null)
                {
                    break;
                }
                var message = new Message();
                if (index == 1)
                {
                    message.Date = DateTime.ParseExact(heads[7].InnerText, "dd/MM/yyyy HH:mm:ss",null);
                    message.Autor = owner = heads[15].InnerText;

                }
                else
                {
                    message.Date = DateTime.ParseExact(heads[1].InnerText, "dd/MM/yyyy HH:mm:ss",null);
                    message.Autor = heads[3].InnerText;
                }
                message.Text = messageContent[index - 1].InnerText;
                message.EsPropio = message.Autor == owner;
                Console.WriteLine("Fecha: {0} \nMensaje: {1}\nEs Propio: {2}",message.Date,message.Text,message.EsPropio);
                mensajes.Add(message);
                index++;
            } while (true);
            return mensajes;
        }
        private Dictionary<string, string> GetGestiones()
        {
            User _user = App.Database.GetCurrentUser();
            var perfil = _configuracion.GetIdPerfil(_user.Profile);
            var campus = _configuracion.getIdCampus(_user.Campus);
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

        private string GetItemValue(HtmlDocument _document, Dictionary<string, object> _parserServer, string _key)
        {
            return _document.DocumentNode.SelectSingleNode((_parserServer[_key]).ToString()).InnerText;
            //var _temp = _document.DocumentNode.SelectNodes((_parserServer[_key]).ToString()).FirstOrDefault().InnerText;
            //return _temp;
        }
        private string GetItemValueNode(HtmlNode node, Dictionary<string, object> _parserServer, string _key)
        {
            return node.SelectSingleNode((_parserServer[_key]).ToString()).InnerText;
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


        public string GetBaseAdress()
        {
            return BASE_ADDRESS;

        }



        public async Task<string> GetURLTicket(string _idTicket)
        {
            var _user = await App.Database.GetCurrentUserAsync();
            return $"{BASE_ADDRESS}/ticket.php?track={_idTicket}&e={_user.Email}";
        }

        public async Task<bool> getOpenTicket(string id)
        {
            HttpClient _client = new HttpClient();
            User user = await App.Database.GetCurrentUserAsync();
            HttpResponseMessage response = await _client.GetAsync(BASE_ADDRESS + "/ticket.php?track=" + id+"&e="+user.Email);
            string html = await response.Content.ReadAsStringAsync();
            return html.IndexOf("resolved") == -1;
        }

        public async Task<bool> changeStatusTicket(string id)
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
            return response != null;
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

        private string GetMessageFrom(int index, HtmlDocument document,HtmlNode node)
        {
            //html//body//table//tr[2]//table[2]//tr[2]//table
            HtmlNode element = null;
            if (index == 1)
            {
                                                         
               element= document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]/td[2]/table//tr//table//tr//table//tr[2]//td[2]");
                return element.InnerText;
            }
            //  element = document.DocumentNode.SelectSingleNode($"//html/body//table//tr[2]//table[2]//tr[2]//table//tr[{index}]//table//tr//table//tr[2]/td[2]");
                                                               //html//body//table//tr[2]//table[2]//tr[2]//table
            element = node.SelectSingleNode($"//tr[{index}]//table//tr//table//tr[2]/td[2]");
            if (element != null)
                return element.InnerText;
            else
                return null;

        }
        private string GetMessageText(int n, HtmlNode node)
        {
            HtmlNode element = node.SelectSingleNode($"//tr[{n}]//p[2]");
          //  var element = document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]//table//tr[{n}]//p[2]");
            if (element != null)
                return element.InnerText.Replace("&nbsp;", "");
            else
                return null;
        }
        private DateTime GetMessageDate(int index, HtmlDocument document, HtmlNode node)
        {
            HtmlNode element = null;// document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]/td[2]/table//tr//table//tr//table//tr//td[2]");
            if (index == 1)
            {
                element= document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]/td[2]/table//tr//table//tr//table//tr//td[2]");
                return DateTime.ParseExact(element.InnerText, "dd/MM/yyyy HH:mm:ss", null);
            }
                                             
            element = document.DocumentNode.SelectSingleNode($"//html//body//table//tr[2]//table[2]//tr[2]//table//tr[{index}]//table//tr//table//td[2]");
            if (element != null)
            {
                Console.WriteLine("Test Fecha: "+element.InnerText);
                return DateTime.ParseExact(element.InnerText, "dd/MM/yyyy HH:mm:ss", null);
            }
            else
                return DateTime.MinValue;
        }
    }
}