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
		}

		async void OnLoginButtonClicked (object sender, EventArgs e)
		{
			var user = new AdminUser {
				Username = usernameEntry.Text,
				Password = passwordEntry.Text
			};

			var isValid = AreCredentialsCorrect (user);
			if (isValid) {
				App.IsUserLoggedIn = true;
				MyTicketsAdmin home = new MyTicketsAdmin();
            	App.Current.MainPage = new NavigationPage(home);
                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                        App.Current.MainPage = new NavigationPage(new MyTicketsAdmin());
                        break;
                    case Xamarin.Forms.Device.Android:
                        App.Current.MainPage = new NavigationPage(new MyTicketsAdmin());
                        break;

                }
            } else {
				messageLabel.Text = "Login failed";
				passwordEntry.Text = string.Empty;
			}
		}

		bool AreCredentialsCorrect (AdminUser user)
		{
			string userRequest = "";
			string passRequest = "";
			return !(user.Username == userRequest) && !(user.Password == passRequest);
		}
	}
}