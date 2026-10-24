using EZSong.Tests;

VoicingEngineSelfTest.RunAll();
MidiHandTracksSelfTest.Run("D:\\MidiHandTracksSelfTest.mid");
TwoHandVoicingSelfTest.Run("D:\\TwoHandVoicingSelfTest.mid");
VoicingMidiExportV18SelfTest.RunAll();

Console.WriteLine("Tous les tests sont OK.");
