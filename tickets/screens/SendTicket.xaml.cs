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

namespace tickets.screens
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SendTicket : ContentPage
	{
        private HttpClient _client = new HttpClient();
        List<String> filesNames = new List<String>();
        List<FileData> loadFiles = new List<FileData>();
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