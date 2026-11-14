using EZSong.MIDI;
using Melanchall.DryWetMidi.Core;
using System.Text;

namespace EZSong.Tests;

/// <summary>
/// Self-tests for MIDI text decoding: ASCII, UTF-8, BOMs and the
/// Windows-1252/Latin-1 fallback used for accented karaoke files.
/// </summary>
public static class MidiTextDecoderSelfTest {

    public static void RunAll() {
        TestAsciiIsDecoded();
        TestUtf8AccentsAreDecoded();
        TestUtf8BomIsRemoved();
        TestWindows1252Fallback();
        TestUtf16BomIsDecoded();
        TestInvalidUtf8FallsBackWithoutThrowing();
        TestEmptyPayloads();
        TestMidiFileReadUsesDecoder();
        Console.WriteLine("MidiTextDecoderSelfTest: ALL OK");
    }

    private static void TestAsciiIsDecoded() {
        string text = MidiTextDecoder.Decode(Encoding.ASCII.GetBytes("Hello world"));
        Expect(text == "Hello world", "ASCII text should be decoded as-is.");
    }

    private static void TestUtf8AccentsAreDecoded() {
        string text = MidiTextDecoder.Decode(Encoding.UTF8.GetBytes("Été à Noël, déjà vu"));
        Expect(text == "Été à Noël, déjà vu",
            "Strict UTF-8 should preserve accents and special characters.");
    }

    private static void TestUtf8BomIsRemoved() {
        byte[] payload = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes("Été"))
            .ToArray();

        string text = MidiTextDecoder.Decode(payload);
        Expect(text == "Été", "The UTF-8 BOM must not appear in the decoded text.");
    }

    private static void TestWindows1252Fallback() {
        // 0xE9 is 'é' in both Windows-1252 and Latin-1 but is not valid UTF-8.
        Expect(MidiTextDecoder.Decode(new byte[] { 0xE9, 0x74, 0xE9 }) == "été",
            "Latin-1 accented letters should be recovered.");
        Expect(MidiTextDecoder.Decode(new byte[] { 0xE7, 0x61 }) == "ça",
            "A cedilla should be recovered from the fallback encoding.");

        // Characters specific to Windows-1252 must not be lost either.
        Expect(MidiTextDecoder.Decode(new byte[] { 0x80 }) == "\u20AC",
            "The Windows-1252 euro sign should be decoded.");
        Expect(MidiTextDecoder.Decode(new byte[] { 0x92 }) == "\u2019",
            "The Windows-1252 right single quote should be decoded.");
    }

    private static void TestUtf16BomIsDecoded() {
        byte[] littleEndian = Encoding.Unicode.GetPreamble()
            .Concat(Encoding.Unicode.GetBytes("Été"))
            .ToArray();
        Expect(MidiTextDecoder.Decode(littleEndian) == "Été",
            "UTF-16 little-endian text with a BOM should be decoded.");

        byte[] bigEndian = Encoding.BigEndianUnicode.GetPreamble()
            .Concat(Encoding.BigEndianUnicode.GetBytes("Noël"))
            .ToArray();
        Expect(MidiTextDecoder.Decode(bigEndian) == "Noël",
            "UTF-16 big-endian text with a BOM should be decoded.");
    }

    private static void TestInvalidUtf8FallsBackWithoutThrowing() {
        string text = MidiTextDecoder.Decode(new byte[] { 0xC3, 0x28 });
        Expect(text == "Ã(", "Malformed UTF-8 should fall back to Windows-1252.");
    }

    private static void TestEmptyPayloads() {
        Expect(MidiTextDecoder.Decode(null) == string.Empty,
            "A null payload should decode to an empty string.");
        Expect(MidiTextDecoder.Decode(Array.Empty<byte>()) == string.Empty,
            "An empty payload should decode to an empty string.");
    }

    private static void TestMidiFileReadUsesDecoder() {
        // Build a minimal format 0 MIDI file whose text event uses Latin-1
        // bytes. Reading it with the import settings must return "Été", and
        // the lyric extractor must receive the decoded text.
        byte[] midiBytes = BuildTextEventMidiFile(new byte[] { 0xC9, 0x74, 0xE9 });

        using MemoryStream stream = new(midiBytes);
        MidiFile midi = MidiFile.Read(stream, MidiTextDecoder.CreateReadingSettings());
        TextEvent textEvent = midi.GetTrackChunks()
            .SelectMany(track => track.Events)
            .OfType<TextEvent>()
            .First();

        Expect(textEvent.Text == "Été",
            "MidiFile.Read should use the accent-aware decoder for text events.");
        Expect(MidiLyricsExtractor.Format(MidiLyricsExtractor.Extract(midi.GetTrackChunks())) == "Été",
            "Decoded accents must reach the lyric extractor.");
    }

    private static byte[] BuildTextEventMidiFile(byte[] textPayload) {
        List<byte> track = new() {
            0x00, 0xFF, 0x01, (byte)textPayload.Length
        };
        track.AddRange(textPayload);
        track.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 });

        List<byte> file = new();
        file.AddRange(Encoding.ASCII.GetBytes("MThd"));
        file.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x01, 0x00, 0x60 });
        file.AddRange(Encoding.ASCII.GetBytes("MTrk"));
        int length = track.Count;
        file.AddRange(new byte[] {
            (byte)(length >> 24),
            (byte)(length >> 16),
            (byte)(length >> 8),
            (byte)length
        });
        file.AddRange(track);
        return file.ToArray();
    }

    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
