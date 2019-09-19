using System;
using Xamarin.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Text;

using Xamarin.Forms.Xaml;


namespace tickets
{
	public partial class SignInAdminPage : ContentPage
	{
		public SignInAdminPage ()
		{
			InitializeComponent();
			usernameEntry.Text = App.UserEmail;
		}

		async void OnSignInButtonClicked (object sender, EventArgs e)
		{
			string password = passwordEntry.Text;
            string password2 = passwordEntry2.Text;
		//	string encriptionKey = "ticketsApp";
			var isValid = ValidatePasswords (password, password2);
			if (isValid) {
                string encryptedPassword = App.Database.encryptPassword(password);
                var admin = new AdminUser {
                    Username = usernameEntry.Text,
                    Password = encryptedPassword
                };
				if (!(string.IsNullOrEmpty(admin.Username)
                     || string.IsNullOrEmpty(admin.Password))){
					admin.ID = 0;
					await App.Database.CreateNewCurrentAdminUser(admin);
					await DisplayAlert("Enhorabuena*", "Su usuario ha sido creado exitosamente!", "Aceptar");
					admin.PrintData();
					AppSettingsPage settings = new AppSettingsPage();
            		App.Current.MainPage = new NavigationPage(settings);
				}else{
					await DisplayAlert("Error", "Debe ingresar los datos requeridos", "Aceptar");
				}
			} else {
				messageLabel.Text = "ERROR LAS CONTRASEÑAS NO SON IGUALES";
				passwordEntry.Text = string.Empty;
			}
		}

        bool ValidatePasswords (string pass1, string pass2)
		{
			if(!(pass1.Equals("")) && !(pass2.Equals(""))){
				return pass1.Equals(pass2);
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