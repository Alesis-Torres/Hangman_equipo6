using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows;

using HangmanServerLibrary.Model;
using System.Windows.Threading;
using System.Diagnostics;
using HangmanServerLibrary.GameServiceReference;


namespace Hangman_Server
{
    public class SocketServidor
    {
        
        private static SocketServidor _instancia;
        private static readonly object _lockerInstancia = new object();
        public static Dictionary<int, JugadorConectado> JugadoresConectados = new Dictionary<int, JugadorConectado>();
        public static Dictionary<int, SalaJuego> salasActivas = new Dictionary<int, SalaJuego>();
        private static readonly TimeSpan tiempoLimiteInactividad = TimeSpan.FromSeconds(40);
        private readonly object locker = new object();
        private static readonly Random random = new Random();
        private TcpListener servidor;
        private bool enEjecucion = false;


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
                new Thread(() => VerificarDesconectadosSocket())
                {
                    IsBackground = true
                }.Start();
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
        private bool SocketActivo(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch
            {
                return false;
            }
        }
        private void VerificarDesconectadosSocket()
        {
            while (true)
            {
                Thread.Sleep(5000);
                var desconectados = new List<JugadorConectado>();
                
                lock (locker)
                {
                    foreach (var jugador in JugadoresConectados.Values.ToList())
                    {
                        if (!SocketActivo(jugador.Socket))
                        {
                            desconectados.Add(jugador);
                        }
                    }

                    foreach (var jugador in desconectados)
                    {
                        int salaId = ObtenerSalaIdPorJugador(jugador.IdPlayer);
                        bool otroJugadorActivo = false;

                        if (salaId != -1 && salasActivas.TryGetValue(salaId, out var sala))
                        {
                            otroJugadorActivo = sala.Clientes.Any(j =>
                                j.IdPlayer != jugador.IdPlayer &&
                                SocketActivo(j.Socket));
                        }

                        if (salaId == -1 || !otroJugadorActivo)
                        {

                            Console.WriteLine($"Desconectado {jugador.Nickname}, pero no se registra (sala: {salaId}, otro activo: {otroJugadorActivo})");
                        }
                        else
                        {
                            Console.WriteLine($" Desconectado {jugador.Nickname}, registrando partida inconclusa");
                            NotificarDesconexionEnPartida(jugador.IdPlayer);
                        }

                        JugadoresConectados.Remove(jugador.IdPlayer);
                    }

                    EliminarSalasVacias();
                }

                MonitorearSalas();
                MonitorearClientes();
            }
        }

        private void EliminarSalasVacias()
        {
            lock (locker)
            {
                var salasVacias = salasActivas
                    .Where(s => s.Value.Clientes.Count == 0)
                    .Select(s => s.Key)
                    .ToList();

                foreach (int idSala in salasVacias)
                {
                    salasActivas.Remove(idSala);
                    Console.WriteLine($"[Limpieza] Sala {idSala} eliminada por estar vacía.");
                }
            }
        }

        private void MonitorearSalas()
        {
            Console.WriteLine();
            Console.WriteLine("[Monitor Servidor] Salas activas:");

            var salasConClientes = salasActivas.Values.Where(s => s.Clientes.Any());

            if (!salasConClientes.Any())
            {
                Console.WriteLine(" - (Sin salas activas)");
                return;
            }

            foreach (var sala in salasConClientes)
            {
                Console.WriteLine($" - Sala {sala.Id} ({sala.Clientes.Count} jugadores):");

                foreach (var jugador in sala.Clientes)
                {
                    string conectado = (jugador.Socket != null && jugador.Socket.Connected) ? "Conectado" : "Desconectado";
                    Console.WriteLine($"    {jugador.Nombre} [{jugador.RolActual}] - {conectado} (IdPlayer={jugador.IdPlayer})");
                }
            }
        }

        private void MonitorearClientes()
        {
            Console.WriteLine();
            Console.WriteLine("[Monitor Servidor] Clientes activos:");

            var ClientesActivos = JugadoresConectados.Values.Where(s => s.Nickname.Any());

            if (!ClientesActivos.Any())
            {
                Console.WriteLine(" - (Sin clientes activos)");
                return;
            }

            foreach (var cliente in ClientesActivos)
            {
                Console.WriteLine($" - Cliente {cliente.Nickname} {cliente.IdPlayer}  Conectado");

            }
        }




        
        public void LimpiarSala(int idSala)
        {
            lock (locker)
            {
                if (salasActivas.ContainsKey(idSala))
                {
                    salasActivas.Remove(idSala);
                    int nuevaSalaId = 20;
                    while (salasActivas.ContainsKey(nuevaSalaId))
                    {
                        nuevaSalaId--;
                    }
                    Console.WriteLine($"✅ Sala {idSala} completamente limpiada.");
                }
            }
        }


