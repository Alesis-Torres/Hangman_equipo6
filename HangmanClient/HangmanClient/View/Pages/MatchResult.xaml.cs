using HangmanClient.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HangmanClient.View.Pages
{
    /// <summary>
    /// Interaction logic for MatchResult.xaml
    /// </summary>
    public partial class MatchResult : Page
    {
        private readonly NotificationContent _notificationContent;
        public MatchResult(NotificationContent notificationContent)
        {
            InitializeComponent();
            _notificationContent = notificationContent;
            SetBackground();
            SetText();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar la notificacion y manejar que se hace despues de cerrar
            string gameResult = _notificationContent.NotificationType.ToString();

            if (gameResult.Equals("Win", StringComparison.OrdinalIgnoreCase))
            {
                // Logica por ganar el juego sumar 100 puntos, etc

            }
            else if (gameResult.Equals("Lose", StringComparison.OrdinalIgnoreCase))
            {
                // Logica por perder el juego sumar menos puntos, etc

            }
        }

        private void SetText()
        {
            NotificationTitle.Content = _notificationContent.NotificationTitle;
            NotificationMessage.Text = _notificationContent.NotificationMessage;
            AcceptButtonText.Content = _notificationContent.AcceptButtonText;
        }

        private void SetBackground()
        {
            string backgroundResourceKey = _notificationContent.NotificationType switch
            {
                NotificationType.Win => "win_background",
                NotificationType.Lose => "lose_background",
                _ => "default_background"
            };
            try
            {
                MainGrid.Background = (Brush)FindResource(backgroundResourceKey);
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                MainGrid.Background = Brushes.White;
            }
        }
    }
}
