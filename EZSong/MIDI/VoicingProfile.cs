using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Named, ready-to-use voicing configuration for common musical workflows.
/// Each access to a built-in profile returns a new instance so callers may
/// safely customise its settings without modifying another profile instance.
/// </summary>
public sealed class VoicingProfile {
    private VoicingProfile(
        string name,
        VoicingSettings oneHand,
        TwoHandVoicingSettings twoHand) {
        Name = name;
        OneHand = oneHand;
        TwoHand = twoHand;
    }

    public string Name {
        get;
    }

    /// <summary>Settings for the one-hand voicing engine.</summary>
    public VoicingSettings OneHand {
        get;
    }

    /// <summary>Settings for the true two-hand voicing engine.</summary>
    public TwoHandVoicingSettings TwoHand {
        get;
    }

    /// <summary>
    /// Compact left-hand profile intended for arranger-style chord playing.
    /// </summary>
    public static VoicingProfile ArrangerLeftHand {
        get {
            return CreateArrangerLeftHand();
        }
    }

    /// <summary>
    /// Compact left-hand profile with a deliberately narrow four-voice ceiling.
    /// </summary>
    public static VoicingProfile LeftHandCompact {
        get {
            return CreateLeftHandCompact();
        }
    }

    /// <summary>
    /// More spacious left-hand profile for open voicings.
    /// </summary>
    public static VoicingProfile LeftHandOpen {
        get {
            return CreateLeftHandOpen();
        }
    }

    /// <summary>
    /// General-purpose piano profile with a wider register and up to five voices.
    /// </summary>
    public static VoicingProfile Piano {
        get {
            return CreatePiano();
        }
    }

    /// <summary>
    /// Balanced true two-hand profile with a conventional LH/RH register split.
    /// </summary>
    public static VoicingProfile TwoHandsBalanced {
        get {
            return CreateTwoHandsBalanced();
        }
    }

    private static VoicingProfile CreateArrangerLeftHand() {
        VoicingSettings oneHand = CreateBaseOneHand();
        oneHand.MinChordAreaNote = P(NoteStep.C, 1);
        oneHand.MaxChordAreaNote = P(NoteStep.C, 3);
        oneHand.ChordMaxKeyCount = 5;
        oneHand.PreferCompactInitialVoicing = true;

        TwoHandVoicingSettings twoHand = CreateBaseTwoHand();
        twoHand.MinLeftHandNote = P(NoteStep.C, 1);
        twoHand.MaxLeftHandNote = P(NoteStep.B, 2);
        twoHand.MinRightHandNote = P(NoteStep.C, 3);
        twoHand.MaxRightHandNote = P(NoteStep.C, 4);
        twoHand.MinLeftHandVoices = 1;
        twoHand.MaxLeftHandVoices = 2;
        twoHand.MinRightHandVoices = 1;
        twoHand.MaxRightHandVoices = 2;
        twoHand.MaxTotalVoices = 4;

        return new VoicingProfile("Arranger Left Hand", oneHand, twoHand);
    }

    private static VoicingProfile CreateLeftHandCompact() {
        VoicingSettings oneHand = CreateBaseOneHand();
        oneHand.MinChordAreaNote = P(NoteStep.C, 1);
        oneHand.MaxChordAreaNote = P(NoteStep.C, 3);
        oneHand.ChordMaxKeyCount = 4;
        oneHand.PreferCompactInitialVoicing = true;

        TwoHandVoicingSettings twoHand = CreateBaseTwoHand();
        twoHand.MinLeftHandNote = P(NoteStep.C, 1);
        twoHand.MaxLeftHandNote = P(NoteStep.B, 2);
        twoHand.MinRightHandNote = P(NoteStep.C, 3);
        twoHand.MaxRightHandNote = P(NoteStep.C, 4);
        twoHand.MinLeftHandVoices = 1;
        twoHand.MaxLeftHandVoices = 2;
        twoHand.MinRightHandVoices = 1;
        twoHand.MaxRightHandVoices = 2;
        twoHand.MaxTotalVoices = 4;

        return new VoicingProfile("Left Hand Compact", oneHand, twoHand);
    }

