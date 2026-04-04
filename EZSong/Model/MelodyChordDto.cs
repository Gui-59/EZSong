namespace EZSong.Model {
    public class MelodyChordDto {
        public List<PitchDto> Pitches { get; set; }

        public MelodyChordDto(List<PitchDto> pitches) {
            Pitches = pitches;
        }
    }
}