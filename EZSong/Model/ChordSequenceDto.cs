namespace EZSong.Model {
    public class ChordSequenceDto {
        public List<ChordDto> Chords { get; set; }

        public ChordSequenceDto(List<ChordDto> chords) {
            Chords = chords;
        }
    }
}