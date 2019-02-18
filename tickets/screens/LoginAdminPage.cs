using System;
using Xamarin.Forms;
using System.Text;

using Xamarin.Forms.Xaml;


namespace tickets
{
	public partial class LoginAdminPage : ContentPage
	{
		public LoginAdminPage ()
		{
			InitializeComponent();
			usernameEntry.Text = App.UserEmail;
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