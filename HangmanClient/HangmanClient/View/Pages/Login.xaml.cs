using HangmanClient.Model.Singleton;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HangmanClient.View.Pages
{
    
    public partial class Login : Page
    {
        private int idioma = SessionManager.Instance.CurrentLanguage;
        public Login()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor ingrese el nombre de usuario y la contraseña.");
                return;
            }

            try
            {
                var hangmanService = SessionManager.Instance.HangmanServiceClient;
                var loginResponse = hangmanService.Login(username, password);

                if (loginResponse == null)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.");
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

                byte[] respuestaBuffer = new byte[1024];
                int bytesLeidos = SessionManager.Instance.SocketCliente.Receive(respuestaBuffer);
                string respuesta = System.Text.Encoding.UTF8.GetString(respuestaBuffer, 0, bytesLeidos).Trim();

                if (respuesta == "DUPLICADO")
                {
                    MessageBox.Show("Este usuario ya está conectado en el servidor. Intente cerrar la sesión anterior.");
                    return;
                }

                MessageBox.Show("¡Bienvenido " + loginResponse.Nickname + "!");
                NavigationService.Navigate(new CreateMatch(esLogin: true, ""));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProfileForm());
        }
    }
}