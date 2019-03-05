using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;
using tickets.screens;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net;
using Xamarin.Essentials;

namespace tickets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        private static GraphServiceClient graphClient = null;
        private static GraphServiceClient graphClient2 = null;
        public static string TokenForUser = null;
        public static DateTimeOffset expiration;

        public static String username;
        public static String email;
        public LoginPage()
        {
            InitializeComponent();
        }



        async void OnSignInSignOut(object sender, EventArgs e)
        {
            if (CheckInternetConnection())
            {
                SignInSignOutBtn.IsVisible = false;
                SignInSignOutBtnAdmin.IsVisible = false;
                Loading.IsVisible = true;
                try
                {
                    Microsoft.Graph.User currentUserObject;
                    graphClient = GetAuthenticatedClient();
                    currentUserObject = await graphClient.Me.Request().GetAsync();




                    if (currentUserObject.UserPrincipalName.ToLower().Contains("@unitec.edu"))
                    {

                        App.Username = currentUserObject.DisplayName;
                        App.UserEmail = currentUserObject.UserPrincipalName;
                        username = App.Username;
                        email = App.UserEmail;
                        Debug.WriteLine(App.Username);
                        Debug.WriteLine(App.UserEmail);

                        // AQUI LOGIN

                        var userSettings = new UserSettingsPage()
                        {
                            BindingContext = new User()
                            {
                                Name = username,
                                Email = email
                            }
                        };
                        var user = App.Database.GetUserAsync(email);
                        if (user == null)
                        {
                            await Navigation.PushAsync(userSettings);
                        }
                        else
                        {
                            user.PrintData();
                            Debug.WriteLine("Before createNewCurrentUser");
                            await App.Database.CreateNewCurrentUser(user);
                            Debug.WriteLine("After createNewCurrentUser");
                            //HomeScreen home = new HomeScreen();
                            //App.Current.MainPage = new NavigationPage(home);
                            switch (Xamarin.Forms.Device.RuntimePlatform)
                            {
                                case Xamarin.Forms.Device.iOS:
                                    Debug.WriteLine("Device is IOS");
                                    var newHome = new HomeScreen();
                                    await Navigation.PushAsync(newHome);
                                    App.Current.MainPage = new NavigationPage(newHome);
                                    break;
                                case Xamarin.Forms.Device.Android:
                                    Debug.WriteLine("Device is ANDROID");
                                    var newHome2 = new MyTickets();
                                    await Navigation.PushAsync(newHome2);
                                    App.Current.MainPage = new NavigationPage(newHome2);
                                    break;
                                case Xamarin.Forms.Device.UWP:
                                    Debug.WriteLine("Device is UWP");
                                    var newHome3 = new HomeScreen();
                                    await Navigation.PushAsync(newHome3);
                                    App.Current.MainPage = new NavigationPage(newHome3);
                                    break;
                            }
                        }
                        userSettings.Disappearing += async (sender2, e2) =>
                        {
                            HomeScreen home = new HomeScreen();
                            App.Current.MainPage = new NavigationPage(home);
                            switch (Xamarin.Forms.Device.RuntimePlatform)
                            {
                                case Xamarin.Forms.Device.iOS:
                                    App.Current.MainPage = new NavigationPage(new HomeScreen());
                                    break;
                                case Xamarin.Forms.Device.Android:
                                    App.Current.MainPage = new NavigationPage(new MyTickets());
                                    break;

                            }
                            //Navigation.RemovePage(page);
                        };
                    }
                    else
                    {
                        Debug.WriteLine("Llegue aca!");
                        await DisplayAlert("Error", "El correo utilizado no es valido. Por favor, utilice el correo de la Universidad", "Ok");
                        username = null;
                        email = null;
                        TokenForUser = null;
                        graphClient = null;
                        SignInSignOutBtn.IsVisible = true;
                        Loading.IsVisible = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //await DisplayAlert("Cancelled", "User cancelled authentication", "Ok");
                    SignInSignOutBtn.IsVisible = true;
                    Loading.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("No hay conexión", "No se detecto una conexión a Internet. Por favor vuelta a intentarlo", "Ok");
                SignInSignOutBtn.IsVisible = true;
                Loading.IsVisible = false;
            }

        }
        async void OnSignInSignOutAdmin(object sender, EventArgs e)
        {
            if (CheckInternetConnection())
            {
                SignInSignOutBtnAdmin.IsVisible = false;
                SignInSignOutBtn.IsVisible = false;
                Loading.IsVisible = true;

                /* Aqui va la conexion al ADMIN del hesk */

                /*  HomeScreen home = new HomeScreen();
                  App.Current.MainPage = new NavigationPage(home);*/


                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                        App.Current.MainPage = new NavigationPage(new HomeScreen());
                        break;
                    case Xamarin.Forms.Device.Android:
                        App.Current.MainPage = new NavigationPage(new LoginPageAdmin());
                        break;

                }
            }
            else
            {
                await DisplayAlert("No hay conexión", "No se detecto una conexión a Internet. Por favor vuelta a intentarlo", "Ok");
                SignInSignOutBtn.IsVisible = true;
                Loading.IsVisible = false;
            }

        }

        public bool CheckInternetConnection()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static GraphServiceClient GetAuthenticatedClient()
        {
            if (graphClient2 == null)
            {
                // Create Microsoft Graph client.
                try
                {
                    graphClient2 = new GraphServiceClient(
                        "https://graph.microsoft.com/v1.0",
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var token = await GetTokenForUserAsync();
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);


                            }));
                    return graphClient2;
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("Could not create a graph client: " + ex.Message);
                }
            }

            return graphClient2;
        }

        public static async Task<string> GetTokenForUserAsync()
        {

            if (TokenForUser == null || expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                AuthenticationResult authResult = await App.IdentityClientApp.AcquireTokenAsync(App.Scopes, App.UiParent);

                if (authResult == null)
                {
                    Debug.WriteLine("Fail!");
                }
                else
                {
                    TokenForUser = authResult.AccessToken;
                    expiration = authResult.ExpiresOn;
                }

            }

            return TokenForUser;
        }

        public static void SignOut()
        {
            foreach (var user in App.IdentityClientApp.Users)
            {
                App.IdentityClientApp.Remove(user);
            }
            graphClient = null;
            TokenForUser = null;
            username = null;
            email = null;

        }

    }
}