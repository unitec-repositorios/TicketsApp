using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace tickets.Droid
{
    [Activity(Theme ="@style/MyTheme.Splash",
        MainLauncher =true,
        NoHistory =true)]
    class SplashActivity:Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
        protected override void OnResume()
        {
            base.OnResume();
            Task timeSplash = new Task(() => { TimeOnSplash(); });
            timeSplash.Start();
        }

        private async void TimeOnSplash()
        {
        
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
           
        }
    }
}