using HangmanClient.Model.Singleton;
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

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor ingrese el nombre de usuario y la contraseña.");
                return;
            }

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
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

                // Almacena el usuario en SessionManager
                SessionManager.Instance.CurrentPlayer = loginResponse;
                MessageBox.Show("¡Bienvenido " + loginResponse.Nickname + "!");

                // Navega a la pantalla de selección de sala
                NavigationService.Navigate(new CreateMatch());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProfileForm());
        }
    }
}
