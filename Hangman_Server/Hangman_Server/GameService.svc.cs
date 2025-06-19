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
        

        public string ProbarConexion()
        {
            return "Conexión establecida";
        }

        public string ObtenerSalas()
        {
            return "TEST OBTENER SALA CLIENTE";
        }

        public string ObtenerPalabraPorSala(int salaId)
        {
            return "TEST";
        }
        public int CrearSala(string nombreJugador, int idCliente)
        {
            return 99999999;
        }
        public int CrearSalaV2(string nombreJugador, int idCliente)
        {
            return 99999999;
        }
        public int EstablecerPalabra(int idSala, string palabra)
        {
            int mensaje = 9999;
            return mensaje;
        }
        public bool EsPartidaTerminada(int salaId)
        {
            return false;
        }

        public string UnirseSala(int salaId, string nombreJugador, int idPlayerGuesser)
        {
            return "TEST";
        }

        public void Salir(int salaId, string nombreJugador)
        {
        }

        public int ObtenerJugadoresEnSala(int salaId)
        {
            return 999;
        }
        public string AgregarJugador(int salaId, string nombreJugador)
        {
            return "TEST";
        }

        public string ObtenerEstadoPalabra(int salaId)
        {
            try
            {
                return "TEST";
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
                return 9999;
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
                return "TEST";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public void EnviarLetra(int salaId, string letra)
        {

        }

        public string ConfirmarLetra(int salaId, string letra)
        {
            return "TEST";
        }

        public string RechazarLetra(int salaId, string letra)
        {
            return "TEST";
        }
        public int RegistrarPartidaFinalizada(int idChallenger, int idGuesser, int idPalabra, string estado, int idJugadorResultado)
        {
            Console.WriteLine($"[WCF] Registrando partida - Challenger: {idChallenger}, Guesser: {idGuesser}, Palabra: {idPalabra}, Estado: {estado}, Resultado: {idJugadorResultado}");

            try
            {
                if (idChallenger <= 0 || idGuesser <= 0 || idPalabra <= 0 || string.IsNullOrWhiteSpace(estado) || idJugadorResultado <= 0)
                {
                    Console.WriteLine("❌ Datos inválidos para el registro.");
                    return 0;
                }

                using (var db = new HangmanEntities())
                {
                    var gameMatch = new gamematch
                    {
                        id_player_challenger = idChallenger,
                        id_player_guesser = idGuesser,
                        id_word = idPalabra,
                        id_gamematch_status = 0
                    };

                    db.gamematch.Add(gameMatch);
                    db.SaveChanges();

                    var status = new gamematch_status
                    {
                        id_gamematch_status = gameMatch.id_gamematch,
                        name = estado,
                        id_player = idJugadorResultado
                    };

                    db.gamematch_status.Add(status);
                    db.SaveChanges();

                    gameMatch.id_gamematch_status = status.id_gamematch_status;
                    db.SaveChanges();

                    Console.WriteLine("✅ Partida registrada correctamente en WCF.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al registrar partida en WCF: {ex.Message}");
                return -1;
            }
        }
        /*public void RegistrarPartidaInconclusa(int salaId, int idDesconectado, string palabra)
        {
            Console.WriteLine($"[GameService] Registrando partida inconclusa. salaId={salaId}, idDesconectado={idDesconectado}, palabra={palabra}");
            using (var db = new HangmanEntities())
            {
                int challengerId = _servidor.ObtenerChallengerIdPorSala(salaId);
                int guesserId = _servidor.ObtenerGuesserIdPorSala(salaId);

                if (challengerId == 0 || guesserId == 0)
                {
                    Console.WriteLine($"❌ No se pudo obtener uno de los jugadores para sala {salaId}. Abortando registro.");
                    return;
                }
                
                var word = db.word.FirstOrDefault(w => w.name == palabra);
                if (word == null)
                {
                    Console.WriteLine($"⚠️ No se encontró la palabra '{palabra}' en la base de datos.");
                    return;
                }

                var gamematch = new gamematch
                {
                    id_player_challenger = challengerId,
                    id_player_guesser = guesserId,
                    id_word = word.id_word,
                    code = Guid.NewGuid().ToString().Substring(0, 7).ToUpper()
                };

                db.gamematch.Add(gamematch);
                db.SaveChanges();

                db.gamematch_status.Add(new gamematch_status
                {
                    id_gamematch_status = gamematch.id_gamematch,
                    name = "desconectada",
                    id_player = idDesconectado
                });

                db.SaveChanges();

                _servidor.LimpiarSala(salaId);

                Console.WriteLine($"✅ Partida inconclusa registrada correctamente. id_gamematch={gamematch.id_gamematch}");
            }
        }*/
        public int ObtenerSalaIdPorCodigo(string codigo)
        {
            //var id = _servidor.ObtenerSalaIdPorCodigo(codigo);
            return  -1;
        }
        public string ObtenerCodigoDeSala(int salaId)
        {
            return "TEST";
        }
        public int ObtenerIdGuesser(int salaId)
        {
                return 0;
        }
        public List<string> ObtenerJugadoresEnPartida(int salaId)
        {
            List<string> test =  null;
            return test;
        }
        public int ObtenerIdWord(int salaId)
        {
                
                return 0;
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

                System.Diagnostics.Debug.WriteLine($"Intentando cargar imagen: {rutaImagen}");

                if (File.Exists(rutaImagen))
                {
                    System.Diagnostics.Debug.WriteLine($" Imagen encontrada: {rutaImagen}");
                    return File.ReadAllBytes(rutaImagen);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($" Imagen no encontrada: {rutaImagen}");
                    if (File.Exists(rutaPlaceholder))
                    {
                        System.Diagnostics.Debug.WriteLine($" Placeholder usado: {rutaPlaceholder}");
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