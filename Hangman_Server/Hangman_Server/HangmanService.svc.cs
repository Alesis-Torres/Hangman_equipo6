using Hangman_Server.Model;
using Hangman_Server.Model.Singleton;
using System;
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
                    Guid sessionId;
                    bool isNewSession = SessionManager.ValidateAndRegisterSession(username, out sessionId);

                    if (!isNewSession)
                    {
                        // Devuelve una respuesta especial para indicar sesión duplicada
                        return new PlayerDTO
                        {
                            Username = username,
                            Nickname = user.nickname,
                            IdPlayer = user.id_player,
                            Password = user.password,
                            Email = user.email,
                            Birthdate = user.birthdate,
                            PhoneNumber = user.phonenumber?.ToString(),
                            ImgRoute = user.img_route,
                            Score = (int)user.score,
                            SessionDuplicate = true // NUEVO CAMPO
                        };
                    }

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
                        SessionDuplicate = false // NUEVO CAMPO
                    };
                }
                return null;
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
                Directory.CreateDirectory(imagesFolder); // Asegura que exista
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
                {
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
        public void Logout(string username)
        {
            SessionManager.RemoveSession(username);
        }
        public class SessionInfo
        {
            public string Username { get; set; }
            public DateTime LoginTime { get; set; }
            public Guid SessionId { get; set; }
        }
    }
}