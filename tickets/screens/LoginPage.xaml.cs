using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http.Headers;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Graph;
using Microsoft.Identity.Client;


namespace tickets
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{

        private static GraphServiceClient graphClient = null;
        private static GraphServiceClient graphClient2 = null;
        public static string TokenForUser = null;
        public static DateTimeOffset expiration;

        public LoginPage ()
		{
			InitializeComponent();
		}

        

        async void OnSignInSignOut(object sender, EventArgs e)
        {
            
                if (SignInSignOutBtn.Text == "Iniciar Sesion")
                {
                    graphClient = GetAuthenticatedClient();
                    var currentUserObject = await graphClient.Me.Request().GetAsync();
                    App.Username = currentUserObject.DisplayName;
                    App.UserEmail = currentUserObject.UserPrincipalName;
                    Debug.WriteLine(App.Username);
                    Debug.WriteLine(App.UserEmail);

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

    }
}