using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace tickets
{
    public partial class UserSettingsPage : ContentPage
    {
        public UserSettingsPage()
        {
            InitializeComponent();
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();
            User current = await App.Database.GetCurrentUser();
            if (current == null) {
                BindingContext = new User();
            } else {
                BindingContext = current;
            }
        }

        void OnSaveTouched(object sender, System.EventArgs e)
        {
            var user = (User)BindingContext;
            user.PrintData();
            Console.WriteLine();
            App.Database.SaveUserAsync(user);
        }

        void OnClearTouched(object sender, System.EventArgs e)
        {
            BindingContext = new User();
        }
    }
}