        private string ConfirmarLetra(SalaJuego sala, string letra)
        {
;
            sala.TurnoActual = "GUESSER";
            if (sala.Palabra.IndexOf(letra, StringComparison.OrdinalIgnoreCase) < 0)
            {
                sala.IntentosFallidos++;
                sala.AccionResultado = "TRAMPA_CONFIRMAR";
                sala.LetraPropuesta = letra;
                return "TRAMPA_CONFIRMAR";
            }

            char[] estado = sala.EstadoPalabra.ToCharArray();
            for (int i = 0; i < sala.Palabra.Length; i++)
            {
                if (sala.Palabra[i].ToString().Equals(letra, StringComparison.OrdinalIgnoreCase))
                {
                    estado[i] = sala.Palabra[i];
                }
            }
            Console.WriteLine(estado);
            sala.EstadoPalabra = new string(estado);
            sala.AccionResultado = "ACEPTADA";
            sala.LetraPropuesta = letra;

            return "ACEPTADA";
        }

        

        public bool EsPartidaTerminada(int salaId)
        {
            lock (locker)
            {
                return salasActivas.ContainsKey(salaId) && salasActivas[salaId].Terminada;
            }
        }


        private string GenerarCodigoUnico()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string codigo;

            do
            {
                codigo = new string(Enumerable.Range(0, 6)
                    .Select(_ => caracteres[random.Next(caracteres.Length)]).ToArray());
            }
            while (salasActivas.Values.Any(s => s.CodigoUnico.Equals(codigo, StringComparison.OrdinalIgnoreCase)));

