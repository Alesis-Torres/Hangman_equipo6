using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Hangman_Server
{
    public class SocketServidor
    {
        private static SocketServidor _instancia;
        private static readonly object _lockerInstancia = new object();

        private TcpListener servidor;
        private bool enEjecucion = false;
        private readonly object locker = new object();
        private readonly Dictionary<int, List<ClienteInfo>> salas = new Dictionary<int, List<ClienteInfo>>();
        private readonly Dictionary<int, string> palabrasPorSala = new Dictionary<int, string>();
        private readonly Dictionary<int, string> estadoPalabraPorSala = new Dictionary<int, string>();
        private readonly Dictionary<int, string> letraPropuestaPorSala = new Dictionary<int, string>();
        private readonly Dictionary<int, HashSet<string>> letrasAdivinadasPorSala = new Dictionary<int, HashSet<string>>();
        private readonly Dictionary<int, int> intentosFallidosPorSala = new Dictionary<int, int>();
        private int contadorSalas = 0;
        private static int ultimaSalaId = 0;
        private static Dictionary<int, bool> partidaTerminadaPorSala = new Dictionary<int, bool>();

        public static SocketServidor ObtenerInstancia()
        {
            lock (_lockerInstancia)
            {
                if (_instancia == null)
                {
                    _instancia = new SocketServidor();
                }
                return _instancia;
            }
        }

        private SocketServidor() { }

        public void Iniciar()
        {
            try
            {
                servidor = new TcpListener(IPAddress.Any, 1002);
                servidor.Start();
                enEjecucion = true;
                Console.WriteLine("Servidor Socket iniciado correctamente.");

                while (enEjecucion)
                {
                    var socketCliente = servidor.AcceptSocket();
                    Thread hilo = new Thread(() => AtenderCliente(socketCliente));
                    hilo.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
            }
        }
        public int ObtenerIntentosRestantes(int salaId)
        {
            lock (locker)
            {
                if (!intentosFallidosPorSala.ContainsKey(salaId))
                {
                    return -1; 
                }
                int intentosFallidos = intentosFallidosPorSala[salaId];
                return Math.Max(0, 5 - intentosFallidos);
            }
        }


        public void Salir(int salaId, string nombreJugador)
        {
            lock (locker)
            {
                if (salas.ContainsKey(salaId))
                {
                    var sala = salas[salaId];
                    sala.RemoveAll(c => c.Nombre.Equals(nombreJugador, StringComparison.OrdinalIgnoreCase));
                    if (sala.Count == 0)
                    {
                        salas.Remove(salaId);
                        palabrasPorSala.Remove(salaId);
                        estadoPalabraPorSala.Remove(salaId);
                        letrasAdivinadasPorSala.Remove(salaId);
                        intentosFallidosPorSala.Remove(salaId);
                        letraPropuestaPorSala.Remove(salaId);
                    }
                }
            }
        }

        public void EnviarLetra(int salaId, string letra)
        {
            lock (locker)
            {
                if (!letraPropuestaPorSala.ContainsKey(salaId))
                {
                    letraPropuestaPorSala[salaId] = letra;
                }
                else
                {
                    letraPropuestaPorSala[salaId] = letra;
                }
            }
        }

        public string ConfirmarLetra(int salaId, string entrada)
        {
            lock (locker)
            {
                if (!palabrasPorSala.ContainsKey(salaId))
                {
                    palabrasPorSala[salaId] = entrada.ToUpper();
                    estadoPalabraPorSala[salaId] = new string('_', entrada.Length);
                    letrasAdivinadasPorSala[salaId] = new HashSet<string>();
                    intentosFallidosPorSala[salaId] = 0;
                    letraPropuestaPorSala[salaId] = "";
                    return "PALABRA_ESTABLECIDA";
                }
                else if (letraPropuestaPorSala.ContainsKey(salaId) && !string.IsNullOrEmpty(letraPropuestaPorSala[salaId]))
                {
                    string palabraCorrecta = palabrasPorSala[salaId];
                    string letraPropuesta = letraPropuestaPorSala[salaId];

                    if (!palabraCorrecta.ToUpper().Contains(letraPropuesta.ToUpper()))
                    {
                        Console.WriteLine("No es cierto, esa letra no se encuentra en la palabra!");

                        intentosFallidosPorSala[salaId]++;
                        letraPropuestaPorSala[salaId] = "";
                        VerificarFinDePartida(salaId);
                        return "TRAMPA_CONFIRMAR";
                    }

                    char[] estado = estadoPalabraPorSala[salaId].ToCharArray();
                    for (int i = 0; i < palabraCorrecta.Length; i++)
                    {
                        if (palabraCorrecta[i].ToString().Equals(letraPropuesta, StringComparison.OrdinalIgnoreCase))
                        {
                            estado[i] = palabraCorrecta[i];
                        }
                    }
                    estadoPalabraPorSala[salaId] = new string(estado);

                    letraPropuestaPorSala[salaId] = "";
                    VerificarFinDePartida(salaId);
                    return "CONFIRMADA";
                }
                return "ERROR";
            }
        }

        private bool VerificarFinDePartida(int salaId)
        {
            string estado = estadoPalabraPorSala[salaId];
            int intentos = intentosFallidosPorSala[salaId];
            string palabra = palabrasPorSala[salaId];

            if (!estado.Contains('_'))
            {
                Console.WriteLine($"¡El guesser adivinó la palabra '{palabra}'!");
                partidaTerminadaPorSala[salaId] = true;
                LimpiarSala(salaId);
                return true;
            }

            if (intentos >= 6)
            {
                Console.WriteLine($"¡El guesser perdió! La palabra era '{palabra}'.");
                partidaTerminadaPorSala[salaId] = true;
                LimpiarSala(salaId);
                return true;
            }

            return false;
        }
        public bool EsPartidaTerminada(int salaId)
        {
            lock (locker)
            {
                return partidaTerminadaPorSala.ContainsKey(salaId) && partidaTerminadaPorSala[salaId];
            }
        }
        private void LimpiarSala(int salaId)
        {
            palabrasPorSala.Remove(salaId);
            estadoPalabraPorSala.Remove(salaId);
            letrasAdivinadasPorSala.Remove(salaId);
            intentosFallidosPorSala.Remove(salaId);
            letraPropuestaPorSala.Remove(salaId);
            salas.Remove(salaId);
        }

        public string RechazarLetra(int salaId, string letra)
        {
            lock (locker)
            {
                if (palabrasPorSala.ContainsKey(salaId) && !string.IsNullOrEmpty(letraPropuestaPorSala[salaId]))
                {
                    string palabraCorrecta = palabrasPorSala[salaId];
                    string letraPropuesta = letraPropuestaPorSala[salaId];

                    if (palabraCorrecta.ToUpper().Contains(letraPropuesta.ToUpper()))
                    {
                        Console.WriteLine("¡No seas tramposo, esa letra sí está!");

                        char[] estado = estadoPalabraPorSala[salaId].ToCharArray();
                        for (int i = 0; i < palabraCorrecta.Length; i++)
                        {
                            if (palabraCorrecta[i].ToString().Equals(letraPropuesta, StringComparison.OrdinalIgnoreCase))
                            {
                                estado[i] = palabraCorrecta[i];
                            }
                        }
                        estadoPalabraPorSala[salaId] = new string(estado);

                        letraPropuestaPorSala[salaId] = "";
                        VerificarFinDePartida(salaId);

                        return "TRAMPA";
                    }
                    else
                    {
                        intentosFallidosPorSala[salaId]++;
                    }

                    letraPropuestaPorSala[salaId] = "";
                    VerificarFinDePartida(salaId);
                    return "RECHAZADA";
                }
                return "ERROR";
            }
        }
        public string ObtenerLetraPropuesta(int salaId)
        {
            lock (locker)
            {
                if (!letraPropuestaPorSala.ContainsKey(salaId))
                {
                    return string.Empty; // O un mensaje de error si prefieres
                }

                return letraPropuestaPorSala[salaId];
            }
        }
        public string ObtenerEstadoPalabra(int salaId)
        {
            lock (locker)
            {
                if (!estadoPalabraPorSala.ContainsKey(salaId))
                {
                    return "ERROR: Sala no existe o no se ha seleccionado palabra.";
                }

                return estadoPalabraPorSala[salaId];
            }
        }
        public int ObtenerJugadoresEnSala(int salaId)
        {
            lock (locker)
            {
                if (salas.ContainsKey(salaId))
                {
                    return salas[salaId]
                        .GroupBy(c => c.Nombre)
                        .Count();
                }
                return 0;
            }
        }

        public int CrearSala(string nombreJugador)
        {
            lock (locker)
            {
                int nuevaSalaId = ++ultimaSalaId;
                salas[nuevaSalaId] = new List<ClienteInfo>();

                salas[nuevaSalaId].Add(new ClienteInfo
                {
                    Nombre = nombreJugador,
                    Rol = "challenger"
                });

                Console.WriteLine($"Sala {nuevaSalaId} creada por {nombreJugador}.");
                return nuevaSalaId;
            }
        }
        public string UnirseSala(int salaId, string nombreJugador)
        {
            lock (locker)
            {
                if (!salas.ContainsKey(salaId))
                {
                    return "ERROR: Sala no existe.";
                }

                var jugadores = salas[salaId];
                if (jugadores.Any(c => c.Nombre.Equals(nombreJugador, StringComparison.OrdinalIgnoreCase)))
                {
                    return "ERROR: Jugador ya unido a esta sala.";
                }

                string rol = jugadores.Count == 0 ? "challenger" : "guesser";

                jugadores.Add(new ClienteInfo
                {
                    Nombre = nombreJugador,
                    Rol = rol
                });

                Console.WriteLine($"Jugador {nombreJugador} unido a sala {salaId} como {rol}.");
                return $"ROLE:{rol}";
            }
        }
        public string AgregarJugador(int salaId, string nombreJugador)
        {
            if (!salas.ContainsKey(salaId))
            {
                salas[salaId] = new List<ClienteInfo>();
            }

            string rol = salas[salaId].Count == 0 ? "challenger" : "guesser";

            var cliente = new ClienteInfo
            {
                Nombre = nombreJugador,
                Rol = rol
            };

            salas[salaId].Add(cliente);

            return rol;
        }

        public void EliminarJugador(int salaId, string nombreJugador)
        {
            if (salas.ContainsKey(salaId))
            {
                salas[salaId].RemoveAll(c => c.Nombre == nombreJugador);

                if (salas[salaId].Count == 0)
                {
                    salas.Remove(salaId);
                }
            }
        }

        public void Detener()
        {
            try
            {
                enEjecucion = false;

                if (servidor != null)
                {
                    servidor.Stop();
                    servidor = null;
                }

                lock (locker)
                {
                    foreach (var sala in salas)
                    {
                        foreach (var cliente in sala.Value)
                        {
                            try
                            {
                                cliente.Socket.Shutdown(SocketShutdown.Both);
                            }
                            catch
                            {

                            }
                            finally
                            {
                                cliente.Socket.Close();
                            }
                        }
                    }

                    salas.Clear();
                    palabrasPorSala.Clear();
                    estadoPalabraPorSala.Clear();
                    letrasAdivinadasPorSala.Clear();
                    intentosFallidosPorSala.Clear();
                }

                Console.WriteLine("Servidor detenido correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al detener el servidor: {ex.Message}");
            }
        }

        private void AtenderCliente(Socket socketCliente)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesLeidos = socketCliente.Receive(buffer);
                string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();

                if (mensaje == "GET_SALAS")
                {
                    string respuesta = ObtenerSalas();
                    socketCliente.Send(Encoding.UTF8.GetBytes(respuesta));
                }
                else if (mensaje == "CREAR_SALA")
                {
                    int idSala;
                    lock (locker)
                    {
                        idSala = contadorSalas++;
                        salas[idSala] = new List<ClienteInfo>();
                    }

                    salas[idSala].Add(new ClienteInfo { Socket = socketCliente, Rol = "challenger" });
                    socketCliente.Send(Encoding.UTF8.GetBytes($"SALA:{idSala} ROLE:challenger\n"));
                    AtenderClienteEnSala(idSala, socketCliente, "challenger");
                }
                else if (int.TryParse(mensaje, out int salaId))
                {
                    lock (locker)
                    {
                        if (salas.ContainsKey(salaId))
                        {
                            salas[salaId].Add(new ClienteInfo { Socket = socketCliente, Rol = "guesser" });
                            socketCliente.Send(Encoding.UTF8.GetBytes($"SALA:{salaId} ROLE:guesser\n"));
                            AtenderClienteEnSala(salaId, socketCliente, "guesser");
                        }
                        else
                        {
                            socketCliente.Send(Encoding.UTF8.GetBytes("ERROR: Sala no encontrada.\n"));
                        }
                    }
                }
                else
                {
                    socketCliente.Send(Encoding.UTF8.GetBytes("ERROR: Comando no reconocido.\n"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en AtenderCliente: {ex.Message}");
            }
        }

        public string ObtenerSalas()
        {
            StringBuilder sb = new StringBuilder();

            lock (salas)
            {
                foreach (var sala in salas)
                {
                    int idSala = sala.Key;
                    int jugadores = sala.Value
                    .GroupBy(c => c.Nombre, StringComparer.OrdinalIgnoreCase)
                    .Count();
                    sb.AppendLine($"Sala {idSala}: {jugadores} jugador(es) conectado(s)");
                }
            }

            return sb.ToString();
        }

        public void AtenderClienteEnSala(int salaId, Socket socketCliente, string rol)
        {
            try
            {
                if (rol == "challenger")
                {
                    socketCliente.Send(Encoding.UTF8.GetBytes("Elige una palabra para adivinar:\n"));
                    byte[] buffer = new byte[1024];
                    int bytesLeidos = socketCliente.Receive(buffer);
                    string palabra = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();

                    lock (locker)
                    {
                        palabrasPorSala[salaId] = palabra;
                        estadoPalabraPorSala[salaId] = new string('_', palabra.Length);
                        letrasAdivinadasPorSala[salaId] = new HashSet<string>();
                        intentosFallidosPorSala[salaId] = 0;
                    }

                    socketCliente.Send(Encoding.UTF8.GetBytes("Palabra establecida correctamente.\n"));
                }

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesLeidos = socketCliente.Receive(buffer);
                    if (bytesLeidos == 0)
                        break;

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();

                    if (mensaje.Equals("SALIR", StringComparison.OrdinalIgnoreCase))
                        break;

                    if (rol == "guesser" && mensaje.Length == 1)
                    {
                        string letra = mensaje;
                        bool acierto = false;

                        lock (locker)
                        {
                            if (!letrasAdivinadasPorSala[salaId].Contains(letra))
                            {
                                letrasAdivinadasPorSala[salaId].Add(letra);
                                char[] estado = estadoPalabraPorSala[salaId].ToCharArray();
                                string palabra = palabrasPorSala[salaId];

                                for (int i = 0; i < palabra.Length; i++)
                                {
                                    if (palabra.ToLower() == letra.ToLower())
                                    {
                                        estado[i] = palabra[i];
                                        acierto = true;
                                    }
                                }

                                estadoPalabraPorSala[salaId] = new string(estado);

                                if (!acierto)
                                {
                                    intentosFallidosPorSala[salaId]++;
                                }
                            }
                        }

                        string estadoActual;
                        lock (locker)
                        {
                            estadoActual = estadoPalabraPorSala[salaId];
                        }

                        string respuesta = acierto
                            ? $"ACIERTO:{estadoActual}"
                            : $"ERROR:{estadoActual}|INTENTOS:{intentosFallidosPorSala[salaId]}";

                        socketCliente.Send(Encoding.UTF8.GetBytes(respuesta + "\n"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la sala {salaId}: {ex.Message}");
            }
            finally
            {
                lock (locker)
                {
                    if (salas.ContainsKey(salaId))
                    {
                        salas[salaId].RemoveAll(c => c.Socket == socketCliente);
                        if (salas[salaId].Count == 0)
                        {
                            salas.Remove(salaId);
                            palabrasPorSala.Remove(salaId);
                            estadoPalabraPorSala.Remove(salaId);
                            letrasAdivinadasPorSala.Remove(salaId);
                            intentosFallidosPorSala.Remove(salaId);
                        }
                    }
                }
                socketCliente?.Close();
            }
        }
    }

    public class ClienteInfo
    {
        public Socket Socket { get; set; }
        public string Identificador { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
    }
}
