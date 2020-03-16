using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tickets.API;
using tickets.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace tickets.ViewModels
{
    public class ListTicketsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        

        private bool _isEmpty = false;
        public bool IsEmpty {
            get
            {
                return _isEmpty;
            }
            set
            {
                _isEmpty = value;
                OnPropertyChanged();
            }
        }
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        private string _sortBy;

        public string SortBy
        {
            get { return _sortBy; }
            set {
                _sortBy = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_sortBy))
                {
                    /*
                     <x:String>Todos</x:String>
                                <x:String>Abiertos</x:String>
                                <x:String>Creación(Recientes)</x:String>
                                <x:String>No Leídos</x:String>
                     */
                    var _originalListCopy = ListBaseTickets;
                    if (_sortBy == "Todos")
                        ListTickets = _originalListCopy;
                    else if (_sortBy == "Abiertos")
                    {
                        ListTickets = new ObservableCollection<Ticket>(_originalListCopy.Where(x => x.IsOpen == true));
                       
                    }
                    else if (_sortBy == "Creación(Recientes)")
                    {
                        ListTickets = new ObservableCollection<Ticket>(_originalListCopy.OrderByDescending(x => x.CreationDate));
                  
                    }
                    else if (_sortBy == "No Leídos")
                    {
                        ListTickets = new ObservableCollection<Ticket>(_originalListCopy.Where(x => x.HasUpdate == true));
                      
                    }
                    IsEmpty = false;
                

                        
                }
            }
        }

        private string _searchText;

        public string SearchText
        {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value)
                {
                    _searchText = value;
                    if (!string.IsNullOrEmpty(_searchText))
                    {
                        var _originalListCopy = ListBaseTickets;
                        var findText = _searchText.ToLower();
                        ListTickets = new ObservableCollection<Ticket>(_originalListCopy.Where(x=>x.Subject.ToLower().Contains(findText)));
                        IsEmpty = false;
                    }
                    else
                    {
                        SortBy = "Todos";
                    }
                }
                OnPropertyChanged();
               
            }
        }



        private SendTicket viewSendTicket { get; set; }
        

        private bool _isBusy=false;

        public bool IsBusy
        {
            get { return _isBusy; }
            set {
                _isBusy = value;
                OnPropertyChanged();

            }
        }


        private ObservableCollection<Ticket> _listTickets;
        public ObservableCollection<Ticket> ListTickets {
            get {
                if (_listTickets == null)
                {
                    InitListTicket();
                }
                return _listTickets;
            }
            set {
                _listTickets = value;
                OnPropertyChanged();
                IsEmpty = ListTickets == null || ListTickets.Count <= 0;

            }

        }

        private async void InitListTicket()
        {
            var user = await App.Database.GetCurrentUserAsync();
            var _tempListTickets = await App.Database.GetTickets(int.Parse(user.Account));
            if (_tempListTickets == null)
            {
                _listTickets = new ObservableCollection<Ticket>();
                return;
            }
            else
            {
                _listTickets = new ObservableCollection<Ticket>(_tempListTickets.OrderByDescending(t => t.CreationDate).OrderByDescending(t => t.IsOpen));
            }
           
        }

        private Server _server= new Server();


        private ObservableCollection<Ticket> ListBaseTickets { get; set; }

        public ICommand AddTicketCommand { get; set; }
        public ICommand GoToSettingsCommand { get; set; }
        public ICommand GoToSendTicketCommand { get; set; }
        public ICommand RefreshTicketsCommand { get; set; }

      //  private SendTicket sendTicketView = new SendTicket();

        public ListTicketsViewModel()
        {
            
            GetTickets();
            checkUpdates();

            AddTicketCommand = new Command(async () => await AddTicketAsync(),()=>!IsBusy);
            GoToSettingsCommand = new Command(async () => await GoToSettings(),()=>!IsBusy);
            GoToSendTicketCommand = new Command( () =>  GoToSendTicket(), () => !IsBusy);
            RefreshTicketsCommand = new Command(async () =>await RefreshTicket());
            
        }

      
        private async void  GetTickets()
        {
            var user = await App.Database.GetCurrentUserAsync();
            var temp_list =  await App.Database.GetTickets(int.Parse(user.Account));
            ListTickets = new ObservableCollection<Ticket>(temp_list.OrderByDescending(t => t.CreationDate).OrderByDescending(t => t.IsOpen));
            ListBaseTickets = ListTickets;
            
        }

     

        private async Task AddTicketAsync()
        {
            IsBusy = true;
            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Name,
                IsCancellable = true,
                Message = "Ingrese el ID del Ticket",
                Placeholder = "Id Ticket"
            };
            var loading = new Task(() => UserDialogs.Instance.ShowLoading("Por favor espere"));
            var result = await UserDialogs.Instance.PromptAsync(promptConfig);
            if (result.Ok)
            {
                
          
                if (string.IsNullOrEmpty(result.Text))
                {
                    UserDialogs.Instance.ShowError("Ingrese un id.");
                }
                else
                {
                    loading.Start();
                   
                    Ticket db_t =await App.Database.GetTicket(result.Text);
                    Ticket t = await _server.GetTicket(result.Text);
                    if (t == null)
                    {
                        UserDialogs.Instance.ShowError("No existe un ticket con el ID: " + result.Text);
                    }
                    else if (db_t != null)
                    {
                        UserDialogs.Instance.ShowError("No se agrego el ticket, porque ya existe en la base de datos.");
                    }
                    else
                    {
                        await App.Database.AgregarTicket(t);
                        GetTickets();
                        UserDialogs.Instance.ShowSuccess("Ticket Agregado!");
                     
                        
                    }
                }
            }
            IsBusy = false;
        }

        private async Task GoToSettings()
        {
            if (!IsBusy)
            {
                IsBusy = false;
                await Application.Current.MainPage.Navigation.PushAsync(new AppSettingsPage());
            }
                
           
        }
        private void GoToSendTicket()
        {
            try
            {
                var currentConnection = Connectivity.NetworkAccess;
                if (currentConnection != NetworkAccess.Internet)
                {
                    UserDialogs.Instance.ShowError("Sin acceso a internet");
                    Task.Delay(500);
                    return;
                }

                if (!IsBusy)
                {
                    IsBusy = false;
                    UserDialogs.Instance.ShowLoading();
                    App.Current.MainPage.Navigation.PushAsync(new SendTicket());
                    UserDialogs.Instance.HideLoading();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        private async Task RefreshTicket()
        {

            IsRefreshing = true;
            checkUpdates();
            await Task.Delay(800);
            IsRefreshing = false;
        }


        private async void checkUpdates()
        {
            if (!IsBusy && ListTickets!=null)
            {
                var currentConnection = Connectivity.NetworkAccess;
                if (currentConnection != NetworkAccess.Internet)
                {
                    IsBusy = false;
                    return;
                }
                IsBusy = true;
                foreach (var item in ListTickets)
                {
                    var tempTicket =await _server.GetTicket(item.ID);
                    if (item.LastUpdate != tempTicket.LastUpdate)
                    {
                        tempTicket.HasUpdate=true;
                        await App.Database.ActualizarTicket(tempTicket);
                    }
                }
                IsBusy = false;
            }
           
        }

     /*   private void HandleSelectedItem()
        {
            if (!IsBusy)
            {
                
                IsBusy = true;
                chatTicket _chatTicket=null;
                Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowLoading("Cargando Ticket"));
                Task.Run( () => {
                    var tempList = ListTickets;
                    ListTickets = new ObservableCollection<Ticket>(tempList);
                    var idTicketSelected = SelectedTicket.ID;
                    
                    _chatTicket = new chatTicket() { BindingContext = idTicketSelected };
                }).ContinueWith(result=>Device.BeginInvokeOnMainThread(()=> {
                    UserDialogs.Instance.HideLoading();
                    
                    IsBusy = false;
                    if (_chatTicket == null)
                        return;

                    App.Current.MainPage.Navigation.PushAsync(_chatTicket);

                })
                
                );
             
            }


           
        }
       */ 

        private async Task<SendTicket> GetSendTicketAsync()
        {

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                viewSendTicket = new SendTicket();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Acceso Internet", "Lo sentimos no tenemos acceso a intenert","Ok");
                return null;
            }
             
                return viewSendTicket;

        }
       
    }    
}
