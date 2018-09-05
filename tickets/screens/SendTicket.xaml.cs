using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Net.Http;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System.Collections.ObjectModel;
using tickets.API;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tickets
{
	public partial class SendTicket : ContentPage
	{
        private Server server = new Server();
        private User user;
        List<String> filesNames = new List<String>();
        List<FileData> loadFiles = new List<FileData>();

        public SendTicket ()
		{
			InitializeComponent ();
            Adjun.HasUnevenRows = true;
            Append.Clicked += searchFile;
            this.BindingContext = new Ticket();
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
                    await DisplayAlert("Advertencia", "No es posible acceder a los datos del archivo", "OK");

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", "Se produjo un error", "OK");
            }
        }

        async void OnSubmit(object sender, System.EventArgs e)
        {
            var valid = !String.IsNullOrWhiteSpace(number.Text) && !String.IsNullOrWhiteSpace(subject.Text) && !String.IsNullOrWhiteSpace(message.Text);
                if (valid)
                {
                    try
                    {
                        string response = await server.submitTicket(number.Text, subject.Text, message.Text, (pickerPriority.SelectedIndex + 1) + "", picker.Items[picker.SelectedIndex]);
                        if (response.Equals("error"))
                        {
                            await DisplayAlert("Ticket no se ha podido enviar", "Revise por favor", "OK");
                        }
                        else
                        {
                            
                            await DisplayAlert("Ticket ha sido enviado", "Ticket ID: " + response, "OK");
                            //clean
                            number.Text = "";
                            subject.Text = "";
                            message.Text = "";
                            picker.SelectedIndex = 1;
                            pickerPriority.SelectedIndex = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Error= "+ex, "OK");
                    }
                
                }
                else
                {
                    await DisplayAlert("Advertencia", "Favor llene todos los campos", "OK");
                }

        }

    }
}