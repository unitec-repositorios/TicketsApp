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


namespace tickets
{
	public partial class LoginAdminPage : ContentPage
	{
        private Server server = new Server();
        private Login_Admin login_ = new Login_Admin();
        public LoginAdminPage ()
		{
			InitializeComponent();
          usernameEntry.Text = App.UserEmail;
		}
        public async void SendRequest(object sender, System.EventArgs e)
        {
            var valid = !String.IsNullOrWhiteSpace(usuario.Text) && !String.IsNullOrWhiteSpace(contrasena.Text);
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
                        else if (response == "sucess")
                        {
                            label_contrasena.IsVisible = false;
                            usuario.IsVisible = false;
                            contrasena.IsVisible = false;
                            label_usuario.IsVisible = false;
                            login.IsVisible = false;
                            Loading.IsVisible = true;

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

		async void OnLoginButtonClicked (object sender, EventArgs e)
		{
			string password = passwordEntry.Text;
            string encryptedPassword = App.Database.encryptPassword(password);
            var admin = new AdminUser {
				Username = usernameEntry.Text,
				Password = encryptedPassword
			};
			var isValid = AreCredentialsCorrect (admin);
			if (isValid) {
				MyTicketsAdmin home = new MyTicketsAdmin();
            	App.Current.MainPage = new NavigationPage(home);
            } else {
				messageLabel.Text = "Contraseña incorrecta";
				passwordEntry.Text = string.Empty;
			}
		}
		
		private void  SignInButtonClicked(object sender, EventArgs e)
        {
			User usr = App.Database.GetUserAsync(App.UserEmail);
			if((usr.Profile).Equals("Administrativo")){
                SignInAdminPage signIn = new SignInAdminPage();
            	App.Current.MainPage = new NavigationPage(signIn);
			}else{
                DisplayAlert("Error", "Su no tiene permisos para esta operacion", "Aceptar");
			}
        }
        bool AreCredentialsCorrect (AdminUser admin)
		{
			AdminUser findUser = App.Database.GetCurrentAdminUserNotAsync();
			if(findUser != null){
				if (!admin.Password.Equals("")){
					return (findUser.Password).Equals(admin.Password);
				}
			}
			return false;
		}

		async void OnCancelTouched(object sender, System.EventArgs e)
        {
            AppSettingsPage settings = new AppSettingsPage();
            App.Current.MainPage = new NavigationPage(settings);
        }
	}
}