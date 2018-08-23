using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Identity.Client;

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
        public static UIParent UiParent;
        static Database database;

        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database(
                      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketsApp.db3"));
                }
                return database;
            }
        }
        public App()
        {
            InitializeComponent();
            IdentityClientApp = new PublicClientApplication(ClientID);
            IdentityClientApp.RedirectUri = RedirectUri;
            if (App.Database.GetCurrentUser() == null)
            {
                MainPage = new NavigationPage(new LoginPage());
            } else {
                MainPage = new NavigationPage(new SendTicket());
            }
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
