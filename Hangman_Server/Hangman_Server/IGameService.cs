using System.Collections.Generic;
using System.ServiceModel;

namespace Hangman_Server
{
    [ServiceContract]
    public interface IGameService
    {
        [OperationContract]
        string RechazarLetra(int salaId, string letra);
        [OperationContract]
        string CrearSala(string nombreJugador);

        [OperationContract]
        string UnirseSala(int salaId, string nombreJugador);

        [OperationContract]
        void Salir(int salaId, string nombreJugador);

        [OperationContract]
        bool EsPartidaTerminada(int salaId);
        [OperationContract]
        string ProbarConexion();

        [OperationContract]
        string ObtenerSalas();

        [OperationContract]
        string ObtenerEstadoPalabra(int salaId);

        [OperationContract]
        int ObtenerIntentosRestantes(int salaId);

        [OperationContract]
        string ObtenerLetraPropuesta(int salaId);

        [OperationContract]
        void EnviarLetra(int salaId, string letra);

        [OperationContract]
        string ConfirmarLetra(int salaId, string letra);


        [OperationContract]
        string AgregarJugador(int salaId, string nombreJugador);
        [OperationContract]
        int ObtenerJugadoresEnSala(int salaId);


    }
}