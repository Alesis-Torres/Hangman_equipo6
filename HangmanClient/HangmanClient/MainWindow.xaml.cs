using Hangman_Client.View.Pages;
using HangmanClient.Model.Singleton;
using HangmanClient.View.Pages;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HangmanClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Closing += MainWindow_Closing;
        PaginaActual.Navigate(new Login());
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            Console.WriteLine("[MainWindow] Cierre detectado.");

            if (PaginaActual.Content is CreateMatch createMatch)
            {
                createMatch.DetenerTimer();
            }
            var socket = SessionManager.Instance.SocketCliente;
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("[MainWindow] Socket cerrado.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en MainWindow_Closing: {ex.Message}");
        }
    }
}