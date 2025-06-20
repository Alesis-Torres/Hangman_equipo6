using HangmanClient.Model.Singleton;
using HangmanClient.Util;
using HangmanServiceReference;
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
    /// Interaction logic for GameStadistics.xaml
    /// </summary>
    public partial class GameStatistics : Page
    {
        private readonly HangmanServiceClient hangmanService;
        public GameStatistics()
        {
            InitializeComponent();

            hangmanService = new HangmanServiceClient();
            SetGamesPlayed();
        }
        private void SetGamesPlayed()
        {
            var notification = new NotificationContent();
            try
            {
                int playerId = SessionManager.Instance.CurrentPlayer.IdPlayer;
                var historial = hangmanService.ObtenerHistorialPartidas(playerId, SessionManager.Instance.CurrentLanguage);
                EstadisticasListBox.ItemsSource = historial;
            } catch (Exception ex)
            {
                notification.NotificationTitle = Literals.Offline;
                notification.NotificationMessage = Literals.ConnectionErrorDescription;
                notification.AcceptButtonText = Literals.Accept;
                notification.Type = NotificationType.Error;

                var window = new NotificationWindow(notification);
                window.ShowDialog();
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
