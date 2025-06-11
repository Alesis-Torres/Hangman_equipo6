using GameServiceReference;
using System;
using System.Configuration;
using System.Data;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace HangmanClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static GameServiceClient GameServiceClient { get; private set; }

        private const int MaxReintentos = 3;
        private const string ServiceUrl = "http://localhost:64520/GameService.svc";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InicializarServicioConReintento();
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
                    var endpoint = new EndpointAddress(ServiceUrl);
                    binding.ReaderQuotas.MaxArrayLength = 5242880;
                    binding.ReaderQuotas.MaxBytesPerRead = 5242880;
                    binding.ReaderQuotas.MaxDepth = 32;
                    GameServiceClient = new GameServiceClient(binding, endpoint);
                    
                    string resultado = GameServiceClient.ProbarConexion();
                    Console.WriteLine($"Conexión exitosa: {resultado}");

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
    }
}