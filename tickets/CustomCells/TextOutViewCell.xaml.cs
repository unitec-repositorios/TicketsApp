using Plugin.Clipboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace tickets.CustomCells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextOutViewCell : ViewCell
    {
        public TextOutViewCell()
        {
            InitializeComponent();
        }

        public async void OnClickedMsg(object sender, ClickedEventArgs args)
        {
            CrossClipboard.Current.SetText(((Xamarin.Forms.Button)sender).Text);
        }
    }
}