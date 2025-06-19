using HangmanClient.Model.Singleton;
using HangmanClient.Util;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HangmanClient.View.Pages
{
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
                var hangmanService = SessionManager.Instance.HangmanServiceClient;
                var loginResponse = hangmanService.Login(username, password);

                if (loginResponse == null)
                {
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
                    MessageBox.Show("Este usuario ya tiene una sesión activa en otro dispositivo. Cierre la otra sesión e intente nuevamente.");
                    return;
                }

                SessionManager.Instance.CurrentPlayer = loginResponse;
                ((App)Application.Current).IniciarSocketMonitor(loginResponse.Username, loginResponse.IdPlayer);
                string loginMsg = $"LOGIN|{loginResponse.IdPlayer}|{loginResponse.Nickname}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(loginMsg);
                SessionManager.Instance.SocketCliente.Send(buffer);

                notification.NotificationTitle = Literals.SuccesfulLogin;
                notification.NotificationMessage = Literals.Welcome;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Confirmation;
                var successWindow = new NotificationWindow(notification);
                successWindow.ShowDialog();
                NavigationService.Navigate(new CreateMatch(esLogin: true, ""));
            }
            catch (Exception ex)
            {
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