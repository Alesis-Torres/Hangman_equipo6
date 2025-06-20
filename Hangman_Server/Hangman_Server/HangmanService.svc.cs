using Hangman_Server.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text.RegularExpressions;

namespace Hangman_Server
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class HangmanService : IHangmanService
    {
        public PlayerDTO Login(string username, string password)
        {
            using (var db = new HangmanEntities())
            {
                var user = db.player.FirstOrDefault(u => u.username == username && u.password == password);
                if (user != null)
                {
                    int idUser = user.id_player;
                    bool existeJugador = SocketServidor.VerificarUsuarioConectado(idUser);

                    if (!existeJugador)
                    {
                        return new PlayerDTO
                        {
                            Username = user.username,
                            Nickname = user.nickname,
                            IdPlayer = user.id_player,
                            Password = user.password,
                            Email = user.email,
                            Birthdate = user.birthdate,
                            PhoneNumber = user.phonenumber?.ToString(),
                            ImgRoute = user.img_route,
                            Score = (int)user.score,
                            SessionDuplicate = false
                        };
                    }
                    else
                    {
                        return new PlayerDTO
                        {
                            Username = user.username,
                            SessionDuplicate = true
                        };
                    }
                }
                return null;
            }
        }
        public int ObtenerIdPorUsername(string username)
        {
            using (var db = new HangmanEntities())
            {
                var player = db.player.FirstOrDefault(p => p.username == username);
                return player?.id_player ?? 0;
            }
        }
        public (List<string> resultados, int puntajeTotal) ObtenerHistorialPartidas(int playerId, int idLanguage)
        {
            using (var db = new HangmanEntities())
            {
                var partidas = (from g in db.gamematch
                                join s in db.gamematch_status on g.id_gamematch_status equals s.id_gamematch_status
                                join w in db.word on g.id_word equals w.id_word
                                where g.id_player_challenger == playerId || g.id_player_guesser == playerId
                                select new
                                {
                                    Palabra = w.name,
                                    Resultado = s.name,
                                    IdGanador = g.id_playerinfo,
                                    Fecha = g.date_finished,
                                    SoyYo = playerId,
                                    IdStatus = g.id_gamematch_status,
                                    EsChallenger = g.id_player_challenger == playerId,
                                    EsGuesser = g.id_player_guesser == playerId
                                }).ToList();

                int puntajeTotal = 0;

                var listaResultados = partidas.Select(p =>
                {
                    string fechaStr = p.Fecha?.ToString("dd/MM/yyyy HH:mm") ?? (idLanguage == 1 ? "sin fecha" : "no date");

                    string rolTraducido = p.EsGuesser
                        ? (idLanguage == 1 ? "Adivinador" : "Guesser")
                        : (idLanguage == 1 ? "Retador" : "Challenger");

                    if (p.IdStatus == 2 && p.IdGanador == p.SoyYo)
                    {
                        puntajeTotal = Math.Max(0, puntajeTotal - 3);
                        return idLanguage == 1
                            ? $"Desconexión - Rol: {rolTraducido} - Palabra: {p.Palabra} - Fecha: {fechaStr} - Score: -3"
                            : $"Disconnected - Role: {rolTraducido} - Word: {p.Palabra} - Date: {fechaStr} - Score: -3";
                    }
                    else if (p.IdStatus == 1 && p.IdGanador == p.SoyYo)
                    {
                        int puntaje = 0;
                        if (p.EsGuesser)
                        {
                            puntajeTotal += 10;
                            puntaje = 10;
                        }
                        else if (p.EsChallenger)
                        {
                            puntajeTotal += 5;
                            puntaje = 5;
                        }

                        return idLanguage == 1
                            ? $"Ganada - Rol: {rolTraducido} - Palabra: {p.Palabra} - Fecha: {fechaStr}  - Score: {puntaje}"
                            : $"Won - Role: {rolTraducido} - Word: {p.Palabra} - Date: {fechaStr} - Score: {puntaje}"
                ;
                    }
                    else if (p.Resultado.ToLower() == "inconclusa")
                    {
                        return idLanguage == 1
                            ? $"Inconclusa - Rol: {rolTraducido} - Palabra: {p.Palabra} - Fecha: {fechaStr}"
                            : $"Unfinished - Role: {rolTraducido} - Word: {p.Palabra} - Date: {fechaStr}";
                    }
                    else
                    {
                        return idLanguage == 1
                            ? $"Perdida - Rol: {rolTraducido} - Palabra: {p.Palabra} - Fecha: {fechaStr}"
                            : $"Lost - Role: {rolTraducido} - Word: {p.Palabra} - Date: {fechaStr}";
                    }
                }).ToList();

                return (listaResultados, puntajeTotal);
            }
        }

        public bool RegisterPlayer(PlayerDTO newPlayer)
        {
            if (!ValidarPlayerDTO(newPlayer, out string errorMessage))
            {
                Console.WriteLine($"Registro fallido: {errorMessage}");
                return false;
            }

            using (var db = new HangmanEntities())
            {
                var existingUser = db.player.FirstOrDefault(u => u.username == newPlayer.Username);
                if (existingUser != null)
                {
                    return false;
                }
                string imgFileName = $"{newPlayer.Username}.png";
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "Users");
                Directory.CreateDirectory(imagesFolder);
                string imagePath = Path.Combine(imagesFolder, imgFileName);

                if (newPlayer.ImgBytes != null && newPlayer.ImgBytes.Length > 0)
                {
                    File.WriteAllBytes(imagePath, newPlayer.ImgBytes);
                }

                var newDbPlayer = new player
                {
                    username = newPlayer.Username,
                    nickname = newPlayer.Nickname,
                    password = newPlayer.Password,
                    email = newPlayer.Email,
                    birthdate = newPlayer.Birthdate,
                    phonenumber = long.TryParse(newPlayer.PhoneNumber, out long phone) ? (long?)phone : null,
                    img_route = imgFileName,
                    score = 0
                };

                db.player.Add(newDbPlayer);
                db.SaveChanges();
                return true;
            }
        }

        public bool UpdatePlayerProfile(PlayerDTO player)
        {
            if (!ValidarPlayerDTO(player, out string errorMessage))
            {
                Console.WriteLine($"Actualización fallida: {errorMessage}");
                return false;
            }

            using (var db = new HangmanEntities())
            {
                var existingUser = db.player.FirstOrDefault(u => u.id_player == player.IdPlayer);
                if (existingUser == null)
                    return false;

                bool nicknameDuplicado = db.player.Any(u => u.nickname == player.Nickname && u.id_player != player.IdPlayer);
                bool emailDuplicado = db.player.Any(u => u.email == player.Email && u.id_player != player.IdPlayer);
                bool telefonoDuplicado = !string.IsNullOrWhiteSpace(player.PhoneNumber)
                    && db.player.Any(u => u.phonenumber.ToString() == player.PhoneNumber && u.id_player != player.IdPlayer);

                if ((existingUser.nickname != player.Nickname && nicknameDuplicado) ||
                    (existingUser.email != player.Email && emailDuplicado) ||
                    (existingUser.phonenumber?.ToString() != player.PhoneNumber && telefonoDuplicado))
                {
                    Console.WriteLine("Error: Nickname, correo o teléfono ya en uso por otro usuario.");
                    return false;
                }
                existingUser.nickname = player.Nickname;
                existingUser.password = player.Password;
                existingUser.email = player.Email;
                existingUser.birthdate = player.Birthdate;
                existingUser.phonenumber = long.TryParse(player.PhoneNumber, out long phone) ? (long?)phone : null;
                existingUser.img_route = player.ImgRoute;

                db.SaveChanges();
                return true;
            }
        }

        public PlayerStatsDTO GetPlayerStats(int playerId)//TODO REMOVER ES DUMMIE
        {
            return new PlayerStatsDTO
            {
                Username = "DemoUser",
                GamesPlayed = 10,
                Wins = 5
            };
        }

        private bool ValidarPlayerDTO(PlayerDTO player, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(player.Username) ||
                string.IsNullOrWhiteSpace(player.Nickname) ||
                string.IsNullOrWhiteSpace(player.Password) ||
                string.IsNullOrWhiteSpace(player.Email))
            {
                errorMessage = "Todos los campos obligatorios deben estar completos.";
                return false;
            }

            if (player.Username.Length > 60 ||
                player.Nickname.Length > 12 ||
                player.Password.Length > 20 ||
                player.Email.Length > 45)
            {
                errorMessage = "Algún campo excede la longitud máxima permitida.";
                return false;
            }

            var regex = new Regex("^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(player.Username) || !regex.IsMatch(player.Nickname) || !regex.IsMatch(player.Password))
            {
                errorMessage = "Username, Nickname y Password sólo deben contener caracteres A-Z, a-z, 0-9.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(player.PhoneNumber) && !Regex.IsMatch(player.PhoneNumber, @"^\d{0,15}$"))
            {
                errorMessage = "El número de teléfono debe contener sólo dígitos y un máximo de 15 caracteres.";
                return false;
            }

            return true;
        }
        public bool EmailExiste(string email)
        {
            using (var db = new HangmanEntities())
            {
                return db.player.Any(u => u.email == email);
            }
        }

        public bool PhoneNumberExiste(string phoneNumber)
        {
            using (var db = new HangmanEntities())
            {
                long.TryParse(phoneNumber, out long phone);
                return db.player.Any(u => u.phonenumber == phone);
            }
        }
        public bool UsernameExiste(string username)
        {
            using (var db = new HangmanEntities())
            {
                return db.player.Any(u => u.username == username);
            }
        }

        public bool NicknameExiste(string nickname)
        {
            using (var db = new HangmanEntities())
            {
                return db.player.Any(u => u.nickname == nickname);
            }
        }

        public class SessionInfo
        {
            public int idPlayer { get; set; }
            public string Username { get; set; }
            public DateTime LoginTime { get; set; }
            public Guid SessionId { get; set; }
        }
    }
}