using Hangman_Server.Model.DTO;
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
        string CrearSala(string nombreJugador, int idCliente);

        [OperationContract]
        string UnirseSala(int salaId, string nombreJugador, int idPlayerGuesser);

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

        [OperationContract]
        void RegistrarPartidaFinalizada(int idPlayerChallenger, bool palabraAdivinada);

        [OperationContract]
        void RegistrarPartidaInconclusa(int idPlayerChallenger, int idDisconnected);

        [OperationContract]
        int ObtenerIdGuesser(int salaId);

        [OperationContract]
        int ObtenerIdWord(int salaId);
        [OperationContract]
        List<string> ObtenerJugadoresEnPartida(int salaId);
        [OperationContract]
        List<WordDTO> ObtenerPalabrasPorCategoria(string categoria, int idLenguaje);

        [OperationContract]
        List<CategoryDTO> ObtenerCategorias(int idLenguaje);


    }
}