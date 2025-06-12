using Hangman_Server.Model;
using Hangman_Server.Model.DTO;
using HangmanClient.Model.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;

namespace Hangman_Server
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GameService : IGameService
    {
        private readonly SocketServidor _servidor = SocketServidor.ObtenerInstancia();

        public string ProbarConexion()
        {
            return "Conexión establecida";
        }

        public string ObtenerSalas()
        {
            try
            {
                return _servidor.ObtenerSalas();
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
        public string CrearSala(string nombreJugador, int idPlayer)
        {
            int salaId = _servidor.CrearSala(nombreJugador, idPlayer);
            return $"SALA:{salaId} ROLE:challenger";
        }
        public bool EsPartidaTerminada(int salaId)
        {
            return _servidor.EsPartidaTerminada(salaId);
        }

        public string UnirseSala(int salaId, string nombreJugador, int idPlayerGuesser)
        {
            return _servidor.UnirseSala(salaId, nombreJugador, idPlayerGuesser);
        }

        public void Salir(int salaId, string nombreJugador)
        {
            _servidor.EliminarJugador(salaId, nombreJugador);
        }

        public int ObtenerJugadoresEnSala(int salaId)
        {
            return _servidor.ObtenerJugadoresEnSala(salaId);
        }
        public string AgregarJugador(int salaId, string nombreJugador)
        {
            return _servidor.AgregarJugador(salaId, nombreJugador);
        }

        public string ObtenerEstadoPalabra(int salaId)
        {
            try
            {
                return _servidor.ObtenerEstadoPalabra(salaId);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public int ObtenerIntentosRestantes(int salaId)
        {
            try
            {
                return _servidor.ObtenerIntentosRestantes(salaId);
            }
            catch
            {
                return 0;
            }
        }

        public string ObtenerLetraPropuesta(int salaId)
        {
            try
            {
                return _servidor.ObtenerLetraPropuesta(salaId);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public void EnviarLetra(int salaId, string letra)
        {
            try
            {
                _servidor.EnviarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en EnviarLetra: {ex.Message}");
            }
        }

        public string ConfirmarLetra(int salaId, string letra)
        {
            try
            {
                return _servidor.ConfirmarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ConfirmarLetra: {ex.Message}");
                return "ERROR";
            }
        }

        public string RechazarLetra(int salaId, string letra)
        {
            try
            {
                return _servidor.RechazarLetra(salaId, letra);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en RechazarLetra: {ex.Message}");
                return "ERROR";
            }
        }
        public void RegistrarPartidaFinalizada(int salaId, bool palabraAdivinada)
        {
            Console.WriteLine($"[GameService] Registrando partida para salaId={salaId}, palabraAdivinada={palabraAdivinada}");
            using (var db = new HangmanEntities())
            {
                // Obtener los ids de challenger y guesser
                int idPlayerChallenger = _servidor.ObtenerChallengerIdPorSala(salaId);
                int idPlayerGuesser = _servidor.ObtenerGuesserIdPorSala(salaId);
                string palabra = _servidor.ObtenerPalabraPorSala(salaId);
                Console.WriteLine( idPlayerChallenger + " " + idPlayerChallenger + " "+ palabra);
                if (idPlayerChallenger == 0 || idPlayerGuesser == 0 || string.IsNullOrEmpty(palabra))
                {
                    Console.WriteLine($"❌ Error: Datos incompletos para registrar partida.");
                    throw new FaultException("No se encontró la sala o los datos necesarios para registrar la partida. " +idPlayerChallenger + " " + idPlayerGuesser + " " + palabra);
                }

                var gameMatch = new gamematch
                {
                    id_player_challenger = idPlayerChallenger,
                    id_player_guesser = idPlayerGuesser,
                    id_word = db.word.FirstOrDefault(w => w.name == palabra)?.id_word ?? 0
                };

                db.gamematch.Add(gameMatch);
                db.SaveChanges();

                var status = new gamematch_status
                {
                    id_gamematch_status = gameMatch.id_gamematch,
                    name = "Finalizada",
                    id_player = palabraAdivinada ? idPlayerGuesser : idPlayerChallenger
                };

                db.gamematch_status.Add(status);
                db.SaveChanges();

                Console.WriteLine("✅ Partida registrada correctamente.");

                // Limpiar la sala
                _servidor.LimpiarSalaDespuesDeRegistrar(salaId);
            }
        }

        public void RegistrarPartidaInconclusa(int idPlayerChallenger, int idDisconnected)
        {
            using (var db = new HangmanEntities())
            {
                int salaId = _servidor.ObtenerSalaIdPorChallenger(idPlayerChallenger);
                string palabra = _servidor.ObtenerPalabraPorSala(salaId);

                int idGuesser = _servidor.ObtenerGuesserIdPorSala(salaId);
                int idWord = db.word.FirstOrDefault(w => w.name == palabra)?.id_word ?? 0;

                var gameMatch = new gamematch
                {
                    id_player_challenger = idPlayerChallenger,
                    id_player_guesser = idGuesser,
                    id_word = idWord
                };
                db.gamematch.Add(gameMatch);
                db.SaveChanges();

                var status = new gamematch_status
                {
                    id_gamematch_status = gameMatch.id_gamematch,
                    name = "Inconclusa",
                    id_player = idDisconnected
                };
                db.gamematch_status.Add(status);
                db.SaveChanges();
                _servidor.LimpiarSalaDespuesDeRegistrar(salaId);
            }
        }

        public int ObtenerIdGuesser(int salaId)
        {
            using (var db = new HangmanEntities())
            {
                var sala = db.gamematch.FirstOrDefault(s => s.id_gamematch == salaId);
                if (sala != null)
                {
                    return sala.id_player_guesser ?? 0;
                }
                return 0;
            }
        }
        public List<string> ObtenerJugadoresEnPartida(int salaId)
        {
            return _servidor.ObtenerJugadoresEnSalaConNombres(salaId);
        }
        public int ObtenerIdWord(int salaId)
        {
            using (var db = new HangmanEntities())
            {
                var sala = db.gamematch.FirstOrDefault(s => s.id_gamematch == salaId);
                if (sala != null)
                {
                    return sala.id_word ?? 0;
                }
                return 0;
            }
        }
        public List<CategoryDTO> ObtenerCategorias(int idLanguage)
        {
            using (var db = new HangmanEntities())
            {
                var categoriasBD = db.category
                    .Where(c => c.id_language == idLanguage)
                    .Select(c => new { c.name, c.img_name })
                    .ToList();

                var categorias = categoriasBD.Select(c => new CategoryDTO
                {
                    Name = c.name,
                    ImageBytes = LeerImagenConPlaceholder("Categories", c.img_name)
                }).ToList();

                return categorias;
            }
        }


        public List<WordDTO> ObtenerPalabrasPorCategoria(string categoria, int idLanguage)
        {
            using (var db = new HangmanEntities())
            {
                var palabrasBD = (from w in db.word
                                  join c in db.category on w.id_category equals c.id_category
                                  where c.name == categoria && w.id_language == idLanguage
                                  select new { w.name, w.img_name })
                                  .ToList();

                var palabras = palabrasBD.Select(w => new WordDTO
                {
                    Name = w.name,
                    ImageBytes = LeerImagenConPlaceholder("Words", w.img_name)
                }).ToList();

                return palabras;
            }
        }

        private byte[] LeerImagenConPlaceholder(string folder, string imgName)
        {
            try
            {
                string rutaPlaceholder = HttpContext.Current.Server.MapPath("~/Resources/Images/placeholder_img.png");

                if (!string.IsNullOrWhiteSpace(imgName) && !Path.HasExtension(imgName))
                {
                    imgName = imgName + ".png";
                }

                string rutaImagen = HttpContext.Current.Server.MapPath($"~/Resources/Images/{folder}/{imgName}");

                System.Diagnostics.Debug.WriteLine($"📂 Intentando cargar imagen: {rutaImagen}");

                if (File.Exists(rutaImagen))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Imagen encontrada: {rutaImagen}");
                    return File.ReadAllBytes(rutaImagen);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Imagen no encontrada: {rutaImagen}");
                    if (File.Exists(rutaPlaceholder))
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Placeholder usado: {rutaPlaceholder}");
                        return File.ReadAllBytes(rutaPlaceholder);
                    }
                    else
                    {
                        throw new FileNotFoundException("No se encontró la imagen ni el archivo placeholder_img.png.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar la imagen: {ex.Message}");
            }
        }
    }
}