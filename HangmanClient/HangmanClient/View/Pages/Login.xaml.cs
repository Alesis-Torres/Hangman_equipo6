using HangmanClient.Model.Singleton;
using HangmanClient.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HangmanClient.View.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            var notification = new NotificationContent();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                // Mostrar notificacion de campos vacios
                notification.NotificationTitle = Literals.EmptyFields;
                notification.NotificationMessage = Literals.EmptyFieldsDescription;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Error;
                var incorrectCredentialsWindow = new NotificationWindow(notification);
                incorrectCredentialsWindow.ShowDialog();
                
                return;
            }

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                var loginResponse = hangmanService.Login(username, password);


                if (loginResponse == null)
                {

                    // Mostrar notificacion de credenciales
                    notification.NotificationTitle = Literals.IncorrectCredentials;
                    notification.NotificationMessage = Literals.IncorrectCredentialsDescription;
                    notification.AcceptButtonText = Literals.Accept;
                    notification.Type = NotificationType.Error;
                    var incorrectCredentialsWindow = new NotificationWindow(notification);
                    incorrectCredentialsWindow.ShowDialog();

                    return;
                }

                if (loginResponse.SessionDuplicate)
                {
                    // Mostar noti perso con nuevos internacion
                    MessageBox.Show("Este usuario ya tiene una sesión activa en otro dispositivo. Cierre la otra sesión e intente nuevamente.");
                    return;
                }

                // Almacena el usuario en SessionManager
                SessionManager.Instance.CurrentPlayer = loginResponse;

                // Mostrar notificacion de exito
                notification.NotificationTitle = Literals.SuccesfulLogin;
                notification.NotificationMessage = Literals.Welcome;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Confirmation;
                var successWindow = new NotificationWindow(notification);
                successWindow.ShowDialog();

                // Navega a la pantalla de selección de sala
                NavigationService.Navigate(new CreateMatch());
            }
            catch (Exception ex)
            {
                // Mostrar notificacion de error de conexion
                notification.NotificationTitle = Literals.Offline;
                notification.NotificationMessage = Literals.ConnectionErrorDescription;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Error;
                var erroWindow = new NotificationWindow(notification);
                erroWindow.ShowDialog();
            }
        }
        
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProfileForm());
        }

        // Acciones para cambiar idioma
        private void SpanishButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-MX");
            NavigationService.Navigate(new Login());
        }

        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            NavigationService.Navigate(new Login());
        }
    }
}