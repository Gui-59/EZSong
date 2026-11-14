using Melanchall.DryWetMidi.Core;
using System.Text;

namespace EZSong.MIDI {

    /// <summary>
    /// Decodes MIDI text meta events (lyrics, track names, markers) whose
    /// encoding is not declared in the file.
    ///
    /// The MIDI specification does not define a text encoding. Most files are
    /// ASCII, but karaoke files regularly store accented characters as
    /// Windows-1252/Latin-1, while some modern tools write UTF-8, sometimes
    /// with a BOM. The decoder tries strict UTF-8 first, honours UTF-8 and
    /// UTF-16 BOMs, then falls back to Windows-1252 so that accents and
    /// special characters are never replaced by '?'.
    /// </summary>
    public static class MidiTextDecoder {

        private static readonly UTF8Encoding StrictUtf8 = new(false, true);
        private static readonly UTF8Encoding Utf8 = new(false, false);
        private static readonly UnicodeEncoding Utf16LittleEndian = new(false, false);
        private static readonly UnicodeEncoding Utf16BigEndian = new(true, false);

        /// <summary>
        /// Windows-1252 characters in the 0x80-0x9F range. Other bytes share
        /// their Latin-1 meaning, so a manual table avoids depending on the
        /// CodePagesEncodingProvider package.
        /// </summary>
        private static readonly char[] Windows1252SpecialCharacters = {
            '\u20AC', '\u0081', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020', '\u2021',
            '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u008D', '\u017D', '\u008F',
            '\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014',
            '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u009D', '\u017E', '\u0178'
        };

        /// <summary>
        /// Builds the reading settings used for MIDI/KAR imports so that every
        /// text meta event goes through <see cref="Decode(byte[])"/>.
        /// </summary>
        public static ReadingSettings CreateReadingSettings() {
            return new ReadingSettings {
                DecodeTextCallback = (bytes, _) => Decode(bytes)
            };
        }

        /// <summary>
        /// Decodes a raw MIDI text payload. UTF-8 is preferred because it is
        /// unambiguous; invalid sequences are read as Windows-1252, which also
        /// covers Latin-1 accented letters.
        /// </summary>
        public static string Decode(byte[]? bytes) {
            if (bytes is null || bytes.Length == 0) {
                return string.Empty;
            }

            if (TryDecodeWithBom(bytes, out string text)) {
                return CleanDecodedText(text);
            }

            try {
                return CleanDecodedText(StrictUtf8.GetString(bytes));
            } catch (DecoderFallbackException) {
                return CleanDecodedText(DecodeWindows1252(bytes));
            }
        }

        private static bool TryDecodeWithBom(byte[] bytes, out string text) {
            if (bytes.Length >= 3 &&
                bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) {
                text = Utf8.GetString(bytes, 3, bytes.Length - 3);
                return true;
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) {
                text = Utf16LittleEndian.GetString(bytes, 2, bytes.Length - 2);
                return true;
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) {
                text = Utf16BigEndian.GetString(bytes, 2, bytes.Length - 2);
                return true;
            }

            text = string.Empty;
            return false;
        }

        private static string DecodeWindows1252(byte[] bytes) {
            StringBuilder builder = new(bytes.Length);
            foreach (byte value in bytes) {
                if (value is >= 0x80 and <= 0x9F) {
                    builder.Append(Windows1252SpecialCharacters[value - 0x80]);
                } else {
                    builder.Append((char)value);
                }
            }
            return builder.ToString();
        }

        private static string CleanDecodedText(string text) {
            // Some files prepend a BOM as a character instead of raw bytes,
            // and pad text events with trailing NUL bytes.
            return text.TrimStart('\uFEFF').TrimEnd('\0');
        }
    }
}