    private static VoicingProfile CreateLeftHandOpen() {
        VoicingSettings oneHand = CreateBaseOneHand();
        oneHand.MinChordAreaNote = P(NoteStep.C, 1);
        oneHand.MaxChordAreaNote = P(NoteStep.C, 3);
        oneHand.ChordMaxKeyCount = 5;
        oneHand.PreferCompactInitialVoicing = false;

        TwoHandVoicingSettings twoHand = CreateBaseTwoHand();
        twoHand.MinLeftHandNote = P(NoteStep.C, 1);
        twoHand.MaxLeftHandNote = P(NoteStep.B, 2);
        twoHand.MinRightHandNote = P(NoteStep.C, 3);
        twoHand.MaxRightHandNote = P(NoteStep.C, 5);
        twoHand.MinLeftHandVoices = 1;
        twoHand.MaxLeftHandVoices = 2;
        twoHand.MinRightHandVoices = 1;
        twoHand.MaxRightHandVoices = 3;
        twoHand.MaxTotalVoices = 5;

        return new VoicingProfile("Left Hand Open", oneHand, twoHand);
    }

    private static VoicingProfile CreatePiano() {
        VoicingSettings oneHand = CreateBaseOneHand();
        oneHand.MinChordAreaNote = P(NoteStep.C, 1);
        oneHand.MaxChordAreaNote = P(NoteStep.C, 4);
        oneHand.ChordMaxKeyCount = 5;
        oneHand.PreferCompactInitialVoicing = true;

        TwoHandVoicingSettings twoHand = CreateBaseTwoHand();
        twoHand.MinLeftHandNote = P(NoteStep.C, 1);
        twoHand.MaxLeftHandNote = P(NoteStep.B, 2);
        twoHand.MinRightHandNote = P(NoteStep.C, 3);
        twoHand.MaxRightHandNote = P(NoteStep.C, 5);
        twoHand.MinLeftHandVoices = 1;
        twoHand.MaxLeftHandVoices = 3;
        twoHand.MinRightHandVoices = 1;
        twoHand.MaxRightHandVoices = 4;
        twoHand.MaxTotalVoices = 5;

        return new VoicingProfile("Piano", oneHand, twoHand);
    }

    private static VoicingProfile CreateTwoHandsBalanced() {
        VoicingSettings oneHand = CreateBaseOneHand();
        oneHand.MinChordAreaNote = P(NoteStep.C, 1);
        oneHand.MaxChordAreaNote = P(NoteStep.C, 4);
        oneHand.ChordMaxKeyCount = 5;
        oneHand.PreferCompactInitialVoicing = true;

        TwoHandVoicingSettings twoHand = CreateBaseTwoHand();
        twoHand.MinLeftHandNote = P(NoteStep.C, 1);
        twoHand.MaxLeftHandNote = P(NoteStep.B, 2);
        twoHand.MinRightHandNote = P(NoteStep.C, 3);
        twoHand.MaxRightHandNote = P(NoteStep.C, 5);
        twoHand.MinLeftHandVoices = 1;
        twoHand.MaxLeftHandVoices = 2;
        twoHand.MinRightHandVoices = 1;
        twoHand.MaxRightHandVoices = 3;
        twoHand.MaxTotalVoices = 5;

        return new VoicingProfile("Two Hands Balanced", oneHand, twoHand);
    }

    private static VoicingSettings CreateBaseOneHand() {
        return new() {
            MaxVoiceMovement = 11,
            CommonNoteWeight = 100,
            MovementWeight = 1,
            BassMovementWeight = 3,
            UserPreferenceWeight = 75,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenExtensions,
            PreferCompactInitialVoicing = true,
        };
    }

    private static TwoHandVoicingSettings CreateBaseTwoHand() {
        return new() {
            MaxVoiceMovement = 11,
            CommonNoteWeight = 100,
            MovementWeight = 1,
            BassMovementWeight = 3,
            UserPreferenceWeight = 75,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenExtensions,
        };
    }

    private static Pitch P(NoteStep note, int octave) {
        return new(note, Alteration.neutral, octave);
    }
}
