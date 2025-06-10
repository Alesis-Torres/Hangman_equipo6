using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Hangman_Server
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GameService : IGameService
    {
        private readonly SocketServidor _servidor = SocketServidor.ObtenerInstancia();

        public string ProbarConexion()
        {
            return "Conexión establecida";
        }

        public string ObtenerSalas()
        {
            try
            {
                return _servidor.ObtenerSalas();
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
        public string CrearSala(string nombreJugador)
        {
            int salaId = _servidor.CrearSala(nombreJugador);
            return $"SALA:{salaId} ROLE:challenger";
        }
        public bool EsPartidaTerminada(int salaId)
        {
            return _servidor.EsPartidaTerminada(salaId);
        }

        public string UnirseSala(int salaId, string nombreJugador)
        {
            string rol = _servidor.AgregarJugador(salaId, nombreJugador);
            return $"ROLE:{rol}";
        }

        public void Salir(int salaId, string nombreJugador)
        {
            _servidor.EliminarJugador(salaId, nombreJugador);
        }

        public int ObtenerJugadoresEnSala(int salaId)
        {
            return _servidor.ObtenerJugadoresEnSala(salaId);
        }
        public string AgregarJugador(int salaId, string nombreJugador)
        {
            return _servidor.AgregarJugador(salaId, nombreJugador);
        }

        public string ObtenerEstadoPalabra(int salaId)
        {
            try
            {
                return _servidor.ObtenerEstadoPalabra(salaId);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public int ObtenerIntentosRestantes(int salaId)
        {
            try
            {
                return _servidor.ObtenerIntentosRestantes(salaId);
            }
            catch
            {
                return 0;
            }
        }

        public string ObtenerLetraPropuesta(int salaId)
        {
            try
            {
                return _servidor.ObtenerLetraPropuesta(salaId);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public void EnviarLetra(int salaId, string letra)
        {
            try
            {
                _servidor.EnviarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EnviarLetra: {ex.Message}");
            }
        }

        public string ConfirmarLetra(int salaId, string letra)
        {
            try
            {
                return _servidor.ConfirmarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ConfirmarLetra: {ex.Message}");
                return "ERROR";
            }
        }

        public string RechazarLetra(int salaId, string letra)
        {
            try
            {
                return _servidor.RechazarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en RechazarLetra: {ex.Message}");
                return "ERROR";
            }
        }
        
    }
}