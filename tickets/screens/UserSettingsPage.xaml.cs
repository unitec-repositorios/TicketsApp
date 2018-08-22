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
            if (current == null)
            {
                Console.WriteLine("OnAppearing: Current user is null");
                BindingContext = new User();
            }
            else
            {
                Console.WriteLine("OnAppearing: Current user exists");
                BindingContext = current;
            }
        }

        async void OnSaveTouched(object sender, System.EventArgs e)
        {
            var user = (User)BindingContext;
            user.PrintData();
            User current = await App.Database.GetCurrentUser();
            if (current == null)
            {
                await App.Database.CreateNewCurrentUser(user);
                await DisplayAlert("Sucess", "Creado correctamente", "Ok");
            }
            else
            {
                user.ID = current.ID;
                user.IsCurrent = true;
                await App.Database.SaveUserAsync(user);
                await DisplayAlert("Sucess", "Actualizado correctamente", "Ok");
            }
            await Navigation.PopAsync();
        }

        void OnClearTouched(object sender, System.EventArgs e)
        {
            BindingContext = new User();
        }

        void OnCancelTouched(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
