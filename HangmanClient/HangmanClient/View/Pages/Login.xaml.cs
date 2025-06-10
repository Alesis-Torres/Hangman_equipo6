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

            try
            {
                var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                var user = hangmanService.Login(username, password);

                if (user != null)
                {
                    SessionManager.Instance.CurrentPlayer = user; 
                    SessionManager.Instance.HangmanService = hangmanService;
                    SessionManager.Instance.GameService = new GameServiceReference.GameServiceClient();

                    MessageBox.Show("¡Login exitoso!");

                    NavigationService.Navigate(new CreateMatch());
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.");
                }
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
