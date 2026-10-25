using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Describes the movement of one musical voice between two successive voicings.
/// A null side means that the voice was added or removed.
/// </summary>
public sealed class VoiceMovement {
    public Pitch? PreviousNote {
        get;
    }
    public Pitch? CurrentNote {
        get;
    }
    public int SemitoneMovement {
        get;
    }
    public bool IsAdded {
        get {
            return PreviousNote is null && CurrentNote is not null;
        }
    }

    public bool IsRemoved {
        get {
            return PreviousNote is not null && CurrentNote is null;
        }
    }

    public bool IsContinued {
        get {
            return PreviousNote is not null && CurrentNote is not null;
        }
    }

    public VoiceMovement(Pitch? previousNote, Pitch? currentNote) {
        if (previousNote is null && currentNote is null) {
            throw new ArgumentException("A voice movement must contain at least one note.");
        }

        PreviousNote = previousNote;
        CurrentNote = currentNote;
        SemitoneMovement = previousNote is null || currentNote is null
            ? 0
            : Math.Abs(currentNote - previousNote);
    }

    public override string ToString() {
        if (IsAdded) {
            return $"+{CurrentNote}";
        }

        if (IsRemoved) {
            return $"{PreviousNote}->-";
        }

        return $"{PreviousNote}->{CurrentNote} ({SemitoneMovement})";
    }
}
