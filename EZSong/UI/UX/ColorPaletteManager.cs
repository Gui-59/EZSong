using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.UX {
    internal class ColorPaletteManager {

        private Color _frameBg = Color.LightPink;
        internal Color FrameBg {
            get { 
                return _frameBg; 
            }
        }

        private Color _frameFore = Color.Black;
        internal Color FrameFore {
            get { 
                return _frameFore; 
            }
        }

        private Color _frameSubtleLine = Color.LightGray;
        internal Color FrameSubtleLine {
            get {
                return _frameSubtleLine;
            }
        }

        private Color _windowBg = Color.Beige;
        internal Color WindowBg {
            get { 
                return _windowBg; 
            }
        }

        private Color _windowFore = Color.Black;
        internal Color WindowFore {
            get { 
                return _windowFore; 
            }
        }

        private Color _successBg = Color.Green;
        internal Color SuccessBg {
            get {
                return _successBg;
            }
        }

        private Color _successFore = Color.Black;
        internal Color SuccessFore {
            get {
                return _successFore;
            }
        }

        private Color _warningBg = Color.Yellow;
        internal Color WarningBg {
            get { 
                return _warningBg; 
            }
        }

        private Color _warningFore = Color.Black;
        internal Color WarningFore {
            get { 
                return _warningFore; 
            }
        }

        private Color _errorBg = Color.Red;
        internal Color ErrorBg {
            get { 
                return _errorBg; 
            }
        }
        private Color _errorFore = Color.White;
        internal Color ErrorFore {
            get { 
                return _errorFore; 
            }
        }

        private Color _cursorLine = Color.Brown;
        internal Color CursorLine {
            get { 
                return _cursorLine; 
            }
        }

        private Color _naturalEvenPianoKey = Color.Yellow;
        internal Color NaturalEvenPianoKey {
            get { 
                return _naturalEvenPianoKey; 
            }
        }
        private Color _alteredEvenPianoKey = Color.Fuchsia;
        internal Color AlteredEvenPianoKey {
            get { 
                return _alteredEvenPianoKey; 
            }
        }
        private Color _naturalOddPianoKey = Color.LightBlue;
        internal Color NaturalOddPianoKey {
            get { 
                return _naturalOddPianoKey; 
            }
        }
        private Color _alteredOddPianoKey = Color.Blue;
        internal Color AlteredOddPianoKey {
            get { 
                return _alteredOddPianoKey; 
            }
        }
        private Color _pianoNoteBg = Color.Red;
        internal Color PianoNoteBg {
            get { 
                return _pianoNoteBg; 
            }
        }





        public ColorPaletteManager() {
        }
    }
}
