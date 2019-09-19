using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tickets.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace tickets.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditUserSettingView : ContentPage
    {
        public EditUserSettingView(User _user=null)
        {
            InitializeComponent();
            BindingContext = new EditUserSettingsViewModel(_user);
        }
    }
}