using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace tickets.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatTicketView : ContentPage
    {
        public ChatTicketView()
        {
            InitializeComponent();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (EditorText.Text.Length == 10)
            {
                MessageContainer.HeightRequest += 20;
            }else if (EditorText.Text.Length == 20)
            {
                MessageContainer.HeightRequest += 20;
            }else if (EditorText.Text.Length < 10)
            {
                MessageContainer.HeightRequest = 32;
            }
        }
    }
}