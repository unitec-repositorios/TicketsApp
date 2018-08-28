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
    }
}
