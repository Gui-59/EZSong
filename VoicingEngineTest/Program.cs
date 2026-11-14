using EZSong.Tests;

VoicingEngineSelfTest.RunAll();
MidiQuantizationSelfTest.RunAll();
MidiLyricsSelfTest.RunAll();
MidiHandTracksSelfTest.Run("D:\\MidiHandTracksSelfTest.mid");
TwoHandVoicingSelfTest.Run("D:\\TwoHandVoicingSelfTest.mid");

Console.WriteLine("Tous les tests sont OK.");
