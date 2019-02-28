using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace tickets
{
    public class LoginAdmin
    {
        private static LoginAdmin  _instance;
     private LoginAdmin()
        {
            _instance = new LoginAdmin();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        }
        public static LoginAdmin Instance {
            get{
                if(_instance == null){
                    _instance = new LoginAdmin();
                }
                return _instance;
            }
        }
        public string username { get; set; }
        public string password { get; set; }
        public string cookie { get; set;}

        public async Task<string> loginAdmins()
        {
            const string BASE_ADDRESS_ADMIN = "http://138.197.198.67/admin";
            var html = @"" + BASE_ADDRESS_ADMIN + "/index.php";
            string temporal_response;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage capture = await httpClient.GetAsync(html);
            MultipartFormDataContent form = new MultipartFormDataContent();

            String res = capture.Headers.ElementAt(3).Value.ElementAt(0).ToString();
            String[] tokens = res.Split(';');
            String cookie = tokens[0];
            String[] tokensValue = cookie.Split('=');
            String valueCookie = tokensValue[1];

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await capture.Content.ReadAsStringAsync());


            Encoding encoder = Encoding.GetEncoding("ISO-8859-1");
            string no_thanks = "NOTHANKS";
            string do_login = "do_login";
            form.Headers.Add("Cookie", cookie);
            form.Headers.ContentType.CharSet = "ISO-8859-1";
            form.Add(new StringContent(username), "user");
            form.Add(new StringContent(password), "pass");
            form.Add(new StringContent(no_thanks), "remember_user");
            form.Add(new StringContent(do_login), "a");
            HttpResponseMessage response = await httpClient.PostAsync(BASE_ADDRESS_ADMIN + "/index.php", form);

            string cookie_response = response.Headers.ElementAt(3).Value.ElementAt(0).ToString();
            tokens = cookie_response.Split(';');
            String cookie_ = tokens[0];
            cookie = cookie_;
            response.EnsureSuccessStatusCode();

            httpClient.Dispose();
            string sd = await response.Content.ReadAsStringAsync();
            var result = new HtmlDocument();
            result.LoadHtml(sd);

            var success = result.DocumentNode.SelectSingleNode("//div[@class='error']");
            if (success == null)
            {
                temporal_response = "sucess";

            }
            else
            {
                temporal_response = "error";

                ;
            }
            return temporal_response;
        }

    }
}






