using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameServiceReference;
using HangmanClient.Model.DTO;
using HangmanClient.Model.Singleton;
using HangmanClient.Model.ViewModels;

namespace HangmanClient.View.Pages
{
    public partial class MatchScreen : Page
    {
        private readonly GameServiceClient gameService;
        private readonly string salaId;
        private readonly string rol;
        private readonly string nombreJugador;
        private DispatcherTimer refrescarTimer;
        private Socket socketCliente;
        int idLanguage = SessionManager.Instance.CurrentLanguage;

        public MatchScreen(string salaId, string rol, string nombreJugador)
        {
            InitializeComponent();

            this.salaId = salaId;
            this.rol = rol;
            this.nombreJugador = SessionManager.Instance.CurrentPlayer.Nickname;

            gameService = new GameServiceClient();

            ConectarSocket();
            CargarEstadoInicial();
            ConfigurarInterfaz();

            refrescarTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            refrescarTimer.Tick += RefrescarLetraPropuesta;
            refrescarTimer.Tick += RefrescarEstado;
            refrescarTimer.Start();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ConectarSocket();
            CargarEstadoInicial();
            CargarJugadoresEnPartida();
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

        private void RefrescarEstado(object sender, EventArgs e)
        {
            try
            {
                bool partidaTerminada = gameService.EsPartidaTerminada(int.Parse(salaId));

                string estadoPalabra = gameService.ObtenerEstadoPalabra(int.Parse(salaId));
                PalabraEstadoTextBlock.Text = estadoPalabra;
                ChallengerPalabraEstadoTextBlock.Text = estadoPalabra;

                string letraPropuesta = gameService.ObtenerLetraPropuesta(int.Parse(salaId));
                LetraPropuestaTextBlock.Text = letraPropuesta;

                int intentosRestantes = gameService.ObtenerIntentosRestantes(int.Parse(salaId));

                // Logs de depuración
                Console.WriteLine($"[RefrescarEstado] estadoPalabra: {estadoPalabra}");
                Console.WriteLine($"[RefrescarEstado] intentosRestantes: {intentosRestantes}");
                Console.WriteLine($"[RefrescarEstado] partidaTerminada: {partidaTerminada}");

                ActualizarIntentos(intentosRestantes);

                if (rol == "challenger")
                {
                    // Validación de estado de la palabra
                    if (!estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!estadoPalabra.Contains('_'))
                        {
                            try
                            {
                                bool palabraAdivinada = true;
                                Console.WriteLine($"[RefrescarEstado] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                                gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                                Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                            }

                            MessageBox.Show("¡La palabra ha sido adivinada! Regresando a la selección de sala.");
                            refrescarTimer?.Stop();
                            NavigationService.GoBack();
                        }
                        else if (intentosRestantes >= 6)
                        {
                            try
                            {
                                bool palabraAdivinada = false;
                                Console.WriteLine($"[RefrescarEstado] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                                gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                                Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                            }

                            MessageBox.Show("¡Se acabaron los intentos! Regresando a la selección de sala.");
                            refrescarTimer?.Stop();
                            NavigationService.GoBack();
                        }
                        else if (partidaTerminada)
                        {
                            try
                            {
                                bool palabraAdivinada = !estadoPalabra.Contains('_');
                                Console.WriteLine($"[RefrescarEstado] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                                gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                                Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                            }

                            MessageBox.Show("La partida ha terminado. Regresando a la selección de sala.");
                            refrescarTimer?.Stop();
                            NavigationService.GoBack();
                        }
                    }
                }
                else
                {
                    // Logs para guesser
                    if (!estadoPalabra.Contains('_') || intentosRestantes >= 6 || partidaTerminada)
                    {
                        Console.WriteLine("[RefrescarEstado] Guesser detectó fin de partida pero NO registra.");
                        MessageBox.Show("La partida ha terminado. Regresando a la selección de sala.");
                        refrescarTimer?.Stop();
                        NavigationService.GoBack();
                    }
                }

                CargarJugadoresEnPartida();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar estado: {ex.Message}");
            }
        }

        private void RefrescarLetraPropuesta(object sender, EventArgs e)
        {
            try
            {
                string letraPropuesta = gameService.ObtenerLetraPropuesta(int.Parse(salaId));
                LetraPropuestaTextBlock.Text = letraPropuesta;

                if (rol == "guesser")
                {
                    string estadoPalabra = gameService.ObtenerEstadoPalabra(int.Parse(salaId));
                    bool palabraEstablecida = !estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase);
                    bool letraPendiente = !string.IsNullOrEmpty(letraPropuesta);

                    Console.WriteLine($"[RefrescarLetraPropuesta] Guesser reactivando teclado: palabraEstablecida={palabraEstablecida}, letraPendiente={letraPendiente}");

                    foreach (Button btn in QwertyKeyboardPanel.Children)
                    {
                        btn.IsEnabled = palabraEstablecida && !letraPendiente;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar letra propuesta: {ex.Message}");
            }
        }

        private void ConfigurarInterfaz()
        {
            if (rol == "challenger")
            {
                ChallengerPanel.Visibility = Visibility.Visible;
                GuessPanel.Visibility = Visibility.Collapsed;
                MostrarOverlayPalabras();
            }
            else
            {
                ChallengerPanel.Visibility = Visibility.Collapsed;
                GuessPanel.Visibility = Visibility.Visible;
                GenerarTecladoQwerty();
                CargarEstadoInicial();
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
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                string letra = btn.Content.ToString();

                try
                {
                    gameService.EnviarLetra(int.Parse(salaId), letra);
                    LetraPropuestaTextBlock.Text = letra;

                    foreach (Button boton in QwertyKeyboardPanel.Children)
                    {
                        boton.IsEnabled = false;
                    }

                    MessageBox.Show($"Letra '{letra}' enviada. Esperando confirmación del challenger.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar letra: {ex.Message}");
                }
            }
        }

        private void MostrarOverlayPalabras()
        {
            try
            {
                var categoriasDTO = gameService.ObtenerCategorias(idLanguage);
                var categorias = categoriasDTO
                    .Select(c => new CategoryViewModel(c))
                    .ToList();

                foreach (var cat in categorias)
                {
                    Debug.WriteLine($"Categoría: {cat.Name}, Bytes: {cat.ImageBytes?.Length ?? 0}");
                }

                CategoriasListBox.ItemsSource = categorias;
                ListaPalabras.ItemsSource = null;
                OverlayPalabras.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las categorías: {ex.Message}");
            }
        }

        private void ConfirmarLetra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string letra = LetraPropuestaTextBlock.Text.Trim();
                if (string.IsNullOrEmpty(letra))
                {
                    MessageBox.Show("No hay una letra para confirmar.");
                    return;
                }

                string resultado = gameService.ConfirmarLetra(int.Parse(salaId), letra);

                switch (resultado)
                {
                    case "TRAMPA_CONFIRMAR":
                        MessageBox.Show("No es cierto, esa letra no se encuentra en la palabra!");
                        break;
                    case "CONFIRMADA":
                        MessageBox.Show("Letra confirmada.");
                        RefrescarLetraPropuesta(null, null);   // 🔧 Forzar refresco inmediato de la letra propuesta
                        RefrescarEstado(null, null);           // 🔧 Forzar refresco inmediato del estado de la partida
                        break;
                    case "PALABRA_ESTABLECIDA":
                        MessageBox.Show("Palabra establecida correctamente.");
                        break;
                    default:
                        MessageBox.Show("Error al confirmar la letra.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al confirmar letra: {ex.Message}");
            }
        }

        private void CargarEstadoInicial()
        {
            try
            {
                string estadoPalabra = gameService.ObtenerEstadoPalabra(int.Parse(salaId));
                PalabraEstadoTextBlock.Text = estadoPalabra;
                ChallengerPalabraEstadoTextBlock.Text = estadoPalabra;

                string letraPropuesta = gameService.ObtenerLetraPropuesta(int.Parse(salaId));
                LetraPropuestaTextBlock.Text = letraPropuesta;

                int intentosRestantes = gameService.ObtenerIntentosRestantes(int.Parse(salaId));
                ActualizarIntentos(intentosRestantes);

                if (gameService.EsPartidaTerminada(int.Parse(salaId)))
                {
                    if (rol == "challenger")
                    {
                        try
                        {
                            bool palabraAdivinada = !estadoPalabra.Contains('_');
                            Console.WriteLine($"[CargarEstadoInicial] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                            gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                            Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[CargarEstadoInicial] Guesser detectó fin de partida pero NO registra.");
                    }

                    MessageBox.Show("La partida ha terminado. Regresando a la selección de sala.");
                    refrescarTimer?.Stop();
                    NavigationService.GoBack();
                    return;
                }

                if (rol == "challenger")
                {
                    if (estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                    {
                        OverlayPalabras.Visibility = Visibility.Visible;
                        ChallengerPalabraEstadoTextBlock.Text = "Aún no has establecido una palabra.";
                        return;
                    }
                    else
                    {
                        OverlayPalabras.Visibility = Visibility.Collapsed;
                    }
                }
                else if (rol == "guesser")
                {
                    bool palabraEstablecida = !estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase);
                    bool letraPendiente = !string.IsNullOrEmpty(letraPropuesta);

                    foreach (Button btn in QwertyKeyboardPanel.Children)
                    {
                        btn.IsEnabled = palabraEstablecida && !letraPendiente;
                    }
                }

                if (!estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                {
                    if (!estadoPalabra.Contains('_'))
                    {
                        if (rol == "challenger")
                        {
                            try
                            {
                                bool palabraAdivinada = true;
                                Console.WriteLine($"[CargarEstadoInicial] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                                gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                                Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[CargarEstadoInicial] Guesser detectó fin de partida pero NO registra.");
                        }

                        MessageBox.Show("¡La palabra ha sido adivinada! Regresando a la selección de sala.");
                        refrescarTimer?.Stop();
                        NavigationService.GoBack();
                    }
                    else if (intentosRestantes >= 6)
                    {
                        if (rol == "challenger")
                        {
                            try
                            {
                                bool palabraAdivinada = false;
                                Console.WriteLine($"[CargarEstadoInicial] Challenger registra partida. salaId={salaId}, palabraAdivinada={palabraAdivinada}");
                                gameService.RegistrarPartidaFinalizada(int.Parse(salaId), palabraAdivinada);
                                Console.WriteLine("✅ Partida finalizada registrada correctamente.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error al registrar partida finalizada: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[CargarEstadoInicial] Guesser detectó fin de partida pero NO registra.");
                        }

                        MessageBox.Show("¡Se acabaron los intentos! Regresando a la selección de sala.");
                        refrescarTimer?.Stop();
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estado inicial: {ex.Message}");
            }
        }

        private void ActualizarIntentos(int intentosRestantes)
        {
            ChallengerIntentosPanel.Children.Clear();
            IntentosPanel.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                var rect = new Border
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(2),
                    Background = i < intentosRestantes ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red
                };

                ChallengerIntentosPanel.Children.Add(rect);
                IntentosPanel.Children.Add(new Border
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(2),
                    Background = i < intentosRestantes ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red
                });
            }
        }

        private void RechazarLetra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string letra = LetraPropuestaTextBlock.Text.Trim();
                if (string.IsNullOrEmpty(letra))
                {
                    MessageBox.Show("No hay letra propuesta para rechazar.");
                    return;
                }

                string resultado = gameService.RechazarLetra(int.Parse(salaId), letra);

                if (resultado == "TRAMPA")
                {
                    MessageBox.Show("¡No seas tramposo, esa letra sí está!");
                }
                else if (resultado == "RECHAZADA")
                {
                    MessageBox.Show("Letra rechazada.");
                }
                else
                {
                    MessageBox.Show("Error al procesar el rechazo de la letra.");
                }

                CargarEstadoInicial();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rechazar letra: {ex.Message}");
            }
        }

        private void SalirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[SalirButton_Click] Enviando solicitud de salida. salaId={salaId}, nombreJugador={nombreJugador}");
                gameService.Salir(int.Parse(salaId), nombreJugador);
                Console.WriteLine($"[SalirButton_Click] Solicitud de salida enviada exitosamente.");
                refrescarTimer?.Stop();
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al salir de la sala: {ex.Message}");
                MessageBox.Show($"Error al salir de la sala: {ex.Message}");
            }
            finally
            {
                refrescarTimer?.Stop();
            }
        }

        private void SeleccionarPalabra_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is WordViewModel palabraSeleccionada)
            {
                try
                {
                    gameService.ConfirmarLetra(int.Parse(salaId), palabraSeleccionada.Name);
                    MessageBox.Show("Palabra establecida correctamente.");
                    OverlayPalabras.Visibility = Visibility.Collapsed;
                    CargarEstadoInicial();
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


        private void CargarJugadoresEnPartida()
        {
            try
            {
                var jugadores = gameService.ObtenerJugadoresEnPartida(int.Parse(salaId));

                Dispatcher.Invoke(() =>
                {
                    ChallengerJugadoresPanel.Children.Clear();
                    GuessJugadoresPanel.Children.Clear();
                    ChallengerJugadoresPanel.Children.Add(new TextBlock
                    {
                        Text = "Jugadores en la partida:",
                        FontWeight = FontWeights.Bold
                    });
                    GuessJugadoresPanel.Children.Add(new TextBlock
                    {
                        Text = "Jugadores en la partida:",
                        FontWeight = FontWeights.Bold
                    });
                    foreach (var jugador in jugadores)
                    {
                        ChallengerJugadoresPanel.Children.Add(new TextBlock { Text = jugador });
                        GuessJugadoresPanel.Children.Add(new TextBlock { Text = jugador });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar jugadores en la partida: {ex.Message}");
            }
        }

        private void CategoriasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriasListBox.SelectedItem is CategoryViewModel categoriaSeleccionada)
            {
                try
                {
                    var palabrasDTO = gameService.ObtenerPalabrasPorCategoria(categoriaSeleccionada.Name, idLanguage);
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

    }
}