            return codigo;
        }
        public string UnirseSala(int salaId, string nombreJugador, int idPlayerGuesser)
        {
            lock (locker)
            {
                if (!salasActivas.TryGetValue(salaId, out var sala))
                {
                    return "ERROR: Sala no existe.";
                }

                if (sala.Clientes.Any(c => c.Nombre.Equals(nombreJugador, StringComparison.OrdinalIgnoreCase)))
                {
                    return "ERROR: Jugador ya unido a esta sala.";
                }
                if (sala.Clientes.Count >= 2)
                {
                    return "ERROR: La sala ya tiene dos jugadores.";
                }
                string rol = sala.Clientes.Count == 0 ? "challenger" : "guesser";

                if (!JugadoresConectados.TryGetValue(idPlayerGuesser, out var jugador))
                {
                    return "ERROR: Jugador no está conectado.";
                }

                jugador.RolActual = rol;
                sala.Clientes.Add(jugador);

                if (rol == "guesser")
                {
                    sala.idGuesser = idPlayerGuesser;

                    foreach (var cliente in sala.Clientes)
                    {
                        try
                        {
                            string notificacion = $"INICIAR_PARTIDA|{salaId}";
                            cliente.Socket?.Send(Encoding.UTF8.GetBytes(notificacion));
                            Console.WriteLine($" INICIAR_PARTIDA enviado a {cliente.Nombre} (sala {salaId})");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($" Error al notificar INICIAR_PARTIDA a {cliente.Nombre}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    sala.idChallenger = idPlayerGuesser;
                }

                Console.WriteLine($"Jugador {nombreJugador} unido a sala {salaId} como {rol}.");
                return $"ROLE:{rol}";
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
                    foreach (var sala in salasActivas.Values)
                    {
                        foreach (var cliente in sala.Clientes)
                        {
                            try
                            {
                                cliente.Socket?.Shutdown(SocketShutdown.Both);
                            }
                            catch
                            {}
                            finally
                            {
                                cliente.Socket?.Close();
                            }
                        }
                    }

                    salasActivas.Clear();
                    JugadoresConectados.Clear();
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
            if (!SocketActivo(socketCliente))
            {
                Console.WriteLine("❌ El socket ya estaba inactivo al intentar atenderlo.");
                try { socketCliente?.Shutdown(SocketShutdown.Both); } catch { }
                try { socketCliente?.Close(); } catch { }
                return;
            }
            bool notificado = false;
            int idJugadorActual = -1;
            int salaId = -1;
            bool continuar = true;

            try
            {
                socketCliente.ReceiveTimeout = 20000;

                while (continuar)
                {
                    byte[] buffer = new byte[1024];
                    int bytesLeidos = socketCliente.Receive(buffer);

                    if (bytesLeidos == 0)
                    {
                        Console.WriteLine("[Socket cerrado por el cliente]");

                        if (idJugadorActual != -1 && salaId != -1)
                        {
                            NotificarSalidaJugador(idJugadorActual, salaId, false);
                            notificado = true;
                        }

                        break;
                    }

                    string mensaje = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();
                    Console.WriteLine($"[Servidor] Mensaje recibido: {mensaje}");
                    string[] partes = mensaje.Split('|');
                    string comando = partes[0];
                    Console.WriteLine("COMANDO INGRESADO");
                    switch (comando)
                    {
                        case "LOGIN":
                            if (partes.Length >= 3)
                            {
                                string idStr = partes[1].Trim();
                                string nickname = partes[2].Trim();

                                if (!int.TryParse(idStr, out int idJugador))
                                {
                                    socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|ID_INVÁLIDO\n"));
                                    break;
                                }

                                lock (locker)
                                {
                                    if (JugadoresConectados.ContainsKey(idJugador))
                                    {
                                        socketCliente.Send(Encoding.UTF8.GetBytes("DUPLICADO\n"));
                                        break;
                                    }

                                    JugadoresConectados[idJugador] = new JugadorConectado
                                    {
                                        IdPlayer = idJugador,
                                        Nickname = nickname,
                                        Socket = socketCliente,
                                        UltimoPing = DateTime.UtcNow
                                    };

                                    socketCliente.Send(Encoding.UTF8.GetBytes("LOGIN_OK\n"));
                                }
                                idJugadorActual = idJugador;
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|FORMATO_LOGIN\n"));
                            }
                            break;
                        case "LOGOUT":
                            if (partes.Length >= 2)
                            {
                                string idStr = partes[1].Trim();

                                if (!int.TryParse(idStr, out int idJugador))
                                {
                                    socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|ID_INVÁLIDO\n"));
                                    break;
                                }

                                lock (locker)
                                {
                                    if (JugadoresConectados.ContainsKey(idJugador))
                                    {
                                        JugadoresConectados.Remove(idJugador);
                                        socketCliente.Send(Encoding.UTF8.GetBytes("LOGOUT_OK\n"));
                                    }
                                    else
                                    {
                                        socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|NO_CONECTADO\n"));
                                    }
                                }
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|FORMATO_LOGOUT\n"));
                            }
                            break;

                        case "CREAR_SALA":
                            if (partes.Length >= 4 &&
                                int.TryParse(partes[1], out int idJugadorCrear) &&
                                int.TryParse(partes[3], out int idioma))
                            {
                                string nickname = partes[2];

                                salaId = CrearSala(idJugadorCrear, nickname, idioma, socketCliente);
                                idJugadorActual = idJugadorCrear;
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR|Datos inválidos al crear sala\n"));
                            }
                            break;

                        case "OBTENER_SALAS":
                            if (partes.Length >= 2 && int.TryParse(partes[1], out int idiomaSalas))
                            {
                                string resultado = ObtenerSalas(idiomaSalas);
                                socketCliente.Send(Encoding.UTF8.GetBytes("SALAS|" + resultado));
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("SALAS|Parámetro de idioma inválido.\n"));
                            }
                            break;

                        case "MONITOR":
                            if (partes.Length >= 2)
                            {
                                string nickname = partes[1].Trim();

                                lock (locker)
                                {
                                    var jugador = JugadoresConectados.Values
                                        .FirstOrDefault(j => j.Nickname == nickname);

                                    if (jugador != null)
                                    {
                                        try
                                        {
                                            jugador.Socket?.Shutdown(SocketShutdown.Both);
                                            jugador.Socket?.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[Monitor] Error cerrando socket duplicado de {nickname}: {ex.Message}");
                                        }

                                        jugador.Socket = socketCliente;
                                        jugador.UltimoPing = DateTime.UtcNow;
                                        idJugadorActual = jugador.IdPlayer;

                                        Console.WriteLine($"[Monitor] Sesión monitor activa para {nickname}");
                                    }
                                }
                            }
                            break;

                        case "NICKNAME":
                            if (partes.Length >= 4 &&
                                int.TryParse(partes[3], out int idNick))
                            {
                                string username = partes[1].Trim();
                                string nickname = partes[2].Trim();

                                lock (locker)
                                {
                                    if (JugadoresConectados.ContainsKey(idNick))
                                    {
                                        JugadoresConectados[idNick].Nombre = username;
                                        JugadoresConectados[idNick].Nickname = nickname;
                                    }
                                }
                                idJugadorActual = idNick;
                            }
                            break;

                        case "CONFIRMAR_LETRA":
                            if (partes.Length >= 3 && int.TryParse(partes[2], out int idSalaConfirmar))
                            {
                                string letra = partes[1].Trim();
                                lock (locker)
                                {
                                    if (!salasActivas.ContainsKey(idSalaConfirmar))
                                        break;

                                    var sala = salasActivas[idSalaConfirmar];
                                    salaId = idSalaConfirmar;

                                    var jugador = sala.Clientes.FirstOrDefault(c => c.Socket == socketCliente);
                                    if (jugador != null)
                                        idJugadorActual = jugador.IdPlayer;

                                    string resultado = ConfirmarLetra(sala, letra);
                                    Console.WriteLine($"[CONFIRMAR_LETRA] Resultado: {resultado}");

                                    foreach (var cliente in sala.Clientes)
                                    {
                                        ActualizarPartida(sala, cliente.Socket, true);
                                    }
                                }
                            }
                            break;

                        case "RECHAZAR_LETRA":
                            if (partes.Length >= 3 && int.TryParse(partes[2], out int idSalaRechazo))
                            {
                                string letra = partes[1].Trim();
                                lock (locker)
                                {
                                    if (!salasActivas.ContainsKey(idSalaRechazo))
                                        break;

                                    var sala = salasActivas[idSalaRechazo];
                                    salaId = idSalaRechazo;

                                    var jugador = sala.Clientes.FirstOrDefault(c => c.Socket == socketCliente);
                                    if (jugador != null)
                                        idJugadorActual = jugador.IdPlayer;

                                    string resultado = ConfirmarRechazoLetra(sala, letra);
                                    Console.WriteLine($"[RECHAZAR_LETRA] Resultado: {resultado}");

                                    foreach (var cliente in sala.Clientes)
                                    {
                                        ActualizarPartida(sala, cliente.Socket, true);
                                    }
                                }
                            }
                            break;
                        case "LETRA":
                            if (partes.Length >= 3 && int.TryParse(partes[2], out int idSalaLetra))
                            {
                                string letra = partes[1].Trim();

                                lock (locker)
                                {
                                    if (salasActivas.TryGetValue(idSalaLetra, out var salaLocal))
                                    {
                                        salaLocal.LetraPropuesta = letra;
                                        salaLocal.AccionResultado = "";
                                        salaLocal.TurnoActual = "CHALLENGER";

                                        salaId = idSalaLetra;

                                        var jugador = salaLocal.Clientes.FirstOrDefault(c => c.Socket == socketCliente);
                                        if (jugador != null)
                                            idJugadorActual = jugador.IdPlayer;

                                        foreach (var cliente in salaLocal.Clientes)
                                        {
                                            ActualizarPartida(salaLocal, cliente.Socket, true);
                                        }
                                    }
                                }
                            }
                            break;

                        case "UNIRSE_SALA":
                            if (partes.Length >= 4 &&
                                int.TryParse(partes[1], out int idSala) &&
                                int.TryParse(partes[3], out int idPlayer))
                            {
                                string nickname = partes[2];

                                string resultado = UnirseSala(idSala, nickname, idPlayer);

                                lock (locker)
                                {
                                    if (salasActivas.TryGetValue(idSala, out var salaLocal))
                                    {
                                        if (!salaLocal.Clientes.Any(c => c.IdPlayer == idPlayer))
                                        {
                                            var jugador = new JugadorConectado
                                            {
                                                IdPlayer = idPlayer,
                                                Nickname = nickname,
                                                Nombre = nickname,
                                                RolActual = "guesser",
                                                Socket = socketCliente,
                                                UltimoPing = DateTime.UtcNow
                                            };

                                            salaLocal.Clientes.Add(jugador);
                                            salaLocal.idGuesser = idPlayer;
                                            JugadoresConectados[idPlayer] = jugador;
                                        }
                                        VerificarInicioDePartida(salaLocal);
                                        ActualizarPartida(salaLocal, socketCliente, false);

                                    }
                                }
                                socketCliente.Send(Encoding.UTF8.GetBytes(resultado));
                                MostrarEstadoSala(idSala);
                            }
                            break;

                        case "PALABRA":
                            if (partes.Length >= 5 &&
                                int.TryParse(partes[2], out int idSalaPalabra) &&
                                int.TryParse(partes[3], out int idPalabra))
                            {
                                string palabra = partes[1].Trim();
                                string hint = partes[4].Trim();


                                if (hint.Length > 40)
                                {
                                    socketCliente.Send(Encoding.UTF8.GetBytes("ERROR_HINT_LARGO\n"));
                                    break;
                                }

                                int resultado = EstablecerPalabra(idSalaPalabra, palabra, idPalabra, hint);

                                if (salasActivas.TryGetValue(idSalaPalabra, out SalaJuego sala))
                                {
                                    sala.idPalabra = idPalabra;
                                    sala.Hint = hint; 

                                    Console.WriteLine($"PALABRA ESTABLECIDA - Sala: {sala.Id}, Pista: {hint}");
                                    VerificarInicioDePartida(sala);
                                    ActualizarPartida(sala, socketCliente, false);
                                }

                                string respuesta;
                                switch (resultado)
                                {
                                    case 1:
                                        respuesta = "PALABRA_ESTABLECIDA\n";
                                        break;
                                    case -2:
                                        respuesta = "YA_HAY_PALABRA\n";
                                        break;
                                    default:
                                        respuesta = "ERROR_PALABRA\n";
                                        break;
                                }

                                socketCliente.Send(Encoding.UTF8.GetBytes(respuesta));
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR_FORMATO_PALABRA\n"));
                            }
                            break;
                        case "UNIRSE_CODIGO":
                            if (partes.Length >= 4)
                            {
                                string codigo = partes[1].Trim();
                                string nickname = partes[2];
                                if (!int.TryParse(partes[3], out int idGuesser))
                                {
                                    socketCliente.Send(Encoding.UTF8.GetBytes("ERROR: ID inválido\n"));
                                    break;
                                }

                                lock (locker)
                                {
                                    var salaLocal = salasActivas.Values.FirstOrDefault(s => s.CodigoUnico.Equals(codigo, StringComparison.OrdinalIgnoreCase));
                                    if (salaLocal == null)
                                    {
                                        socketCliente.Send(Encoding.UTF8.GetBytes("ERROR: Código de sala no encontrado\n"));
                                        break;
                                    }

                                    var jugador = new JugadorConectado
                                    {
                                        IdPlayer = idGuesser,
                                        Nickname = nickname,
                                        Nombre = nickname,
                                        RolActual = "guesser",
                                        Socket = socketCliente,
                                        UltimoPing = DateTime.UtcNow
                                    };

                                    JugadoresConectados[idGuesser] = jugador;

                                    if (!salaLocal.Clientes.Any(c => c.IdPlayer == idGuesser))
                                    {
                                        salaLocal.Clientes.Add(jugador);
                                        salaLocal.idGuesser = idGuesser;
                                    }

                                    salaId = salaLocal.Id;
                                    idJugadorActual = idGuesser;

                                    socketCliente.Send(Encoding.UTF8.GetBytes($"UNIDO|{salaLocal.Id}\n"));
                                }
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR: Formato UNIRSE_CODIGO inválido\n"));
                            }
                            break;
                        case "SOLICITAR_ESTADO":
                            if (partes.Length >= 2 && int.TryParse(partes[1], out int idSalaEstado))
                            {
                                lock (locker)
                                {
                                    if (salasActivas.TryGetValue(idSalaEstado, out var salaEstado))
                                    {
                                        VerificarInicioDePartida(salaEstado);
                                        ActualizarPartida(salaEstado, socketCliente, false);
                                    }
                                }
                            }
                            break;
                        case "SALIR":
                            if (partes.Length >= 3 &&
                                int.TryParse(partes[1], out int idJugadorSalir) &&
                                int.TryParse(partes[2], out int idSalaSalir))
                            {
                                NotificarSalidaJugador(idJugadorSalir, idSalaSalir, true);
                                socketCliente.Send(Encoding.UTF8.GetBytes("SALIDA_CONFIRMADA\n"));
                                continuar = false;
                            }
                            else
                            {
                                socketCliente.Send(Encoding.UTF8.GetBytes("ERROR_FORMATO_SALIR\n"));
                                Console.WriteLine("Formato inválido en mensaje SALIR.");
                            }
                            break;

                        default:
                            Console.WriteLine($"[Advertencia] Comando no reconocido: {comando}");
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en AtenderCliente: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                if (!notificado && idJugadorActual != -1 && salaId != -1)
                {
                    NotificarSalidaJugador(idJugadorActual, salaId, false);
                }
                try
                {
                    socketCliente?.Shutdown(SocketShutdown.Both);
                }
                catch { }
                try
                {
                    socketCliente?.Close();
                }
                catch { }
            }
        }

        private string ObtenerSalas(int idioma)
        {
            lock (locker)
            {
                var sb = new StringBuilder();


                var salasFiltradas = salasActivas.Values
                    .Where(s => s.Idioma == idioma)
                    .ToList();

                foreach (var sala in salasFiltradas)
                {
                    string estadoTraducido = sala.Estado;

                    if (idioma == 1)
                    {
                        switch (sala.Estado)
                        {
                            case "INICIADA":
                                estadoTraducido = "En juego";
                                break;
                            case "EN_ESPERA":
                                estadoTraducido = "Esperando jugador";
                                break;
                            case "TERMINADA":
                                estadoTraducido = "Finalizada";
                                break;
                        }

                        sb.AppendLine($"ID:{sala.Id} - Código:{sala.CodigoUnico} - Idioma:{idioma} - Estado:{estadoTraducido}");
                    }
                    else
                    {
                        switch (sala.Estado)
                        {
                            case "INICIADA":
                                estadoTraducido = "In Game";
                                break;
                            case "EN_ESPERA":
                                estadoTraducido = "Waiting for player";
                                break;
                            case "TERMINADA":
                                estadoTraducido = "Finished";
                                break;
                        }

                        sb.AppendLine($"ID:{sala.Id} - Code:{sala.CodigoUnico} - Language:{idioma} - Status:{estadoTraducido}");
                    }
                }

                return sb.ToString();
            }
        }
        private int GenerarIdSala()
        {
            int nuevaSalaId = 1;
            while (salasActivas.ContainsKey(nuevaSalaId))
            {
                nuevaSalaId++;
            }
            return nuevaSalaId;
        }
        
        private int CrearSala(int idJugador, string nickname, int idioma, Socket socketCliente)
        {
            int salacreacion = -1;
            lock (locker)
            {
                var sala = new SalaJuego
                {
                    Id = GenerarIdSala(),
                    CodigoUnico = GenerarCodigoUnico(),
                    Estado = "EN_ESPERA",
                    idChallenger = idJugador,
                    idGuesser = 0,
                    Idioma = idioma
                };
                salacreacion = sala.Id;
                sala.Clientes.Add(new JugadorConectado
                {
                    IdPlayer = idJugador,
                    Nickname = nickname,
                    Nombre = nickname,
                    RolActual = "challenger",
                    Socket = socketCliente,
                    UltimoPing = DateTime.UtcNow
                });

                salasActivas[sala.Id] = sala;

                socketCliente.Send(Encoding.UTF8.GetBytes($"SALA_CREADA|{sala.Id}\n"));
            }
            return salacreacion;
        }


        private void ActualizarPartida(SalaJuego sala, Socket socketIndividual, bool limpiar)
        {
            if (socketIndividual == null || !socketIndividual.Connected)
            {
                Console.WriteLine("❌ Socket no conectado. Se omite el envío de estado.");
                return;
            }
            int idGanador = -1;
            string jugadoresConcat = string.Join(",", sala.Clientes.Select(c => c.Nickname));
            string estado = sala.EstadoPalabra ?? new string('_', sala.Palabra?.Length ?? 5);
            int intentos = 6 - sala.IntentosFallidos;
            bool partidaFinalizada = false;
            string estadoLogico;
            string pista = sala.Hint;
            if (intentos <= 0 && sala.Estado == "INICIADA")
            {
                estadoLogico = sala.Clientes.Any(c => c.RolActual == "guesser" && c.Socket == socketIndividual)
                    ? "PERDISTE"
                    : "GANASTE";
                partidaFinalizada = true;
                idGanador = sala.idChallenger;
            }
            else if (!estado.Contains("_") && sala.Estado == "INICIADA")
            {
                estadoLogico = sala.Clientes.Any(c => c.RolActual == "guesser" && c.Socket == socketIndividual)
                    ? "GANASTE"
                    : "PERDISTE";
                partidaFinalizada = true;
                idGanador = sala.idGuesser;
            }
            else
            {
                estadoLogico = sala.Estado;
            }
            string turnoActual = sala.TurnoActual;

            string estadoMensaje = $"ESTADO_PARTIDA|" +
                $"{estado}|INTENTOS:{intentos}|" +
                $"JUGADORES:{jugadoresConcat}|" +
                $"ESTADO:{estadoLogico}|" +
                $"LETRA:{sala.LetraPropuesta}|" +
                $"ACCION:{sala.AccionResultado}|" +
                $"TECLADO:{turnoActual}|" +
                $"CODIGO:{sala.CodigoUnico}|" +
                $"PISTA:{pista}";

            try
            {
                socketIndividual.Send(Encoding.UTF8.GetBytes(estadoMensaje + "\n"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando estado a un cliente: {ex.Message}");
            }
            if (partidaFinalizada && !sala.PartidaRegistrada)
            {
                try
                {
                    var binding = new BasicHttpBinding();
                    var endpoint = new EndpointAddress("http://localhost:64520/GameService.svc");
                    var factory = new ChannelFactory<IGameService>(binding, endpoint);
                    var gameService = factory.CreateChannel();

                    gameService.RegistrarPartidaFinalizada(
                        sala.Id,
                        sala.idChallenger,
                        sala.idGuesser,
                        sala.idPalabra,
                        idGanador,
                        sala.CodigoUnico
                    );
                    sala.PartidaRegistrada = true;
                    Console.WriteLine($"Partida {sala.Id} finalizada y registrada. Ganador: {idGanador}");

                    ((IClientChannel)gameService).Close();
                    factory.Close();

                    LimpiarSala(sala.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error registrando partida finalizada: {ex.Message}");
                }
            }
            if (limpiar)
            {
                sala.LetraPropuesta = "";
                sala.AccionResultado = "";
            }
        }

        private void VerificarInicioDePartida(SalaJuego sala)
        {
            if (sala.idChallenger != 0 && sala.idGuesser != 0 && !string.IsNullOrEmpty(sala.Palabra))
            {
                if (sala.Estado != "INICIADA")
                {
                    sala.Estado = "INICIADA";
                    Console.WriteLine($"✅ Partida en sala {sala.Id} iniciada correctamente.");
                    sala.TurnoActual = "GUESSER";
                }
            }
            else
            {
                if (sala.Estado != "EN_ESPERA")
                {
                    sala.Estado = "EN_ESPERA";
                    Console.WriteLine($"⏳ Sala {sala.Id} en espera: Challenger={sala.idChallenger}, Guesser={sala.idGuesser}, Palabra='{sala.Palabra}'");
                }
            }
        }
        private string ConfirmarRechazoLetra(SalaJuego sala, string letra)
        {
            if (sala.Palabra.IndexOf(letra, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sala.TurnoActual = "GUESSER";
                char[] estado = sala.EstadoPalabra.ToCharArray();
                for (int i = 0; i < sala.Palabra.Length; i++)
                {
                    if (sala.Palabra[i].ToString().Equals(letra, StringComparison.OrdinalIgnoreCase))
                    {
                        estado[i] = sala.Palabra[i];
                    }
                }

                sala.EstadoPalabra = new string(estado);
                sala.AccionResultado = "TRAMPA_RECHAZAR";
                sala.LetraPropuesta = letra;

                return "TRAMPA_RECHAZAR";
            }

            sala.IntentosFallidos++;
            sala.AccionResultado = "RECHAZADA";
            sala.LetraPropuesta = letra;
            sala.TurnoActual = "GUESSER";
            return "RECHAZADA";
        }

        private void NotificarSalidaJugador(int idJugador, int salaId, bool salidaVoluntaria)
        {
            lock (locker)
            {

                if (!salasActivas.TryGetValue(salaId, out var sala))
                    return;
                if (!salidaVoluntaria)
                {
                    var desconectado = sala.Clientes.FirstOrDefault(c => c.IdPlayer == idJugador);
                    if (desconectado != null)
                    {
                        sala.Clientes.Remove(desconectado);
                        JugadoresConectados.Remove(desconectado.IdPlayer);
                    }
                }
               

                
                foreach (var cliente in sala.Clientes)
                {
                    try
                    {
                        cliente.Socket.Send(Encoding.UTF8.GetBytes("OPONENTE_DESCONECTADO|\n"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error notificando salida: {ex.Message}");
                    }
                }

                if (!sala.PartidaRegistrada && sala.idPalabra > 0)
                {
                    try
                    {
                        Console.WriteLine("[PARTIDA] INTENTANDO REGISTRO");
                        var binding = new BasicHttpBinding();
                        var endpoint = new EndpointAddress("http://localhost:64520/GameService.svc");
                        var factory = new ChannelFactory<IGameService>(binding, endpoint);
                        var gameService = factory.CreateChannel();

                        gameService.RegistrarPartidaInconclusa(
                            sala.Id,
                            sala.idChallenger,
                            sala.idGuesser,
                            sala.idPalabra,
                            idJugador, 
                            sala.CodigoUnico
                        );

                        ((IClientChannel)gameService).Close();
                        factory.Close();

                        Console.WriteLine($"Partida {salaId} registrada como inconclusa.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Error al registrar partida inconclusa: {ex.Message}");
                    }
                }
                salasActivas.Remove(salaId);
                Console.WriteLine($"[Servidor] Jugador con ID {idJugador} salió o fue desconectado de la sala {salaId}.");
            }
        }
        private void MostrarEstadoSala(int salaId)
        {
            lock (locker)
            {
                if (!salasActivas.TryGetValue(salaId, out var sala))
                {
                    Console.WriteLine($"[Estado de la Sala {salaId}] No existe.");
                    return;
                }

                Console.WriteLine($"\n[Estado de la Sala {salaId}]");

                var challenger = sala.Clientes.FirstOrDefault(j => j.IdPlayer == sala.idChallenger);
                if (challenger != null)
                {
                    Console.WriteLine($" - Challenger: {challenger.Nickname} (ID {challenger.IdPlayer})");
                }
                else
                {
                    Console.WriteLine(" - Challenger: (no asignado)");
                }

                var guesser = sala.Clientes.FirstOrDefault(j => j.IdPlayer == sala.idGuesser);
                if (guesser != null)
                {
                    Console.WriteLine($" - Guesser: {guesser.Nickname} (ID {guesser.IdPlayer})");
                }
                else
                {
                    Console.WriteLine(" - Guesser: (no asignado)");
                }

                Console.WriteLine();
            }
        }

        
        

        public int EstablecerPalabra(int salaId, string palabra, int idPalabra, string hint)
        {
            lock (locker)
            {
                if (!salasActivas.TryGetValue(salaId, out var sala))
                    return -1;

                if (!string.IsNullOrEmpty(sala.Palabra))
                    return -2;

                sala.Palabra = palabra.ToUpper();
                sala.EstadoPalabra = new string('_', palabra.Length);
                sala.LetrasAdivinadas.Clear();
                sala.IntentosFallidos = 0;
                sala.idPalabra = idPalabra;
                sala.Terminada = false;
                sala.Hint = hint;
                return 1; 
            }
        }





        public void NotificarDesconexionEnPartida(int idPlayer)
        {
            int salaId = ObtenerSalaIdPorJugador(idPlayer);
            if (salaId == -1)
            {
                Console.WriteLine($"[Monitor] No se encontró una sala para el jugador {idPlayer}");
                return;
            }

            if (!salasActivas.TryGetValue(salaId, out var sala))
            {
                Console.WriteLine($"[Monitor] Sala {salaId} no activa. Abortando.");
                return;
            }

            int idJugadorRestante = sala.Clientes
                .FirstOrDefault(j => j.IdPlayer != idPlayer)?.IdPlayer ?? 0;

            bool jugadorRestanteActivo = JugadoresConectados.TryGetValue(idJugadorRestante, out var jugadorRestante)
                                         && SocketActivo(jugadorRestante.Socket);

            if (!jugadorRestanteActivo)
            {
                Console.WriteLine($"[Monitor] Ambos jugadores están desconectados en sala {salaId}. No se registrará partida.");
                LimpiarSala(salaId); 
                return;
            }

            int idChallenger = sala.idChallenger;
            int idGuesser = sala.idGuesser;
            int idPalabra = sala.idPalabra;
            string codigoSala = sala.CodigoUnico;

            if (!sala.Terminada && sala.idPalabra > 0 && !sala.PartidaRegistrada)
            {
                try
                {
                    var binding = new BasicHttpBinding();
                    var endpoint = new EndpointAddress("http://localhost:64520/GameService.svc");
                    var factory = new ChannelFactory<IGameService>(binding, endpoint);
                    var gameService = factory.CreateChannel();

                    gameService.RegistrarPartidaInconclusa(
                        sala.Id,
                        sala.idChallenger,
                        sala.idGuesser,
                        sala.idPalabra,
                        idPlayer,
                        sala.CodigoUnico
                    );

                    sala.PartidaRegistrada = true;

                    ((IClientChannel)gameService).Close();
                    factory.Close();

                    Console.WriteLine($" Partida {salaId} registrada como inconclusa.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al registrar partida inconclusa: {ex.Message}");
                }
            }
            try
            {
                jugadorRestante.Socket.Send(Encoding.UTF8.GetBytes($"OPONENTE_DESCONECTADO|{idJugadorRestante}"));
                Console.WriteLine($"[Notificación] Se avisó a {jugadorRestante.Nickname} que su oponente se desconectó.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo notificar al jugador restante: {ex.Message}");
            }

            LimpiarSala(salaId);
        }



        private int ObtenerSalaIdPorJugador(int idPlayer)
        {
            lock (locker)
            {
                foreach (var kv in salasActivas)
                {
                    if (kv.Value.Clientes.Any(c => c.IdPlayer == idPlayer))
                        return kv.Key;
                }
            }
            return -1;
        }
        public static bool VerificarUsuarioConectado(int idPlayer)
        {
                foreach (var jugador in JugadoresConectados.ToList())
                {
                    if (jugador.Value.IdPlayer ==idPlayer)
                        return true;
                }
            return false;
        }

    }
    public class JugadorConectado
    {
        public string Nombre { get; set; }
        public string Nickname { get; set; }
        public int IdPlayer { get; set; }
        public string RolActual { get; set; }
        public Socket Socket { get; set; }
        public DateTime UltimoPing { get; set; }
    }
    public class SalaJuego
    {
        public int Id { get; set; }
        public string CodigoUnico { get; set; }
        public int idPalabra { get; set; }
        public string Palabra { get; set; }
        public string Estado { get; set; } = "EN_ESPERA";
        public string EstadoPalabra { get; set; }
        public string LetraPropuesta { get; set; }
        public string AccionResultado { get; set; } = "";
        public HashSet<string> LetrasAdivinadas { get; set; } = new HashSet<string>();
        public int IntentosFallidos { get; set; }
        public bool Terminada { get; set; }
        public int Idioma { get; set; }
        public string Hint { get; set; }
        public int idChallenger { get; set; }
        public int idGuesser { get; set; }
        public string TurnoActual { get; set; } = "GUESSER";
        public bool PartidaRegistrada { get; set; } = false;
        public List<JugadorConectado> Clientes { get; set; } = new List<JugadorConectado>();
    }
}
        


        
    
