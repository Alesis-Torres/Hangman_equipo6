using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using HangmanClient.Model.Singleton;
using HangmanClient.Model.ViewModels;
using System.IO;
using System.ServiceModel.Channels;
using Timer = System.Timers.Timer;

namespace HangmanClient.View.Pages
{
    public partial class MatchScreen : Page
    {
        private readonly int salaId;
        private readonly string rol;
        private readonly string nombreJugador;
        int idLanguage = SessionManager.Instance.CurrentLanguage;
        private DispatcherTimer estadoTimer;
        private bool turnoGuesser = false;
        private HashSet<string> letrasUsadas = new HashSet<string>();
        private DispatcherTimer inactividadTimer;

        private Timer escuchaTimer;

        public MatchScreen(int salaId, string rol, string nombreJugador)
        {
            InitializeComponent();
            this.salaId = salaId;
            this.rol = rol;
            this.nombreJugador = SessionManager.Instance.CurrentPlayer.Nickname;
            ConfigurarInterfaz();

            escuchaTimer = new Timer(500);
            escuchaTimer.Elapsed += (s, e) => Dispatcher.Invoke(EscucharSocket);
            escuchaTimer.Start();
            IniciarEscuchaPartida();
            checarBloqueo();
            this.Unloaded += MatchScreen_Unloaded;
        }

