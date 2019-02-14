using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using tickets.API;
using tickets.Models;
using System.IO;
using Plugin.Media;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using MvvmHelpers;
using Acr.UserDialogs;
using Xamarin.Essentials;

namespace tickets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]


    public partial class chatTicket : ContentPage
    {
        public ObservableRangeCollection<Message> ListMessages { get; }
        private Server server = new Server();
        public List<(string, byte[])> files = new List<(string, byte[])>();
        public string ticketID = null;
        private string messageRef = "<p><b>Mensaje:</b></p>";
        private string autorRef = "<td class=\"tickettd\">";
        public string stateText {get;set;}

        public chatViewModel chatVM;
        public chatTicket()
        {
            try
            {
                InitializeComponent();
                this.BindingContext = this;
                chatVM = new chatViewModel(ticketID, files);
                stateText = "Probando";
                chatVM.ListMessages.CollectionChanged += (sender, e) =>
                {
                    var target = chatVM.ListMessages[chatVM.ListMessages.Count - 1];
                    MessagesListView.ScrollTo(target, ScrollToPosition.End, true);
                    MessagesListView.IsPullToRefreshEnabled = true;


                };
                ListMessages = new ObservableRangeCollection<Message>();

                var openTicket = new ToolbarItem
                {
                    Text = "prueba",
                    Command = new Command(execute: () => switchState()),

                    Order = ToolbarItemOrder.Secondary

                };

                var openBrowserTool = new ToolbarItem
                {
                    Text = "Abrir en el navegador",
                    Command = new Command(execute: () => openBrowser()),
                    Order = ToolbarItemOrder.Secondary
                };


                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        ToolbarItems.Add(openTicket);
                        ToolbarItems.Add(openBrowserTool);
                        break;
                    case Device.UWP:
                        ToolbarItems.Add(openTicket);
                        break;
                }//*/

            }
            catch (Exception ex)
            {
                throw ex;
            }



        }


        //Open In Browser

        private async void openBrowser()
        {
            string refresh = await server.getRefresh();
            string uri = server.getBaseAdress() + "/ticket.php?track=" + ticketID + "&Refresh=" + refresh;
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        /*
             private async void enviarMensaje(object sender, EventArgs args)
             {           

                 if (!String.IsNullOrWhiteSpace(mensajeChat.Text))
                 {
                     var message = new Message
                     {
                         Text = mensajeChat.Text,
                         Files = files,
                         IsTextIn = false,
                         MessageDateTime = DateTime.Now
                     };

                     //await DisplayAlert("Notificacion", "Enviando mensaje...", "Ok");

                     sendMessage(message);


                 }
                 else
                 {
                     await DisplayAlert("Notificacion", "Ingrese mensaje", "Ok");
                 }

             }

             public async void sendMessage(Message message)
             {
                 Loading.IsVisible = true;
                 string status = await server.replyTicket(message.Text, message.Files, ticketID);
                 //await DisplayAlert("Notificacion del server", status, "Ok");
                 if (status.Equals("ok"))
                 {

                     mensajeChat.Text = "";
                     await DisplayAlert("Notificacion", "Mensaje Enviado!", "Ok");
                     Loading.IsVisible = false;

                     ListMessages.Add(message);

                 }
                 else
                 {
                     await DisplayAlert("Notificacion", "No se pudo enviar el mensaje...", "Ok");
                     //OutText = this.ticketID;
                 }
             }

         */

        private async void take_Photo(object sender, EventArgs args)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "photo",
                Name = "photo" + files.Count + ".jpg",
                CompressionQuality = 25


            });

            if (file == null)
                return;
            string filePath = file.Path;
            byte[] data = MediaFileBytes(file);

            files.Add(("photo" + files.Count + ".jpg", data));
            string temp = "";
            for (int i = 0; i < files.Count(); i++)
            {
                temp += files[i].Item1;
                temp += "\n";
            }
            Adjun.Text = temp;
            //await DisplayAlert("File Location", filePath, "OK");
        }



        byte[] MediaFileBytes(Plugin.Media.Abstractions.MediaFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                return memoryStream.ToArray();
            }
        }

        private async void searchFile(object sender, EventArgs e)
        {
            try
            {
                FileData file = await CrossFilePicker.Current.PickFile();
                if (file != null)
                {
                    string name = file.FileName;
                    var data = file.DataArray;
                    files.Add((name, data));
                    //loadFiles.Add(file);
                    //Adjun.ItemsSource = null;
                    string temp = "";
                    for (int i = 0; i < files.Count(); i++)
                    {
                        temp += files[i].Item1;
                        temp += "\n";
                    }
                    Adjun.Text = temp;
                }
                else
                {
                    await DisplayAlert("Advertencia", "No es posible acceder a los datos del archivo", "OK");

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
                await DisplayAlert("Aviso", "Se produjo un error", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                if (ticketID == null)
                {
                    ticketID = (string)BindingContext;
                    Title = "Ticket No. " + ticketID;
                }
                BindingContext = chatVM = new chatViewModel(ticketID, files);
                ToolbarItems.Clear();
                var openTicket = new ToolbarItem
                {
                    Text = await getSateText(),
                    Command = new Command(execute: () => switchState()),

                    Order = ToolbarItemOrder.Secondary

                };
                var openBrowserTool = new ToolbarItem
                {
                    Text = "Abrir en el navegador",
                    Command = new Command(execute: () => openBrowser()),
                    Order = ToolbarItemOrder.Secondary
                };

                ToolbarItems.Add(openTicket);
                ToolbarItems.Add(openBrowserTool);
                readTicket();
                
 

                

            }
            catch (Exception ex)
            {
            }
        }

        public async void readTicket()
        {
            string html = await server.getTicket(ticketID);
            string autor = "";
            string message = "";
            string myName = null;
            int position = html.IndexOf(autorRef + "N");
            int index = position + autorRef.Count();
            while (position != -1)
            {
                html = html.Substring(index);
                autor = getAutor(html);
                if (myName == null)
                {
                    myName = autor;
                }
                position = html.IndexOf(messageRef);
                index = position + messageRef.Count();
                if (position != -1)
                {
                    html = html.Substring(index);
                    message = getMessage(html);
                    bool typeText = true;
                    //add new message to the chat
                    if (autor.Equals(myName))
                    {
                        autor = "";
                        typeText = false;
                    }
                    else
                    {
                        autor += ":\n";
                    }
                    var mymessage = new Message
                    {
                        Text = autor + message,
                        IsTextIn = typeText,
                        //need to correct the time message
                        MessageDateTime = DateTime.Now
                    };
                    chatVM.ListMessages.Add(mymessage);
                }
                position = html.IndexOf(autorRef + "N");
                index = position + autorRef.Count();
            }

            messageComponent.IsVisible = await server.getOpenTicket(ticketID);
            //Loading.IsVisible = false;
        }
        public string getAutor(string html)
        {
            string autor = "";
            string supportString = "";
            int index = 0;
            while (index != -1)
            {
                if (supportString.Contains(autorRef))
                {
                    if (html[index] != '<')
                    {
                        autor += html[index];
                    }
                    else
                    {
                        index = -2;
                    }
                }
                else
                {
                    supportString += html[index];
                }
                index++;
            }
            return autor;
        }
        public string getMessage(string html)
        {
            string Mimessage = "";
            string supportString = "";
            int index = html.IndexOf("<p>") + 3;
            int endMessage = html.IndexOf("</p>");
            int endMessage2 = html.IndexOf("&nbsp;</p>");
            bool tag = false;
            if (endMessage2 < 0)
            {
                endMessage2 = endMessage;
            }
            while (index < endMessage && index < endMessage2)
            {
                if (html[index] == '<')
                {
                    supportString += html[index];
                    tag = true;
                }
                else if (html[index] == '>')
                {
                    supportString += html[index];
                    if (supportString.Equals("<br />"))
                    {
                        Mimessage += "\n";
                    }
                    supportString = "";
                    tag = false;
                }
                else if (tag)
                {
                    supportString += html[index];
                }
                else
                {
                    Mimessage += html[index];
                }
                index++;
            }
            return Mimessage;
        }

        async void switchState()
        {           
            string close = await server.getOpenTicket(ticketID) ? "cerrar" : "abrir";
            bool answer = await DisplayAlert("Alerta!", "¿Estas seguro que deseas " + close +" el ticket?", "Yes","No");
            UserDialogs.Instance.ShowLoading("");
            if (answer)
            {
                messageComponent.IsVisible = close == "abrir";
                await server.changeStatusTicket(ticketID);
                //Loading.IsVisible = false;
                string open = close == "abrir" ? "abierto" : "cerrado"; 
                await DisplayAlert("Operanción exitosa", "El estado del ticket " + ticketID + " ha sido " + open, "OK");
                ToolbarItems.Clear();
                var openTicket = new ToolbarItem
                {
                    Text = await getSateText(),
                    Command = new Command(execute: () => switchState()),

                    Order = ToolbarItemOrder.Secondary

                };
                ToolbarItems.Add(openTicket);
            }
            UserDialogs.Instance.HideLoading();
        }

        async Task<string> getSateText()
        {
            return  await server.getOpenTicket(ticketID) ? "Cerrar Ticket" : "Abrir Ticket";
        }


        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }
    }

}

