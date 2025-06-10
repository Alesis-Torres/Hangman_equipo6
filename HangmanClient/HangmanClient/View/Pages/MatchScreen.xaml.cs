using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameServiceReference;

namespace HangmanClient.View.Pages
{
    public partial class MatchScreen : Page
    {
        private readonly GameServiceClient gameService;
        private readonly string salaId;
        private readonly string rol;
        private readonly string nombreJugador;
        private DispatcherTimer refrescarTimer;

        public MatchScreen(string salaId, string rol, string nombreJugador)
        {
            InitializeComponent();

            this.salaId = salaId;
            this.rol = rol;
            this.nombreJugador = nombreJugador;

            gameService = new GameServiceClient();

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

        private void RefrescarEstado(object sender, EventArgs e)
        {
            try
            {
                if (gameService.EsPartidaTerminada(int.Parse(salaId)))
                {
                    MessageBox.Show("La partida ha terminado. Regresando a la selección de sala.");
                    refrescarTimer?.Stop();
                    NavigationService.GoBack();
                    return;
                }

                string estadoPalabra = gameService.ObtenerEstadoPalabra(int.Parse(salaId));
                PalabraEstadoTextBlock.Text = estadoPalabra;
                ChallengerPalabraEstadoTextBlock.Text = estadoPalabra;

                string letraPropuesta = gameService.ObtenerLetraPropuesta(int.Parse(salaId));
                LetraPropuestaTextBlock.Text = letraPropuesta;

                int intentosRestantes = gameService.ObtenerIntentosRestantes(int.Parse(salaId));
                ActualizarIntentos(intentosRestantes);

                if (rol == "challenger")
                {
                    if (estadoPalabra.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                    {
                        OverlayPalabras.Visibility = Visibility.Visible;
                        ChallengerPalabraEstadoTextBlock.Text = "Aún no has establecido una palabra.";
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
            var palabras = new[] { "Pato", "Avion", "Computadora", "Sol", "Luna" };
            ListaPalabras.ItemsSource = palabras;
            OverlayPalabras.Visibility = Visibility.Visible;
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
                        MessageBox.Show("¡La palabra ha sido adivinada! Regresando a la selección de sala.");
                        refrescarTimer?.Stop();
                        NavigationService.GoBack();
                    }
                    else if (intentosRestantes >= 6)
                    {
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
                gameService.Salir(int.Parse(salaId), nombreJugador);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al salir de la sala: {ex.Message}");
            }
            finally
            {
                refrescarTimer?.Stop();
            }
        }

        private void SeleccionarPalabra_Click(object sender, RoutedEventArgs e)
        {
            if (ListaPalabras.SelectedItem is string palabraSeleccionada)
            {
                try
                {
                    gameService.ConfirmarLetra(int.Parse(salaId), palabraSeleccionada);
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
                MessageBox.Show("Selecciona una palabra de la lista.");
            }
        }
    }
}