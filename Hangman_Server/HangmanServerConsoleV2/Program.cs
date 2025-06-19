using System;
using System.Threading;
using Hangman_Server;

namespace HangmanServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando servidor de sockets...");

            try
            {
                SocketServidor servidor = SocketServidor.ObtenerInstancia();
                servidor.Iniciar();

                Console.WriteLine("Servidor iniciado correctamente.");
                Console.WriteLine("Presiona ENTER para detener el servidor.");

                Console.ReadLine();

                Console.WriteLine("Deteniendo servidor...");
                servidor.Detener();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
            }
        }
    }
}