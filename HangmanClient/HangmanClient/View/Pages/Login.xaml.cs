using HangmanClient.Model.Singleton;
using HangmanClient.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            // Establecer el idioma predeterminado al cargar la página
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            var notification = new NotificationContent();

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                var user = hangmanService.Login(username, password);

                if (user != null)
                {
                    SessionManager.Instance.CurrentPlayer = user; 
                    SessionManager.Instance.HangmanService = hangmanService;
                    SessionManager.Instance.GameService = new GameServiceReference.GameServiceClient();

                    
                    notification.NotificationTitle = Literals.SuccesfulLogin;
                    notification.NotificationMessage = Literals.Welcome;
                    notification.AcceptButtonText = Literals.Accept;
                    notification.Type = NotificationType.Confirmation;

                    var window = new NotificationWindow(notification);
                    window.ShowDialog();

                    NavigationService.Navigate(new CreateMatch());
                }
                else
                {
                    notification.NotificationTitle = Literals.IncorrectCredentials;
                    notification.NotificationMessage = Literals.IncorrectCredentials;
                    notification.AcceptButtonText = Literals.Accept;
                    notification.Type = NotificationType.Error;

                    var window = new NotificationWindow(notification);
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                notification.NotificationTitle = Literals.Offline;
                notification.NotificationMessage = Literals.ConnectionErrorDescription;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Error;

                var window = new NotificationWindow(notification);
                window.ShowDialog();
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProfileForm());
        }

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
