using Hangman_Server.Model.DTO;
using HangmanClient.Model.DTO;
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
        int CrearSala(string nombreJugador, int idCliente);
        [OperationContract]
        int CrearSalaV2(string nombreJugador, int idCliente);
        [OperationContract]
        int ObtenerSalaIdPorCodigo(string codigo);
        [OperationContract]
        string UnirseSala(int salaId, string nombreJugador, int idPlayerGuesser);

        [OperationContract]
        void Salir(int salaId, string nombreJugador);

        [OperationContract]
        string ObtenerPalabraPorSala(int salaId);

        [OperationContract]
        string ObtenerCodigoDeSala(int salaId); 
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
        void RegistrarPartidaInconclusa(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        [OperationContract]
        int RegistrarPartidaFinalizada(int idChallenger, int idGuesser, int idPalabra, int idEstado, int idJugadorGanador);
        [OperationContract]
        int EstablecerPalabra(int idSala, string palabra);

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