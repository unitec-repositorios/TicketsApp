﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Net.Http;
using Plugin.FilePicker;
using Plugin;
using Plugin.FilePicker.Abstractions;
using System.Collections.ObjectModel;
using tickets.API;
using Plugin.Clipboard;
using Plugin.Media;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;


namespace tickets
{
    public partial class SendTicket : ContentPage
    {
        private Server server;
        public List<string> Categorias;
        public ObservableCollection<string> Areas;
        private Dictionary<string, List<string>> AreasxCategorias;
   
       
        public bool sentTicket;
        List<(string, byte[])> files = new List<(string, byte[])>();
        //List<FileData> loadFiles = new List<FileData>();
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; }
        }


        public SendTicket()
        {
           
            InitializeComponent();
            initDatos();
            this.BindingContext = new Ticket();


        }

        private void initDatos()
        {
          
                server = new Server();
                AreasxCategorias = server.getDictionaryAreasXCategorias();
            
            if (Areas == null)
            {

                Areas = new ObservableCollection<string>(AreasxCategorias.Keys);
            }
           
            picker_Areas.ItemsSource = Areas;
            picker_Areas.SelectedIndex = 0;
            Categorias = AreasxCategorias[picker_Areas.SelectedItem.ToString()];
            picker_categories.ItemsSource = Categorias;
            picker_categories.SelectedIndex = 0;
        //    Categorias = new ObservableCollection<string>(server.GetDictionaryCategory((string)picker_Areas.ItemsSource[selectedIndex]).Keys);
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
            //await DisplayAlert("File Location", filePath, "OK");
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

        public async void OnSubmit(object sender, System.EventArgs e)
        {
           
            var valid = !String.IsNullOrWhiteSpace(subject.Text) && !String.IsNullOrWhiteSpace(message.Text);
            if (valid)
            {
                try
                {
                    UserDialogs.Instance.ShowLoading("Enviando Ticket...");
                    server = new Server();
                    string _keyArea = picker_Areas.SelectedItem.ToString();
                    var _tempArea = server.GetValueArea(_keyArea);
                    var _tempCategory = server.GetValueCategoria(_keyArea, picker_categories.SelectedItem.ToString());
                    Ticket _ticket = new Ticket()
                    {
                        ID = "",
                        Area=_tempArea,
                        Category=_tempCategory,
                        Priority = ""+(pickerPriority.SelectedIndex + 1),
                        Subject = subject.Text,
                        Message = message.Text,
                        Classification = picker.SelectedItem.ToString(),

                    };
                    // string response = await server.submitTicket("0", subject.Text, message.Text, (pickerPriority.SelectedIndex + 1) + "", picker.Items[picker.SelectedIndex],_ticket, files);
                    string response;
                     response= await server.SendTicket(_ticket, files);
          
                       
                    if (response.Equals("error"))
                    {
                        await DisplayAlert("Ticket no se ha podido enviar", "Revise por favor", "OK");
                        this.sentTicket = false;
                        UserDialogs.Instance.HideLoading();
                    }
                    else
                    {
                        //   string date = await server.getInitDate(response);
                        _ticket = await server.GetTicket(response);
                        await App.Database.AgregarTicket(_ticket);
                        UserDialogs.Instance.ShowSuccess("Ticket Enviado!");
                        bool copy = await DisplayAlert("Ticket ha sido enviado", "Ticket ID: " + response, "OK", "Copiar Ticket ID");
                        
                        if (!copy)
                        {
                            CrossClipboard.Current.SetText(response);
                           // App.Database.AgregarTicket(await server.GetTicket(response));
                        }
                        this.sentTicket = true;
                        //clean

                        

                        
                        subject.Text = "";
                        message.Text = "";
                        picker.SelectedIndex = 1;
                        pickerPriority.SelectedIndex = 1;
                        App.Current.MainPage = new NavigationPage(new MyTickets());
                        await Navigation.PopAsync();
                        
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine(ex.StackTrace + " ----- "+ex.TargetSite );
                    await DisplayAlert("Error", "Error= " + ex, "OK");
                }
            }
            else
            {
                await DisplayAlert("Advertencia", "Favor llene todos los campos", "OK");
            }
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

        private void Picker_Areas_SelectedIndexChanged(object sender, EventArgs e)
        {
         //   int SelectedIndex = picker_Areas.SelectedIndex;
            Categorias = AreasxCategorias[picker_Areas.SelectedItem.ToString()];
            picker_categories.ItemsSource = Categorias;

            picker_categories.SelectedIndex = 0;
        }
    }
}