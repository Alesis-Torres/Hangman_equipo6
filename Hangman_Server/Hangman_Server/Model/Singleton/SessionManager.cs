using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Web;
using static Hangman_Server.HangmanService;

namespace Hangman_Server.Model.Singleton
{
    public class SessionManager
    {
        private static readonly Dictionary<string, SessionInfo> ActiveSessions = new Dictionary<string, SessionInfo>();
        private static readonly object _lock = new object();

        public static bool ValidateAndRegisterSession(string username, out Guid sessionId)
        {
            lock (_lock)
            {
                if (ActiveSessions.ContainsKey(username))
                {
                    sessionId = ActiveSessions[username].SessionId;
                    return false;
                }

                sessionId = Guid.NewGuid();
                ActiveSessions[username] = new SessionInfo
                {
                    Username = username,
                    LoginTime = DateTime.Now,
                    SessionId = sessionId
                };
                return true;
            }
        }


        public static void RemoveSession(string username)
        {
            lock (_lock)
            {
                if (ActiveSessions.ContainsKey(username))
                {
                    ActiveSessions.Remove(username);
                }

                if (SocketServidor.ClientesConectados.ContainsKey(username))
                {
                    try
                    {
                        var cliente = SocketServidor.ClientesConectados[username];
                        cliente.Socket.Shutdown(SocketShutdown.Both);
                        cliente.Socket.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cerrar socket del usuario {username}: {ex.Message}");
                    }
                    SocketServidor.ClientesConectados.Remove(username);
                }
            }
        }
    }
}