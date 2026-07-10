using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZSong.IO {

    public static class SongPersistancyManager {
        public static void Save(string path, Song song) {
            SongDto dto = song.ToDto();

            string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

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
