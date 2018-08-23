using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http.Headers;
using System.Net.Http;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace tickets
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

	public partial class SendTicket : ContentPage
	{
        private HttpClient _client = new HttpClient();
        List<String> filesNames = new List<String>();
        List<FileData> loadFiles = new List<FileData>();
        private ObservableCollection<Post> _post;
        private User user;

        public SendTicket ()
		{
			InitializeComponent ();
            Append.Clicked += searchFile;
            Send.Clicked += OnAdd;
        }

        private async void searchFile(object sender, EventArgs e)
        {
            try
            {
                FileData file = await CrossFilePicker.Current.PickFile();
                if (file != null)
                {
                    string name = file.FileName;
                    filesNames.Add(name);
                    loadFiles.Add(file);
                    Adjun.ItemsSource = null;
                    Adjun.ItemsSource = filesNames;
                }
                else
                {
                    DisplayAlert("Advertencia", "No es posible acceder a los datos del archivo", "OK");

                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Aviso", "Se produjo un error", "OK");
            }
        }
        async void OnAdd(object sender, System.EventArgs e)
        {
            User user = await App.Database.GetCurrentUser();
            if (user != null)
            {
                var invalid = String.IsNullOrWhiteSpace(number.Text) && String.IsNullOrWhiteSpace(subject.Text) && String.IsNullOrWhiteSpace(message.Text);
                if (!invalid)
                {
                    //http call
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
                    string custom5Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom5", picker.Items[picker.SelectedIndex]);
                    byte[] custom5InputBytes = ascii.GetBytes(custom5Input);
                    //custom15
                    string custom15Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom15", user.PhoneNumber);
                    byte[] custom15InputBytes = ascii.GetBytes(custom15Input);
                    //custom20
                    string custom20Input = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "custom20", number.Text);
                    byte[] custom20InputBytes = ascii.GetBytes(custom20Input);
                    //category
                    string categoryInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "category", "category");
                    byte[] categoryInputBytes = ascii.GetBytes(categoryInput);
                    //priority
                    string priorityInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "priority", pickerPriority.SelectedIndex.ToString());
                    byte[] priorityInputBytes = ascii.GetBytes(priorityInput);
                    //subject
                    string subjectInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "subject", subject.Text);
                    byte[] subjectInputBytes = ascii.GetBytes(subjectInput);
                    //message
                    string messageInput = String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", "message", message.Text);
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
                    string ticketID = "";
                    await DisplayAlert("Ticket ha sido enviado", "Ticket ID: " + ticketID, "OK");
                }
                else
                {
                    await DisplayAlert("Advertencia", "Favor llene todos los campos", "OK");
                }
            }
            else {
                DisplayAlert("Aviso", "No se ha encontrado datos del usuario", "OK");
            }
        }
    }
}