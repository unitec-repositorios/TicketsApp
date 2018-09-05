using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty2",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty3",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
        }

        protected override async void OnAppearing()
        {
            TicketsListView.ItemsSource = await App.Database.GetTicketsAsync();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                Debug.WriteLine("Opening messages for ticket with id = " + ((Ticket)e.SelectedItem).ID);
                await Navigation.PushAsync(new chatTicket()
                {
                    BindingContext = ((Ticket)e.SelectedItem).ID
                });
            }
        }
    }
}
