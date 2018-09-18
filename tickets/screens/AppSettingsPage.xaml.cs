using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace tickets
{
    public partial class AppSettingsPage : ContentPage
    {
        public AppSettingsPage()
        {
            InitializeComponent();
        }

        public async void goToUserSettings(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new UserSettingsPage());
        }

        private async void SignInSignOutBtn_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Cerrar Sesión", "Esta seguro de cerrar Sesión?", "Si", "No");
            if (answer)
            {
                App.Database.Logout();
            LoginPage.SignOut();
            LoginPage login = new LoginPage();
            App.Current.MainPage = new NavigationPage(login);
            }
            

        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {

        }
    }
}
