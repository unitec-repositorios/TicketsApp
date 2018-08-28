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
            Title = "Ajustes de Cuenta";
            txtname.Completed += (s, e) => txtemail.Focus();
            txtemail.Completed += (s, e) => campuspicker.Focus();
            profilepicker.SelectedIndexChanged += (s, e) => txtaccount.Focus();
            //txtemail.Completed += (s, e) => campuspicker.Focus();
            //txtemail.Completed += (s, e) => campuspicker.Focus();
            //txtemail.Completed += (s, e) => campuspicker.Focus();
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();
            User current = await App.Database.GetCurrentUser();
            if (current != null && BindingContext == null)
            {
                Console.WriteLine("OnAppearing: Current user exists");
                BindingContext = current;
            }
        }

        async void OnSaveTouched(object sender, System.EventArgs e)
        {
            var user = (User)BindingContext;
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
            user.PrintData();
            await Navigation.PopAsync();
        }

        void OnClearTouched(object sender, System.EventArgs e)
        {
            BindingContext = new User();
        }

        async void OnCancelTouched(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
