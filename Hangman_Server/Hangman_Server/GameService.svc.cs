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

        public void RegistrarPartidaFinalizada(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala)
        {
            Console.WriteLine($"[GameService] Registrando partida finalizada. salaId={salaId}, idDesconectado={idDesconectado}, idPalabra={idPalabra}");

            using (var db = new HangmanEntities())
            {
                var palabra = db.word.FirstOrDefault(w => w.id_word == idPalabra);
                if (palabra == null)
                {
                    Console.WriteLine($"No se encontró la palabra con ID '{idPalabra}' en la base de datos.");
                    return;
                }

                var gamematch = new gamematch
                {
                    id_player_challenger = idChallenger,
                    id_player_guesser = idGuesser,
                    id_playerinfo = idDesconectado,
                    id_word = idPalabra,
                    id_gamematch_status = 1,
                    code = codigoSala,
                    date_finished = DateTime.Now
                };

                db.gamematch.Add(gamematch);
                db.SaveChanges();


                db.SaveChanges();
                Console.WriteLine($"Partida Finalizada registrada correctamente. id_gamematch={gamematch.id_gamematch}");
            }
        }
        public void RegistrarPartidaInconclusa(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala)
        {
            Console.WriteLine($"[GameService] Registrando partida inconclusa. salaId={salaId}, idDesconectado={idDesconectado}, idPalabra={idPalabra}");

            using (var db = new HangmanEntities())
            {
                var palabra = db.word.FirstOrDefault(w => w.id_word == idPalabra);
                if (palabra == null)
                {
                    Console.WriteLine($"No se encontró la palabra con ID '{idPalabra}' en la base de datos.");
                    return;
                }
                try { 
                var gamematch = new gamematch
                {
                    id_player_challenger = idChallenger,
                    id_player_guesser = idGuesser,
                    id_playerinfo = idDesconectado,
                    id_word = idPalabra,
                    id_gamematch_status = 2, 
                    code = codigoSala,
                    date_finished = DateTime.Now
                };

                db.gamematch.Add(gamematch);
                    db.SaveChanges();
                    Console.WriteLine($"Partida inconclusa registrada correctamente. id_gamematch={gamematch.id_gamematch}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: DATOS FALTANTES EN EL REGISTRO DE LA PARTIDA:"+ ex.ToString());
                }
                
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
                                  select new { w.id_word, w.name, w.img_name, w.hint })
                                  .ToList();

                var palabras = palabrasBD.Select(w => new WordDTO
                {
                    Id = w.id_word,
                    Name = w.name,
                    Hint = w.hint,
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