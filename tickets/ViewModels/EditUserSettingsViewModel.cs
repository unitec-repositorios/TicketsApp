using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using tickets.API;
using tickets.Views;
using Xamarin.Forms;

namespace tickets.ViewModels
{
    public class EditUserSettingsViewModel:User
    {
        private User _modelo; 
      //  private readonly Database _database = new Database();
        Configuraciones configuracion = new Configuraciones();
        
        
        public EditUserSettingsViewModel(User _usuario=null)
        {
            if (_usuario != null)
            {
                _modelo = _usuario;
            }
            else
            {
                _modelo = App.Database.GetCurrentUser();
            }
          
            CargarDatos();
            CampusList = new ObservableCollection<string>(configuracion.GetCampus());
            PerfilList = new ObservableCollection<string>(configuracion.GetPerfil());
            CarreraList = new ObservableCollection<string>(configuracion.GetCarreras());
            ActualizarCommand   = new Command(async () => await Actualizar(),   ()=>!IsBusy);
            CancelarCommand     = new Command(async () => await Cancelar(),     ()=>!IsBusy);
        }

        public Command ActualizarCommand { get; set; }
        public Command CancelarCommand { get; set; }
        public ObservableCollection<string> CampusList { get; set; }
        public ObservableCollection<string> PerfilList { get; set; }
        public ObservableCollection<string> CarreraList { get; set; }

        public Database Database => App.Database;

        private async Task Actualizar(){
            IsBusy = true;
            if (IsValid){
                User _currentUser = await Database.GetCurrentUserAsync();
                bool isNewUser = _currentUser == null;
                _modelo = new User()
                {
                    ID = ID,
                    Name = Name,
                    Email = Email,
                    Campus = Campus,
                    Career = Career,
                    Profile = Profile,
                    Account = Account,
                    PhoneNumber = PhoneNumber,
                    PersonalEMail=PersonalEMail,
                    IsCurrent = true
                };
                UserDialogs.Instance.ShowLoading("Por favor espere");
                if (!isNewUser) {
                    _modelo.ID = _currentUser.ID;
                    await Database.ActualizarUsuario(_modelo);
                }
                else
                {
                    _modelo.ID = 0;
                    await Database.CreateNewCurrentUser(_modelo);
                }
                await Task.Delay(2000);
                UserDialogs.Instance.HideLoading();
                IsBusy = false;
                if (!isNewUser)
                {
                    await Application.Current.MainPage.DisplayAlert("Actualizacion de Usuario", "El usuario a sido actualizado exitosamente!", "Aceptar");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    
                    await Application.Current.MainPage.DisplayAlert("Creacion de Usuario", "Usuario creado exitosamente", "Aceptar");
                    Application.Current.MainPage =new NavigationPage (new ListTicketsView());
                   
                }

              
            }
            else{
                await Application.Current.MainPage.DisplayAlert("Campos Incompletos","Rellene todos los campos","Aceptar");
            }
            IsBusy = false;
        }

        private async Task Cancelar()
        {
            IsBusy = false;
            await Application.Current.MainPage.Navigation.PopAsync();
            
        }
        private  void CargarDatos()
        {
            Name = _modelo.Name;
            Email = _modelo.Email;
            Campus = _modelo.Campus;
            Career = _modelo.Career;
            Profile = _modelo.Profile;
            Account = _modelo.Account;
            PhoneNumber = _modelo.PhoneNumber;
            PersonalEMail = _modelo.PersonalEMail;

        }

    }
}
