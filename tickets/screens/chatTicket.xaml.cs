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
using tickets.ViewModels;
using tickets.Views;

namespace tickets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]


    public partial class chatTicket : ContentPage
    {
        public ObservableRangeCollection<Message> ListMessages { get; }
        private Server server = new Server();
        public List<(string, byte[])> files = new List<(string, byte[])>();
        public string ticketID = null;
        public string stateText {get;set;}
       
        public chatViewModel chatVM;
        public chatTicket()
        {
            try
            {
                InitializeComponent();
               
                
                chatVM = new chatViewModel(ticketID, files);
                     
                chatVM.ListMessages.CollectionChanged += (sender, e) =>
                {
                    var target = chatVM.ListMessages[chatVM.ListMessages.Count - 1];
                    MessagesListView.ScrollTo(target, ScrollToPosition.End, true);
                    MessagesListView.IsPullToRefreshEnabled = true;


                };
                ListMessages = new ObservableRangeCollection<Message>();
 
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

           
        }

        private async Task openBrowser()
        {
            var uri = await server.GetURLTicket(ticketID);
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        private async Task deleteTicket()
        {
            bool answer = await DisplayAlert("Alerta!", "¿Estas seguro que deseas eliminar este ticket?", "Si", "No");
            if (answer)
            {
                App.Database.EliminarTicket(ticketID);
                App.Current.MainPage = new NavigationPage(new ListTicketsView());

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
                readTicket();
                Console.WriteLine("Mensajes Cargados");
                openTicket.Text = await getSateText();


            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Se ha producido un error inesperado daremos soporte los mas antes posible\nDetalles Error:" + ex.InnerException, "Aceptar");
                Console.WriteLine(ex.StackTrace + "\nMensaje: " + ex.Message);
            }
        }

        public async void readTicket()
        {
            Console.WriteLine(ticketID);
            List<Message> mensajes =await server.GetMessages(ticketID);
            if (mensajes != null)
            {
                Console.WriteLine(mensajes.Count);
                foreach (var mensaje in mensajes)
                {
                    chatVM.ListMessages.Add(mensaje);
                }
            }
            else
            {
                Console.WriteLine("No se ha cargado ningun mensaje");
               
            }
            
           
           
           
            //Loading.IsVisible = false;
        }


        async Task switchState()
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

        private async void ToolbarItem_Clicked(object sender, EventArgs e)

        {
            var _ticket = await App.Database.GetTicket(ticketID);
            await Navigation.PushAsync(new DetalleTicketView() { BindingContext = new DetalleTicketViewModel(_ticket)});
        }

        async Task<string> getSateText()
        {
            return  await server.getOpenTicket(ticketID) ? "Cerrar Ticket" : "Abrir Ticket";
        }

        private async void DeleteTicket(object sender, EventArgs e)

        {
            await deleteTicket();
        }

        private async void GotoBrowser(object sender, EventArgs e)

        {
            await openBrowser();
        }

        private async void changeStatusTicket(object sender, EventArgs e)

        {
            await switchState();
        }



        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {

        }
    }

}

