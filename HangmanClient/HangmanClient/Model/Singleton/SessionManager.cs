using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangmanClient.Model.Singleton
{
    public class SessionManager
    {
        private static SessionManager _instance;
        public static SessionManager Instance => _instance ?? (_instance = new SessionManager());

        public GameServiceReference.GameServiceClient GameService { get; set; }
        public HangmanServiceReference.HangmanServiceClient HangmanService { get; set; }

        public HangmanServiceReference.PlayerDTO CurrentPlayer { get; set; }
        public string CurrentLanguage { get; set; } = "Español";

        private SessionManager() { }
    }
}
