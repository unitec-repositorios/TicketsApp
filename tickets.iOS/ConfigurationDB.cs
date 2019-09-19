using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using SQLite.Net.Interop;
using tickets.Data;
using UIKit;
using Xamarin.Forms;
[assembly: Dependency(typeof(tickets.iOS.ConfigurationDB))]

namespace tickets.iOS
{
    public class ConfigurationDB : IConfigurationDB
    {
        private string Directorio;
        private ISQLitePlatform Plataforma;
        public string directorio {
            get
            {
                if (string.IsNullOrEmpty(Directorio))
                {
                    var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    Directorio = Path.Combine(dir,"..","Library");
                }
                return Directorio;
            }
        }

        public ISQLitePlatform plataforma {
            get
            {
                if (Plataforma == null)
                {
                    Plataforma = new  SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
                }
                return Plataforma;
            }
        }
    }
}