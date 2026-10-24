using EZSong.Tests;

VoicingEngineSelfTest.RunAll();
MidiHandTracksSelfTest.Run("D:\\MidiHandTracksSelfTest.mid");
TwoHandVoicingSelfTest.Run("D:\\TwoHandVoicingSelfTest.mid");

Console.WriteLine("Tous les tests sont OK.");
