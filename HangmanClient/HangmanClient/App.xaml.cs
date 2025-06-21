using GameServiceReference;
using HangmanClient.Model.Singleton;
using HangmanServiceReference;
using System;
using System.Configuration;
using System.Data;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HangmanClient
{
    public partial class App : Application
    {
        private const int MaxReintentos = 3;
        private const string GameServiceUrl = "http://172.20.10.3/Hangman_Server/GameService.svc";
        private const string HangmanServiceUrl = "http://172.20.10.3/Hangman_Server/HangmanService.svc";
        private MediaPlayer backgroundMusicPlayer;

        private TcpClient monitorClient;
        private Thread monitorThread;
        private DispatcherTimer pingTimer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeBackgroundMusic();
            InicializarServicioConReintento();
            SessionManager.Instance.CurrentLanguage = 1;
        }

        private void InitializeBackgroundMusic()
        {
            try
            {
                backgroundMusicPlayer = new MediaPlayer();
                // Ruta relativa al archivo de audio en la carpeta de salida
                string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Music", "musica_fondo.wav");
                backgroundMusicPlayer.Open(new Uri(musicPath, UriKind.Absolute));
                backgroundMusicPlayer.MediaEnded += (s, args) =>
                {
                    // Repetir la música cuando termine
                    backgroundMusicPlayer.Position = TimeSpan.Zero;
                    backgroundMusicPlayer.Play();
                };
                backgroundMusicPlayer.Volume = 0.5; // Ajustar volumen (0.0 a 1.0)
                backgroundMusicPlayer.Play(); // Iniciar reproducción
            }
            catch (Exception ex)
            {}
        }

        private async void InicializarServicioConReintento()
        {
            int reintento = 0;
            bool conectado = false;

            while (!conectado && reintento < MaxReintentos)
            {
                try
                {
                    var binding = new BasicHttpBinding();
                    var gameEndpoint = new EndpointAddress(GameServiceUrl);
                    var hangmanEndpoint = new EndpointAddress(HangmanServiceUrl);
                    binding.MaxReceivedMessageSize = 5242880;
                    binding.ReaderQuotas.MaxArrayLength = 5242880;
                    binding.ReaderQuotas.MaxBytesPerRead = 5242880;
                    binding.ReaderQuotas.MaxDepth = 32;

                    var gameClient = new GameServiceClient(binding, gameEndpoint);
                    string resultado = gameClient.ProbarConexion();
                    Console.WriteLine($"Conexión exitosa: {resultado}");
                    var hangmanClient = new HangmanServiceClient(binding, hangmanEndpoint);
                    SessionManager.Instance.InicializarGameServiceClient(gameClient);
                    SessionManager.Instance.InicializarHangmanServiceClient(hangmanClient);
                    conectado = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error de conexión (intento {reintento + 1}): {ex.Message}");
                    reintento++;
                    await Task.Delay(2000);
                }
            }

            if (!conectado)
            {
                MessageBox.Show("No se pudo conectar con el servidor tras varios intentos. La aplicación se cerrará.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        public void IniciarSocketMonitor(string username, int idJugador)
        {
            try
            {
                SessionManager.Instance.SocketCliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SessionManager.Instance.SocketCliente.Connect("172.20.10.3", 1002);

                pingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(10)
                };
                pingTimer.Tick += (s, e) =>
                {
                    try
                    {
                        string pingMsg = $"PING|{username}|{idJugador}\n";
                        byte[] buffer = Encoding.UTF8.GetBytes(pingMsg);
                        SessionManager.Instance.SocketCliente.Send(buffer);
                        Console.WriteLine($"[PING enviado] {pingMsg}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PING ERROR] {ex.Message}");
                        pingTimer.Stop();
                    }
                };
                pingTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servidor: {ex.Message}", "Conexión fallida", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DetenerSocketMonitor()
        {
            try
            {
                monitorClient?.Close();
                monitorThread?.Interrupt();
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}