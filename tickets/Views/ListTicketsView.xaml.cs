using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tickets.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace tickets.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListTicketsView : ContentPage
    {

       
        
        public ListTicketsView()
        {
            InitializeComponent();
          //  BindingContext=this;
            
            BindingContext =new ListTicketsViewModel();
            Picker_Filter.SelectedIndex = 0;
            
        }

        private async void OnSelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            Ticket item = (Ticket)e.SelectedItem;
            if (item !=null)
            {
                ListView_ListTicket.SelectedItem = null;
                chatTicket _chatTicket=null;
                Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowLoading());
                await Task.Run(async () =>
                {
                    if (item.HasUpdate)
                    {
                        item.HasUpdate = false;
                        await App.Database.UpdateTicket(item);
                    }
                    _chatTicket = new chatTicket() { BindingContext = item.ID };

                }).ContinueWith(result => Device.BeginInvokeOnMainThread(() =>
                {

                    UserDialogs.Instance.HideLoading();
                    Navigation.PushAsync(_chatTicket);
                })
                );
               
              
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBar_Search != null) {
                if (SearchBar_Search.Text.Length > 0)
                {
                    StackLayout_Filter.IsVisible = false;
                }
                else
                {
                    StackLayout_Filter.IsVisible = true;
                }
            }
        }
    }

    public class BooleanInvert : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }

}