        private void checarBloqueo()
        {
            inactividadTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            inactividadTimer.Tick += (s, e) =>
            {
                inactividadTimer.Stop(); 
                try
                {
                    string mensaje = $"SOLICITAR_ESTADO|{salaId}\n";
                    SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(mensaje));
                    Debug.WriteLine("[Inactividad] Estado solicitado automáticamente.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[Inactividad] Error al solicitar estado: " + ex.Message);
                }
            };
        }
        private void ReiniciarInactividad()
        {
            inactividadTimer?.Stop();
            inactividadTimer?.Start();
        }
        private void IniciarEscuchaPartida()
        {
            estadoTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            estadoTimer.Tick += (s, e) =>
            {
                try
                {
                    string mensaje = $"SOLICITAR_ESTADO|{salaId}\n";
                    SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(mensaje));
                    Console.WriteLine($"[Estado] Enviado: {mensaje}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Estado] Error al enviar solicitud de estado: {ex.Message}");
                    estadoTimer?.Stop();
                }
            };
            estadoTimer.Start();
        }

        private void MatchScreen_Unloaded(object sender, RoutedEventArgs e)
        {
            escuchaTimer?.Stop();
            estadoTimer?.Stop();
            escuchaTimer = null;
            estadoTimer = null;
        }

        private void EscucharSocket()
        {
            try
            {
                if (SessionManager.Instance.SocketCliente.Available > 0)
                {
                    byte[] buffer = new byte[2048];
                    int bytes = SessionManager.Instance.SocketCliente.Receive(buffer);
                    string datos = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                    var mensajes = datos.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mensaje in mensajes)
                    {
                        ProcesarMensajeServidor(mensaje.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                escuchaTimer?.Stop();
                MessageBox.Show("Error al recibir datos del servidor: " + ex.Message);
                NavigationService.Navigate(new CreateMatch(false, "Conexión interrumpida."));
            }
        }

        private void DetenerEscucha()
        {
            escuchaTimer?.Stop();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            EscucharSocket();
        }

        private void ProcesarMensajeServidor(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            string comando = partes[0];
            Debug.WriteLine(mensaje);
            switch (comando)
            {


                case "LETRA":
                    if (partes.Length >= 2)
                    {
                        LetraPropuestaTextBlock.Text = partes[1];
                    }
                    break;


                case "OPONENTE_DESCONECTADO":
                    DetenerEscucha();
                    NavigationService.Navigate(new CreateMatch(false, "Tu oponente se desconectó. La partida fue registrada como inconclusa."));
                    break;

                case "ESTADO_PARTIDA":
                    if (partes.Length >= 5)
                    {

                        string estadoPalabra = partes[1];
                        int intentosPartida = int.Parse(partes[2].Split(':')[1]);
                        string jugadoresConcat = partes.FirstOrDefault(p => p.StartsWith("JUGADORES:"))?.Substring(10);
                        string[] jugadores = jugadoresConcat?.Split(',') ?? Array.Empty<string>();
                        string estadoPartida = partes.FirstOrDefault(p => p.StartsWith("ESTADO:"))?.Substring(7);
                        string letraPropuesta = partes.FirstOrDefault(p => p.StartsWith("LETRA:"))?.Substring(6);
                        string accion = partes.FirstOrDefault(p => p.StartsWith("ACCION:"))?.Substring(7);
                        string turno = partes.FirstOrDefault(p => p.StartsWith("TECLADO:"))?.Substring(8);
                        Debug.WriteLine(turno);
                        if (turno == "CHALLENGER")
                        {
                            ConfirmLetterButton.IsEnabled = true;
                            DeclineLetterButton.IsEnabled = true;
                            CambiarEstadoTeclado(false);
                        }
                        else if (turno == "GUESSER")
                        {
                            ConfirmLetterButton.IsEnabled = false;
                            DeclineLetterButton.IsEnabled = false;
                            CambiarEstadoTeclado(true);
                        }


                        Dispatcher.Invoke(() =>
                        {
                            ActualizarIntentos(intentosPartida);

                            if (OverlayPalabras.Visibility != Visibility.Visible)
                            {
                                PalabraEstadoTextBlock.Text = estadoPalabra;
                                ChallengerPalabraEstadoTextBlock.Text = estadoPalabra;

                                if (!string.IsNullOrWhiteSpace(letraPropuesta))
                                {
                                    LetraPropuestaTextBlock.Text = letraPropuesta;
                                }

                                if (!string.IsNullOrWhiteSpace(accion))
                                {
                                    switch (accion.ToUpperInvariant())
                                    {
                                        case "ACEPTADA":
                                            ReproducirSonidoLetraCorrecta();
                                            break;
                                        case "TRAMPA_CONFIRMAR":
                                            MessageBox.Show($"La letra '{letraPropuesta}' no estaba en la palabra. ¡Trampa al aceptar!");
                                            break;
                                        case "TRAMPA_RECHAZAR":
                                            MessageBox.Show($"La letra '{letraPropuesta}' sí estaba en la palabra. ¡Trampa al rechazar!");
                                            break;
                                        case "RECHAZADA":
                                            MessageBox.Show($"Letra '{letraPropuesta}' fue rechazada.");
                                            break;
                                    }

                                    LetraPropuestaTextBlock.Text = "";
                                }

                                ChallengerJugadoresPanel.Children.Clear();
                                GuessJugadoresPanel.Children.Clear();

                                foreach (var jugador in jugadores)
                                {
                                    var textChallenger = new TextBlock
                                    {
                                        Text = jugador,
                                        Foreground = Brushes.White,
                                        FontSize = 14,
                                        Margin = new Thickness(5)
                                    };
                                    var textGuesser = new TextBlock
                                    {
                                        Text = jugador,
                                        Foreground = Brushes.White,
                                        FontSize = 14,
                                        Margin = new Thickness(5)
                                    };
                                    ChallengerJugadoresPanel.Children.Add(textChallenger);
                                    GuessJugadoresPanel.Children.Add(textGuesser);
                                }

                                switch (estadoPartida?.ToUpperInvariant())
                                {
                                    case "GANASTE":
                                        inactividadTimer.Stop();
                                        DetenerEscucha();
                                        FinalizarPartida("GANASTE");
                                        break;
                                    case "PERDISTE":
                                        inactividadTimer.Stop();
                                        DetenerEscucha();
                                        FinalizarPartida("PERDISTE");
                                        break;
                                    case "DESCONEXION":
                                        inactividadTimer.Stop();
                                        DetenerEscucha();
                                        FinalizarPartida("DESCONEXION");
                                        break;
                                    case "INICIADA":
                                        if (EsperandoOverlay.Visibility == Visibility.Visible)
                                        {
                                            EsperandoOverlay.Visibility = Visibility.Collapsed;
                                            MostrarOverlayInicioPartida();
                                        }
                                        estadoTimer?.Stop();
                                        break;
                                    case "EN_ESPERA":
                                        if (EsperandoOverlay.Visibility != Visibility.Visible)
                                            EsperandoOverlay.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                        });
                    }
                    break;


                case "PALABRA_ESTABLECIDA":
                    if (OverlayPalabras.Visibility == Visibility.Visible)
                    {
                        MessageBox.Show("Palabra establecida correctamente.");
                        OverlayPalabras.Visibility = Visibility.Collapsed;
                    }
                    break;

                case "YA_HAY_PALABRA":
                    MessageBox.Show("Ya se ha establecido una palabra.");
                    break;

                case "ERROR_PALABRA":
                    MessageBox.Show("Hubo un error al establecer la palabra.");
                    break;

                default:
                    Console.WriteLine($"[Servidor] Comando no reconocido: {mensaje}");
                    break;
            }
        }

        private void ReproducirSonidoLetraCorrecta()
        {
            /*try
            {
                var player = new System.Media.SoundPlayer("Assets/letra_correcta.wav");
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reproducir sonido: {ex.Message}");
            }*/
        }
        
        private void MostrarOverlayInicioPartida()
        {
            InicioOverlay.Visibility = Visibility.Visible;

            // Reproduce sonido de inicio de partida (opcional)
            // var player = new SoundPlayer("Assets/partida_inicio.wav");
            // player.Play();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                InicioOverlay.Visibility = Visibility.Collapsed;
            };
            timer.Start();
        }


        private void FinalizarPartida(string mensaje)
        {
            string resultado = mensaje switch
            {
                var m when m.StartsWith("GANASTE") => "¡Ganaste!",
                var m when m.StartsWith("PERDISTE") => "Perdiste. Intenta nuevamente.",
                _ => "La partida terminó de forma inesperada por desconexion."
            };
            NavigationService.Navigate(new CreateMatch(false, resultado));
        }


        private void ConfigurarInterfaz()
        {
            if (rol == "challenger")
            {
                string codigo = SessionManager.Instance.GameServiceClient.ObtenerCodigoDeSala(salaId);


                CodigoSalaTextBlock.Text = $"Código de sala: {codigo}";
                ChallengerPanel.Visibility = Visibility.Visible;
                GuessPanel.Visibility = Visibility.Collapsed;
                MostrarOverlayPalabras();
                
            }
            else
            {
                ChallengerPanel.Visibility = Visibility.Collapsed;
                GuessPanel.Visibility = Visibility.Visible;
                GenerarTecladoQwerty();
            }
        }

        private void GenerarTecladoQwerty()
        {
            QwertyKeyboardPanel.Children.Clear();
            string letrasQwerty = "QWERTYUIOPASDFGHJKLZXCVBNM";

            foreach (char letra in letrasQwerty)
            {
                var boton = new Button
                {
                    Content = letra.ToString(),
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(2),
                    IsEnabled = false
                };
                boton.Click += LetraButton_Click;
                QwertyKeyboardPanel.Children.Add(boton);
            }
        }

        private void LetraButton_Click(object sender, RoutedEventArgs e)
        {
            ReiniciarInactividad();
            if (sender is Button btn)
            {
                string letra = btn.Content.ToString();

                try
                {
                    string comando = $"LETRA|{letra}|{salaId}";
                    SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(comando));
                    letrasUsadas.Add(letra); 
                    LetraPropuestaTextBlock.Text = letra;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar letra: {ex.Message}");
                }
            }
        }

        private void CambiarEstadoTeclado(bool habilitar)
        {
            foreach (Button boton in QwertyKeyboardPanel.Children.OfType<Button>())
            {
                string letra = boton.Content.ToString();
                if (habilitar)
                {
                    if (!letrasUsadas.Contains(letra))
                        boton.IsEnabled = true;
                }
                else
                {
                    boton.IsEnabled = false;
                }
            }
        }

        private void ConfirmarLetra_Click(object sender, RoutedEventArgs e)
        {
            ReiniciarInactividad();
            try
            {
                string letra = LetraPropuestaTextBlock.Text.Trim();
                if (string.IsNullOrEmpty(letra))
                {
                    MessageBox.Show("No hay una letra para confirmar.");
                    return;
                }

                string comando = $"CONFIRMAR_LETRA|{letra}|{salaId}";
                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(comando));
                MessageBox.Show($"Letra '{letra}' confirmada. Se notificará al guesser.");
                LetraPropuestaTextBlock.Text = "";
                OverlayPalabras.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al confirmar letra: {ex.Message}");
            }
        }


        private void ActualizarIntentos(int intentosRestantes)
        {
            int intentosFallidos = 6 - intentosRestantes;

            // Ocultar todas las partes
            Ch_Head.Visibility = Head.Visibility = Visibility.Hidden;
            Ch_Torso.Visibility = Torso.Visibility  = Visibility.Hidden;
            Ch_LeftArm.Visibility = LeftArm.Visibility  = Visibility.Hidden;
            Ch_RightArm.Visibility = RightArm.Visibility = Visibility.Hidden;
            Ch_LeftLeg.Visibility = LeftLeg.Visibility= Visibility.Hidden;
            Ch_RightLeg.Visibility = RightLeg.Visibility = Visibility.Hidden;

            if (intentosFallidos >= 1)
            {
                Ch_Head.Visibility = Visibility.Visible;
                Head.Visibility = Visibility.Visible;
            }
            if (intentosFallidos >= 2)
            {
                Ch_Torso.Visibility = Visibility.Visible;
                Torso.Visibility = Visibility.Visible;
            }
            if (intentosFallidos >= 3)
            {
                Ch_LeftArm.Visibility = Visibility.Visible;
                LeftArm.Visibility = Visibility.Visible;
            }
            if (intentosFallidos >= 4)
            {
                Ch_RightArm.Visibility = Visibility.Visible;
                RightArm.Visibility = Visibility.Visible;
            }
            if (intentosFallidos >= 5)
            {
                Ch_LeftLeg.Visibility = Visibility.Visible;
                LeftLeg.Visibility = Visibility.Visible;
            }
            if (intentosFallidos >= 6)
            {
                Ch_RightLeg.Visibility = Visibility.Visible;
                RightLeg.Visibility = Visibility.Visible;
            }
        }

        private void RechazarLetra_Click(object sender, RoutedEventArgs e)
        {
            ReiniciarInactividad();
            string letra = LetraPropuestaTextBlock.Text.Trim();
            if (string.IsNullOrEmpty(letra))
            {
                MessageBox.Show("No hay letra propuesta para rechazar.");
                return;
            }

            string comando = $"RECHAZAR_LETRA|{letra}|{salaId}";
            SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(comando));
        }


        private void SalirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string comando = $"SALIR|{nombreJugador}|{salaId}";
                SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(comando));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al salir: {ex.Message}");
            }
            finally
            {
                try { SessionManager.Instance.SocketCliente?.Shutdown(SocketShutdown.Both); } catch { }
                try { SessionManager.Instance.SocketCliente?.Close(); } catch { }

                NavigationService.Navigate(new Login());
            }
        }

        private void SeleccionarPalabra_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WordViewModel palabraSeleccionada)
            {
                try
                {
                    int idSala = salaId;
                    string palabra = palabraSeleccionada.Name;
                    string comando = $"PALABRA|{palabraSeleccionada.Name}|{idSala}";

                    SessionManager.Instance.SocketCliente.Send(Encoding.UTF8.GetBytes(comando));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al establecer la palabra: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Selecciona una palabra válida.");
            }
        }




        private void CategoriasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriasListBox.SelectedItem is CategoryViewModel categoriaSeleccionada)
            {
                try
                {
                    var palabrasDTO = SessionManager.Instance.GameServiceClient.ObtenerPalabrasPorCategoria(categoriaSeleccionada.Name, idLanguage);
                    var palabras = palabrasDTO
                        .Select(p => new WordViewModel(p))
                        .ToList();

                    foreach (var word in palabras)
                    {
                        Debug.WriteLine($"Palabra: {word.Name}, Bytes: {word.ImageBytes?.Length ?? 0}");
                    }

                    ListaPalabras.ItemsSource = palabras;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar palabras: {ex.Message}");
                }
            }
        }
        private void MostrarOverlayPalabras()
        {
            try
            {
                var categoriasDTO = SessionManager.Instance.GameServiceClient.ObtenerCategorias(idLanguage);
                var categorias = categoriasDTO
                    .Select(c => new CategoryViewModel(c))
                    .ToList();

                CategoriasListBox.ItemsSource = categorias;
                ListaPalabras.ItemsSource = null;
                OverlayPalabras.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las categorías: {ex.Message}");
            }
        }

    }
}
