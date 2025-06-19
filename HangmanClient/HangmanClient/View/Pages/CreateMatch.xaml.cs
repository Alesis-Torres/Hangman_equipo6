using System;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HangmanClient.Model.Singleton;
using Microsoft.IdentityModel.Tokens.Saml2;
using Timer = System.Timers.Timer;

namespace HangmanClient.View.Pages
{
    public partial class CreateMatch : Page
    {
        private Timer actualizacionSalasTimer = new Timer(3000);
        private readonly bool esLogin;
        private int idioma = SessionManager.Instance.CurrentLanguage;

        public CreateMatch(bool esLogin, string mensaje)
        {
            InitializeComponent();
            this.esLogin = esLogin;
            actualizacionSalasTimer = new Timer(3000);
            actualizacionSalasTimer.Elapsed += (s, e) => Dispatcher.Invoke(ActualizarSalas);
            actualizacionSalasTimer.Start();
            this.Unloaded += CreateMatch_Unloaded;
            if (!mensaje.Equals(""))
            {
                MessageBox.Show(mensaje, "Retorno a la pantalla de inicio");
            }
        }
        private void CreateMatch_Unloaded(object sender, RoutedEventArgs e)
        {
            actualizacionSalasTimer?.Stop();
            actualizacionSalasTimer = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            ActualizarSalas();
        }
        public void DetenerTimer()
        {
            actualizacionSalasTimer?.Stop();
        }



        private void ActualizarSalas()
        {
            try
            {
                string solicitud = $"OBTENER_SALAS|{idioma}";
                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(solicitud));
                byte[] buffer = new byte[2048];
                int bytes = SessionManager.Instance.SocketCliente.Receive(buffer);
                string respuesta = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                if (respuesta.StartsWith("SALAS|"))
                {
                    string contenido = respuesta.Substring("SALAS|".Length);
                    var salas = contenido.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    Dispatcher.Invoke(() =>
                    {
                        SalasListBox.ItemsSource = salas.Select(s => s.Trim()).ToList();
                    });
                }
                else
                {
                    Console.WriteLine("Respuesta inesperada al actualizar salas: " + respuesta);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Error al actualizar salas: " + ex.Message));
            }
        }


        private void CrearSalaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                actualizacionSalasTimer?.Stop();

