using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GameServiceReference;
using HangmanClient.Model.Singleton;
using Timer = System.Timers.Timer;

namespace HangmanClient.View.Pages
{
    public partial class CreateMatch : Page
    {
        private readonly Timer actualizacionSalasTimer;
        private readonly GameServiceClient gameService;
        private Socket socketCliente;

        public CreateMatch()
        {
            InitializeComponent();

            gameService = new GameServiceClient();

            actualizacionSalasTimer = new Timer(4000);
            actualizacionSalasTimer.Elapsed += (s, e) => Dispatcher.Invoke(ActualizarSalas);
            actualizacionSalasTimer.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConectarSocket();
            ActualizarSalas();
        }

        private void ConectarSocket()
        {
            try
            {
                socketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketCliente.Connect("127.0.0.1", 64520);

                string mensajeLogin = $"LOGIN|{SessionManager.Instance.CurrentPlayer.Username}";
                socketCliente.Send(Encoding.UTF8.GetBytes(mensajeLogin));
                string mensajeNickname = $"NICKNAME|{SessionManager.Instance.CurrentPlayer.Username}|{SessionManager.Instance.CurrentPlayer.Nickname}";
                socketCliente.Send(Encoding.UTF8.GetBytes(mensajeNickname));
                Task.Run(() => EscucharServidor());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servidor de juego: {ex.Message}");
            }
        }

        private void EscucharServidor()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (socketCliente.Connected)
                {
                    int bytesRecibidos = socketCliente.Receive(buffer);
                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesRecibidos).Trim();

                    if (mensaje.StartsWith("EXPULSION|"))
                    {
                        SessionManager.Instance.HandleExpulsion();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EscucharServidor: {ex.Message}");
            }
        }

        private void ActualizarSalas()
        {
            try
            {
                string salasData = gameService.ObtenerSalas();
                var salas = salasData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                SalasListBox.Items.Clear();
                foreach (var sala in salas)
                {
                    SalasListBox.Items.Add(sala.Trim());
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Error al actualizar salas: " + ex.Message));
            }
        }

        private void UnirseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalasListBox.SelectedItem is string salaSeleccionada)
            {
                try
                {
                    string[] partes = salaSeleccionada.Split(':');
                    string salaIdStr = partes[0].Replace("Sala", "").Trim();

                    string nombreJugador = SessionManager.Instance.CurrentPlayer.Nickname;
                    int idPlayerGuesser = SessionManager.Instance.CurrentPlayer.IdPlayer;
                    string respuesta = gameService.UnirseSala(int.Parse(salaIdStr), nombreJugador, idPlayerGuesser);

                    string rol = "guesser";
                    if (respuesta.StartsWith("ROLE:"))
                    {
                        rol = respuesta.Replace("ROLE:", "").Trim().ToLower();
                    }

                    NavigationService.Navigate(new MatchScreen(salaIdStr, rol, nombreJugador));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al unirse a la sala: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Selecciona una sala para unirte.");
            }
        }

        private void CrearSalaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombreJugador = SessionManager.Instance.CurrentPlayer.Nickname;
                string respuesta = gameService.CrearSala(nombreJugador, SessionManager.Instance.CurrentPlayer.IdPlayer);

                string salaId = "";
                string rol = "challenger";

                if (!string.IsNullOrEmpty(respuesta))
                {
                    string[] partes = respuesta.Split(' ');
                    foreach (var parte in partes)
                    {
                        if (parte.StartsWith("SALA:"))
                            salaId = parte.Replace("SALA:", "").Trim();
                        else if (parte.StartsWith("ROLE:"))
                            rol = parte.Replace("ROLE:", "").Trim().ToLower();
                    }
                }

                if (string.IsNullOrEmpty(salaId))
                {
                    MessageBox.Show("No se pudo obtener el ID de la sala.");
                    return;
                }

                NavigationService.Navigate(new MatchScreen(salaId, rol, nombreJugador));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear sala: " + ex.Message);
            }
        }
    }
}