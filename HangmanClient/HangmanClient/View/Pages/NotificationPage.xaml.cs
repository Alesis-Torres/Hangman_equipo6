using HangmanClient.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hangman_Client.View.Pages
{
    /// <summary>
    /// Interaction logic for NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage : Page
    {
        private readonly NotificationContent _notificationContent;

        public NotificationPage(NotificationContent notificationContent)
        {
            InitializeComponent();
            _notificationContent = notificationContent;
            SetBackground();
            SetText();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        private void SetText()
        {
            NotificationTitle.Content = _notificationContent.NotificationTitle;
            NotificationMessage.Text = _notificationContent.NotificationMessage;
            AcceptButtonText.Content = _notificationContent.AcceptButtonText;
        }

        private void SetBackground()
        {
            string backgroundResourceKey = _notificationContent.Type switch
            {
                NotificationType.Error => "notification_background",
                NotificationType.Confirmation => "confirmation_background",
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

            string iconResourceKey = _notificationContent.Type switch
            {
                NotificationType.Error => "notification_icon",
                NotificationType.Confirmation => "confirmation_icon",
                _ => "notification_icon"
            };
            try
            {
                Icon.Background = (Brush)FindResource(iconResourceKey);
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                MainGrid.Background = Brushes.White;
            }
        }
    }
}
