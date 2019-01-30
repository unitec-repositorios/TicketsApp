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
				Navigation.InsertPageBefore (new HomeScreen(), this);
				await Navigation.PopAsync ();
			} else {
				messageLabel.Text = "Login failed";
				passwordEntry.Text = string.Empty;
			}
		}

		bool AreCredentialsCorrect (AdminUser user)
		{
			string userRequest = "";
			string passRequest = "";
			return user.Username == userRequest && user.Password == passRequest;
		}
	}
}