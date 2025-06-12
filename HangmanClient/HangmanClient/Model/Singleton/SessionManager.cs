using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HangmanClient.Model.Singleton
{
    public class SessionManager
    {
        private static SessionManager _instance;
        public static SessionManager Instance => _instance ?? (_instance = new SessionManager());

        public GameServiceReference.GameServiceClient GameService { get; set; }
        public HangmanServiceReference.HangmanServiceClient HangmanService { get; set; }

        public HangmanServiceReference.PlayerDTO CurrentPlayer { get; set; }
        public int CurrentLanguage { get; set; } = 1;

        private SessionManager() { }

        public void Logout()
        {
            if (CurrentPlayer != null)
            {
                try
                {
                    var hangmanService = new HangmanServiceReference.HangmanServiceClient();
                    hangmanService.Logout(CurrentPlayer.Username);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cerrar sesión: {ex.Message}");
                }
                CurrentPlayer = null;
            }
        }

        public void HandleExpulsion()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Tu sesión ha sido cerrada por otro inicio de sesión.", "Sesión Cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                Logout();
                Application.Current.Shutdown();
            });
        }
    }
}