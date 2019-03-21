using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tickets.API;
using tickets.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Xamarin.Forms;
using Xamarin.Essentials;

using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using Acr.UserDialogs;
using SQLite;

namespace tickets
{
	public partial class LoginAdminPage : ContentPage
	{
        private AdminLogin admin_log;// = AdminLogin.Instance;
        public LoginAdminPage ()
		{
			InitializeComponent();
            admin_log = App.Database.GetAdminLogin();
            if (admin_log != null)
            {
                usernameEntry.Text = admin_log.username;
                passwordEntry.Text = admin_log.password;
            }
        }

		async void OnLoginButtonClicked (object sender, EventArgs e)
		{
            if (admin_log == null)
            {
                admin_log = AdminLogin.Instance;
                admin_log.username = usernameEntry.Text;
                admin_log.password = passwordEntry.Text;
            }
        
        
          
            var valid = !String.IsNullOrWhiteSpace(usernameEntry.Text) && !String.IsNullOrWhiteSpace(passwordEntry.Text);
            if (valid)
            {
                UserDialogs.Instance.ShowLoading();
                if (CheckInternetConnection())
                {
                    try
                    {
                        string response = await admin_log.loginAdmins();

                        if (response == "error")
                        {
                            await DisplayAlert("Error", "Usuario o Contraseña de Administrador incorrecto.", "OK");
                            UserDialogs.Instance.HideLoading();
                        }
                        else if (response == "sucess")
                        {
                            
                            usernameEntry.IsVisible = false;
                            passwordEntry.IsVisible = false;
                            labelusers.IsVisible = false;
                            labelpasss.IsVisible = false;
                            button_login.IsVisible = false;
                            //Loading.IsVisible = true;
                            UserDialogs.Instance.HideLoading();
                            UserDialogs.Instance.ShowSuccess("Acceso Correcto");
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