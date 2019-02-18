using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tickets.API;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Xamarin.Forms;
using Xamarin.Essentials;

using Xamarin.Forms.Xaml;
using Xamarin.Forms;


namespace tickets.screens
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginAdminPage : ContentPage
	{        private Server server = new Server();
        private Login_Admin login_ = new Login_Admin();
		public LoginAdminPage ()
		{
			InitializeComponent();
		}

		public async void SendRequest(object sender, System.EventArgs e)
        {
            var valid = !String.IsNullOrWhiteSpace(usernameEntry.Text) && !String.IsNullOrWhiteSpace(passwordEntry.Text);
            if (valid)
            {

            if (CheckInternetConnection())
            {

                try
                {
                    string response = await login_.loginAdmins(usernameEntry.Text, passwordEntry.Text);
                    if (response == " error")
                    {
                        await DisplayAlert("No se ha podido Acceder como Admin", "Revise por favor", "OK");
                    }
                    else if(response == "sucess")
                    {

                        switch (Xamarin.Forms.Device.RuntimePlatform)
                        {
                            case Xamarin.Forms.Device.iOS:
                                App.Current.MainPage = new NavigationPage(new HomeScreen());
                                break;
                            case Xamarin.Forms.Device.Android:
                                App.Current.MainPage = new NavigationPage(new MyTicketsAdmin());
                                break;

                        }

                    }
                }
                catch
                {
                    await DisplayAlert("Error", "Revise el Admin", "OK");
                }
            }
            else
            {
                    await DisplayAlert("No hay conexión", "No se detecto una conexión a Internet. Por favor vuelta a intentarlo", "Ok");
            }

            }


            else
            {
                await DisplayAlert("Error", "Ingrese Datos", "OK");

            }
        }
        public bool CheckInternetConnection()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

		}
}