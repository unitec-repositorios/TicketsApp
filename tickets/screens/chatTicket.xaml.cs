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
       
        private ToolbarItem openTicket,openBrowserTool,deleteTicketT;
       // public static bool deletedTicket = false;
        public chatViewModel chatVM;
        public chatTicket()
        {
            try
            {
                InitializeComponent();
                this.BindingContext = this;
                chatVM = new chatViewModel(ticketID, files);
       
                
                chatVM.ListMessages.CollectionChanged += (sender, e) =>
                {
                    var target = chatVM.ListMessages[chatVM.ListMessages.Count - 1];
                    MessagesListView.ScrollTo(target, ScrollToPosition.End, true);
                    MessagesListView.IsPullToRefreshEnabled = true;


                };
                ListMessages = new ObservableRangeCollection<Message>();
                
               openTicket = new ToolbarItem
                {
                   Icon = "trash.png",
                    Text = "Abrir Ticket",
                    Command = new Command(execute: () => switchState()),

                    Order = ToolbarItemOrder.Secondary

                };

                openBrowserTool = new ToolbarItem
                {
                    Icon = "trash.png",
                    Text = "Mas detalles",
                    Command = new Command(execute: () => openBrowser()),
                    Order = ToolbarItemOrder.Secondary
                };
                deleteTicketT = new ToolbarItem
                {
                    Icon="trash.png",
                    Text = "Eliminar Ticket",
                    Command = new Command(execute: () => deleteTicket()),
                    Order = ToolbarItemOrder.Secondary
                };


                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        ToolbarItems.Add(openTicket);
                        ToolbarItems.Add(openBrowserTool);
                        ToolbarItems.Add(deleteTicketT);
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

        private async void openBrowser()
        {
            string refresh = await server.getRefreshCode();
            string uri =$"{server.GetBaseAdress()}/ticket.php?track={ticketID}&Refresh={refresh}";
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        private async void deleteTicket()
        {
            bool answer = await DisplayAlert("Alerta!", "¿Estas seguro que deseas eliminar este ticket?", "Si", "No");
            if (answer)
            {
                App.Database.EliminarTicket(ticketID);
                App.Current.MainPage = new NavigationPage(new MyTickets());

            }


        }

        private async void take_Photo(object sender, EventArgs args)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
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
            this.chatVM.Files.Add(files[files.Count-1]);
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
                Console.WriteLine("Cargando Mensajes");
                await readTicket();
                Console.WriteLine("Mensajes Cargados");
                openTicket.Text = await getSateText();
              
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + "\nMensaje: " + ex.Message);
            }
        }

        public async Task readTicket()
        {
            List<Message> mensajes = await server.GetMessages(ticketID);
           
            string myName = mensajes.First().Autor;
            foreach(var mensaje in mensajes)
            {
                if (mensaje.Autor == myName)
                {
                    mensaje.EsPropio = true;
                }
                chatVM.ListMessages.Add(mensaje);
            }
           
            //Loading.IsVisible = false;
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
                openTicket.Text = await getSateText();
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

