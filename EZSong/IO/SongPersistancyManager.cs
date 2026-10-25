using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZSong.IO {

    /// <summary>
    /// Manages the persistence of Song objects to and from JSON files.
    /// </summary>
    public static class SongPersistancyManager {

        /// <summary>
        /// Saves a Song object to a specified file path in JSON format.
        /// </summary>
        /// <param name="path">The file path to save the JSON data to.</param>
        /// <param name="song">The Song object to save.</param>
        public static void Save(string path, Song song) {
            SongDto dto = song.ToDto();

            string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads a Song object from a specified file path containing JSON data.
        /// </summary>
        /// <param name="path">The file path to load the JSON data from.</param>
        /// <returns>The deserialized Song object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the file does not contain valid Song data.</exception>
        public static Song Load(string path) {
            string json = File.ReadAllText(path);

            SongDto? dto = JsonSerializer.Deserialize<SongDto>(json);

            if (dto is null) {
                throw new InvalidOperationException("Le fichier ne contient pas de données valides.");
            }

            return Song.FromDto(dto);
        }
    }
}
