using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmHelpers;


namespace tickets.Models
{
    public class chatViewModel : BaseViewModel
    {
        public ObservableRangeCollection<Message> ListMessages { get; }
        public ICommand SendCommand { get; set; }


        public chatViewModel()
        {

            ListMessages = new ObservableRangeCollection<Message>();

            SendCommand = new Command(() =>
            {
                if (!String.IsNullOrWhiteSpace(OutText))
                {
                    var message = new Message
                    {
                        Text = OutText,
                        IsTextIn = false,
                        MessageDateTime = DateTime.Now
                    };

                    ListMessages.Add(message);
                    OutText = "";
                }

            });
            
        }


        public string OutText
        {
            get { return _outText; }
            set { SetProperty(ref _outText, value); }
        }
        string _outText = string.Empty;
    }
}
