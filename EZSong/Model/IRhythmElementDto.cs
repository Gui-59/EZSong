using System.Text.Json;
using System.Text.Json.Serialization;

namespace EZSong.Model {

    /*
     * Une interface DTO compatible JSON doit être :
     * - public interface
     */

    [JsonDerivedType(typeof(RhythmTupletDto), "RhythmTuplet")]
    [JsonDerivedType(typeof(RhythmSimpleElementDto), "RhythmSimpleElement")]
    public interface IRhythmElementDto {

    }

}
