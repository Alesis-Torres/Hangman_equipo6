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
    /// Interaction logic for WaitingGame.xaml  
    /// </summary>  
    public partial class WaitingGame : Page
    {
        private readonly string _gameId;
        public WaitingGame(string gameId)
        {
            InitializeComponent();
            _gameId = gameId;
            MatchCode.Content = _gameId;
        }


        // Llamar función cuando se une un jugador
        private void ShowPlayerJoinedMessage(string playerName)
        {
            PlayerJoinedMessage.Visibility = Visibility.Visible;
            NewPlayerName.Content = playerName;
        }

        private void CancelMatch_Click(object sender, RoutedEventArgs e)
        {
            // Manejar lógica de cancelar partida
        }
    }
}
