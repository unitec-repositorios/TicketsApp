using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace tickets
{
    public partial class UserSettingsPage : ContentPage
    {
        public UserSettingsPage()
        {
            try
            {
                InitializeComponent();
            }catch(Exception e)
            {
                Console.WriteLine("Message: " + e.Message + "\nStackTrace:" + e.StackTrace);
            }
            txtname.Completed += (s, e) => txtemail.Focus();
            txtemail.Completed += (s, e) => campuspicker.Focus();
            profilepicker.SelectedIndexChanged += (s, e) => txtaccount.Focus();
            campuspicker.SelectedIndex = 1;
            //txtemail.Completed += (s, e) => campuspicker.Focus();
            //txtemail.Completed += (s, e) => campuspicker.Focus();
            //txtemail.Completed += (s, e) => campuspicker.Focus();
           
        }

        async protected override void OnAppearing()
        {
            base.OnAppearing();
            User current = await App.Database.GetCurrentUserAsync();
            if (current != null && BindingContext == null)
            {
                Console.WriteLine("OnAppearing: Current user exists");
                BindingContext = current;
            }
        }

        async void OnSaveTouched(object sender, System.EventArgs e)
        {
            var user = (User)BindingContext;
            User current = await App.Database.GetCurrentUserAsync();
            if (user.IsValid())
            {
                if (current == null)
                {
                    user.ID = 0;
                    await App.Database.CreateNewCurrentUser(user);
                    await DisplayAlert("Enhorabuena", "Su usuario ha sido creado exitosamente!", "Aceptar");
                }
                else
                {
                    user.ID = current.ID;
                    user.IsCurrent = true;
                    App.Database.ActualizarUsuario(user);
                    await DisplayAlert("Enhorabuena", "Su usuario ha sido actualizado exitosamente!", "Aceptar");
                }
                user.PrintData();
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Debe rellenar todos los campos", "Aceptar");
            }

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
