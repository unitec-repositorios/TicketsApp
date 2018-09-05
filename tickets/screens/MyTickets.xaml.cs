using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        public MyTickets()
        {
            InitializeComponent();

            var newTicket = new ToolbarItem
            {
                Text = "Nuevo",
                Command = new Command(async (s) => await Navigation.PushAsync(new SendTicket())),
                Order = ToolbarItemOrder.Primary
            };

            var settings = new ToolbarItem
            {
                Text = "Ajustes",
                Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPage())),
                Order = ToolbarItemOrder.Secondary
            };

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    ToolbarItems.Add(newTicket);
                    break;
                case Device.Android:
                    ToolbarItems.Add(newTicket);
                    ToolbarItems.Add(settings);
                    break;
            }
        }

        protected override async void OnAppearing()
        {
            TicketsListView.ItemsSource = await App.Database.GetTicketsAsync();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                await Navigation.PushAsync(new chatTicket());
            }
        }
    }
}
