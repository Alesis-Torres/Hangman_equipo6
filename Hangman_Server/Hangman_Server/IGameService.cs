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
        string ProbarConexion();
        [OperationContract]
        void RegistrarPartidaInconclusa(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);

        [OperationContract]
        void RegistrarPartidaFinalizada(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        [OperationContract]

        List<WordDTO> ObtenerPalabrasPorCategoria(string categoria, int idLenguaje);

        [OperationContract]
        List<CategoryDTO> ObtenerCategorias(int idLenguaje);

    }
}