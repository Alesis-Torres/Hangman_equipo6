using Hangman_Server.Model;
using System.ServiceModel;

namespace Hangman_Server
{
    [ServiceContract]
    public interface IHangmanService
    {
        [OperationContract]
        PlayerDTO Login(string username, string password);

        [OperationContract]
        bool RegisterPlayer(PlayerDTO newPlayer);

        [OperationContract]
        bool UpdatePlayerProfile(PlayerDTO player);

        [OperationContract]
        PlayerStatsDTO GetPlayerStats(int playerId);
        [OperationContract]
        bool EmailExiste(string email);

        [OperationContract]
        bool PhoneNumberExiste(string phoneNumber);
        [OperationContract]
        bool UsernameExiste(string username);

        [OperationContract]
        bool NicknameExiste(string nickname);


    }
}