                string nombreJugador = SessionManager.Instance.CurrentPlayer.Nickname;
                int idJugador = SessionManager.Instance.CurrentPlayer.IdPlayer;
                string mensaje = $"CREAR_SALA|{idJugador}|{nombreJugador}|{idioma}";
                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(mensaje));
                if (SessionManager.Instance.SocketCliente.Poll(5_000_000, SelectMode.SelectRead))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRecibidos = SessionManager.Instance.SocketCliente.Receive(buffer);
                    string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesRecibidos).Trim();

                    if (respuesta.StartsWith("SALA_CREADA|"))
                    {
                        int salaId = int.Parse(respuesta.Split('|')[1]);
                        NavigationService.Navigate(new MatchScreen(salaId, "challenger", nombreJugador));
                    }
                    else
                    {
                        MessageBox.Show("Error al crear sala: " + respuesta);
                        actualizacionSalasTimer?.Start();
                    }
                }
                else
                {
                    MessageBox.Show("Timeout esperando respuesta del servidor.");
                    actualizacionSalasTimer?.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear sala: " + ex.Message);
                actualizacionSalasTimer?.Start();
            }
        }

        private void UnirseDirectoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                actualizacionSalasTimer?.Stop();

                if (sender is not Button boton) return;

                ListBoxItem item = ItemsControl.ContainerFromElement(SalasListBox, boton) as ListBoxItem;
                if (item == null)
                {
                    MessageBox.Show("No se pudo determinar la sala correspondiente.");
                    actualizacionSalasTimer?.Start();
                    return;
                }

                int index = SalasListBox.ItemContainerGenerator.IndexFromContainer(item);
                if (index < 0 || index >= SalasListBox.Items.Count)
                {
                    MessageBox.Show("Índice inválido.");
                    actualizacionSalasTimer?.Start();
                    return;
                }

                string lineaSala = SalasListBox.Items[index] as string;
                if (string.IsNullOrWhiteSpace(lineaSala))
                {
                    MessageBox.Show("Formato de sala inválido.");
                    actualizacionSalasTimer?.Start();
                    return;
                }
                string codigo = "";
                string[] partes = lineaSala.Split('-');
                foreach (var parte in partes)
                {
                    if (parte.Trim().StartsWith("Código:"))
                    {
                        codigo = parte.Trim().Substring("Código:".Length).Trim();
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(codigo) || codigo.Length != 6)
                {
                    MessageBox.Show("No se pudo extraer un código válido de la sala.");
                    actualizacionSalasTimer?.Start();
                    return;
                }

                string nickname = SessionManager.Instance.CurrentPlayer.Nickname;
                int idJugador = SessionManager.Instance.CurrentPlayer.IdPlayer;
                string mensaje = $"UNIRSE_CODIGO|{codigo}|{nickname}|{idJugador}";
                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(mensaje));

                byte[] buffer = new byte[1024];
                int bytesRecibidos = SessionManager.Instance.SocketCliente.Receive(buffer);
                string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesRecibidos).Trim();

                if (respuesta.StartsWith("UNIDO|"))
                {
                    int idSala = int.Parse(respuesta.Split('|')[1]);
                    NavigationService.Navigate(new MatchScreen(idSala, "guesser", nickname));
                }
                else
                {
                    MessageBox.Show("No fue posible unirse a la sala: " + respuesta);
                    actualizacionSalasTimer?.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar unirse a la sala: " + ex.Message);
                actualizacionSalasTimer?.Start();
            }
        }

        private void UnirsePorCodigoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Ingresa el código de la sala (ej. 3FJ8ZQ):", "Unirse por código", "");

                if (string.IsNullOrWhiteSpace(input) || input.Length != 6 || !input.All(char.IsLetterOrDigit))
                {
                    MessageBox.Show("Código inválido. Asegúrate de ingresar un código de 6 caracteres alfanuméricos.");
                    return;
                }

                actualizacionSalasTimer?.Stop();

                string codigo = input.ToUpperInvariant();
                string nickname = SessionManager.Instance.CurrentPlayer.Nickname;
                int idJugador = SessionManager.Instance.CurrentPlayer.IdPlayer;
                string mensaje = $"UNIRSE_CODIGO|{codigo}|{nickname}|{idJugador}";

                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(mensaje));

                byte[] buffer = new byte[1024];
                int bytesRecibidos = SessionManager.Instance.SocketCliente.Receive(buffer);
                string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesRecibidos).Trim();

                if (respuesta.StartsWith("UNIDO|"))
                {
                    int idSala = int.Parse(respuesta.Split('|')[1]);
                    NavigationService.Navigate(new MatchScreen(idSala, "guesser", nickname));
                }
                else
                {
                    MessageBox.Show("No fue posible unirse a la sala: " + respuesta);
                    actualizacionSalasTimer?.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar unirse por código: " + ex.Message);
                actualizacionSalasTimer?.Start();
            }
        }

        private void VerEstadisticasButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new GameStadistics());
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var isPopupOpen = PopUp.IsOpen;

            if (!isPopupOpen)
            {
                PopUp.IsOpen = true;
                PopUp.StaysOpen = false;
            }
            else
            {
                PopUp.IsOpen = false;
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a formulario de edición de perfil
        }

        private void ViewScoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a pagina de estadisticas
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var jugador = SessionManager.Instance.CurrentPlayer;

                if (jugador != null && SessionManager.Instance.SocketCliente?.Connected == true)
                {
                    string mensajeLogout = $"LOGOUT|{jugador.IdPlayer}";
                    byte[] buffer = Encoding.UTF8.GetBytes(mensajeLogout + "\n");
                    SessionManager.Instance.SocketCliente.Send(buffer);
                    if (SessionManager.Instance.SocketCliente.Poll(3_000_000, SelectMode.SelectRead))
                    {
                        byte[] respuestaBuffer = new byte[1024];
                        int recibidos = SessionManager.Instance.SocketCliente.Receive(respuestaBuffer);
                        string respuesta = Encoding.UTF8.GetString(respuestaBuffer, 0, recibidos).Trim();

                        if (!respuesta.StartsWith("LOGOUT_OK"))
                        {
                            MessageBox.Show("El servidor no confirmó el cierre de sesión: " + respuesta);
                        }
                    }

                    SessionManager.Instance.SocketCliente.Shutdown(SocketShutdown.Both);
                    SessionManager.Instance.SocketCliente.Close();
                }

                SessionManager.Instance.CurrentPlayer = null;
                NavigationService.Navigate(new Login());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar sesión: " + ex.Message);
            }
        }
    }
}