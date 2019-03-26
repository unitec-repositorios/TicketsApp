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

            pictureQualitySetting.Value = AppSettings.ImagesQuality;
            ticketsTimeoutSetting.Text = AppSettings.RefreshTicketsTimeout.ToString();

            pictureQualitySetting.ValueChanged += Slider_ValueChanged;
            ticketsTimeoutSetting.Completed += Timeout_ValueChanged;
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            AppSettings.ImagesQuality = Convert.ToInt32(pictureQualitySetting.Value);
        }

        private void Timeout_ValueChanged(object sender, EventArgs e)
        {
            AppSettings.RefreshTicketsTimeout = Convert.ToInt32(ticketsTimeoutSetting.Text);
        }

        public async void goToUserSettings(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new UserSettingsPage());
        }

        private async void SignInSignOutBtn_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Cerrar Sesión", "Esta seguro de cerrar sesión?", "Si", "No");
            if (answer)
            {
                App.Database.Logout();
                LoginPage.SignOut();
                LoginPage login = new LoginPage();
                App.Current.MainPage = new NavigationPage(login);
            }


        }

        private async void SignInAdminClicked(object sender, EventArgs e)
        {
            /* LoginAdminPage login = new LoginAdminPage();
             App.Current.MainPage = new NavigationPage(login);*/
            await Navigation.PushAsync(new LoginAdminPage());
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {

        }
    }
}