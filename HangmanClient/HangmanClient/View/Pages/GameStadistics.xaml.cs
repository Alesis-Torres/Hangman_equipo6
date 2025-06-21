using HangmanClient.Model.Singleton;
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
    public partial class GameStadistics : Page
    {
        private readonly HangmanServiceClient hangmanService;

        public GameStadistics()
        {
            InitializeComponent();

            hangmanService = new HangmanServiceClient();
            CargarEstadisticas();
        }

        private void CargarEstadisticas()
        {
            try
            {
                int playerId = SessionManager.Instance.CurrentPlayer.IdPlayer;
                var historial = hangmanService.ObtenerHistorialPartidas(playerId, SessionManager.Instance.CurrentLanguage);
                //EstadisticasListBox.ItemsSource = historial;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}");
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateMatch(esLogin: false, ""));
        }
    }
}