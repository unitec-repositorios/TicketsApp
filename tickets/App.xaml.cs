using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Identity.Client;
using System.Diagnostics;
using tickets.Views;
using tickets.ViewModels;
using tickets.API;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace tickets
{
    public partial class App : Application
    {
        public static PublicClientApplication IdentityClientApp;
        public static string ClientID = "d0297af7-d0ed-4331-8d16-eddb448252d9";
        public static string RedirectUri = "msal" + ClientID + "://auth";
        public static string[] Scopes = { "User.Read" };
        public static string Username = string.Empty;
        public static string UserEmail = string.Empty;

        public static bool IsUserLoggedIn = false;
        public static UIParent UiParent;
        static Database database;

        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database();
                }
                return database;
            }
        }
        public App()
        {
            InitializeComponent();
            IdentityClientApp = new PublicClientApplication(ClientID)
            {
                RedirectUri = RedirectUri
            };

            bool isTestView = false;
            if (isTestView)
            {
                MainPage = new NavigationPage(new ListTicketsView());

                //Database.ClearDatabase();

                // Debug.WriteLineIf(Database.GetCurrentUser() == null, "Current user is null, should go to login page");
            }
            else
            {
                if (Database.GetCurrentUser() == null)
                {
                    MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    switch (Device.RuntimePlatform)
                    {
                        case Device.iOS:
                            MainPage = new NavigationPage(new HomeScreen());
                            break;
                        case Device.Android:
                            //    MainPage = new NavigationPage(new MyTickets());
                            MainPage = new NavigationPage(new ListTicketsView());
                            break;
                        case Device.UWP:
                            MainPage = new NavigationPage(new HomeScreen());
                            break;
                    }
                }
            }
        }
      
    }
}
