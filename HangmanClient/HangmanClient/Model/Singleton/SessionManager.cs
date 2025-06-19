using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace HangmanClient.Model.Singleton
{
    using GameServiceReference;
    using HangmanServiceReference;
    using PlayerDTO = HangmanServiceReference.PlayerDTO;

    public class SessionManager
    {
        private static SessionManager _instance;
        public static SessionManager Instance => _instance ??= new SessionManager();

        public PlayerDTO CurrentPlayer { get; set; }
        public int CurrentLanguage { get; set; } = 1;

        public Socket SocketCliente { get; set; }

        public GameServiceClient GameServiceClient { get; private set; }
        public HangmanServiceClient HangmanServiceClient { get; private set; }

        private SessionManager() { }

        public void InicializarGameServiceClient(GameServiceClient client)
        {
            GameServiceClient = client;
        }
        public void InicializarHangmanServiceClient(HangmanServiceClient client)
        {
            HangmanServiceClient = client;
        }
       
    }
}