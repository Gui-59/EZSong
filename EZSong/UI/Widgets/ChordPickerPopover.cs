using EZSong.Enums;
using EZSong.Model;
using EZSong.UI.Widgets.Helpers;
using Gtk;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace EZSong.UI.Widgets {
    public class ChordPickerPopover : Popover {

        private ChordBeat? _chordBeat;

        private RhythmRationalDuration _maxDuration;

        public event Action<Object?>? ElementSelected;

        private Notebook _notebook;

        public ChordPickerPopover(Widget relativeTo) : base(relativeTo) { 
            BorderWidth = 6;
            WidthRequest = 300;

            _notebook = new Notebook {
                ShowTabs = true
            };

            BuildUI();

            Closed += (s, e) => {
                ElementSelected?.Invoke(null);
            };
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        public void Open(ChordBeat chordBeat, RhythmRationalDuration maxDuration) {
            _maxDuration = maxDuration;
            _chordBeat = chordBeat;

            Rebuild();

            ShowAll();
            Popup();
        }

        // =========================================================
        // UI
        // =========================================================

        private void BuildUI() {


            Add(_notebook);
        }

        private void Rebuild() {
            // Clear
            while (_notebook.NPages > 0) {
                _notebook.RemovePage(0);
            }

            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.C, Alteration.neutral), new Label("C"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.C, Alteration.sharp), new Label("C#/Db"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.D, Alteration.neutral), new Label("D"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.D, Alteration.sharp), new Label("D#/Eb"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.E, Alteration.neutral), new Label("E"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.F, Alteration.neutral), new Label("F"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.F, Alteration.sharp), new Label("F#/Gb"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.G, Alteration.neutral), new Label("G"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.G, Alteration.sharp), new Label("G#/Ab"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.A, Alteration.neutral), new Label("A"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.A, Alteration.sharp), new Label("A#/Bb"));
            _ = _notebook.AppendPage(BuildChordListPage(NoteStep.B, Alteration.neutral), new Label("B"));


        }

        private Widget BuildChordListPage(NoteStep root, Alteration rootAlteration) {
            FlowBox flow = CreateFlow();
            foreach (Model.Chord chord in GetAllChordsForRoot(root, rootAlteration)) {
                Button btn = CreateButton(chord.ToHumanString());
                btn.Clicked += (s, e) => {
                    ElementSelected?.Invoke(chord);
                    Popdown();
                };
                flow.Add(btn);
            }
            return flow;
        }

        private List<Model.Chord> GetAllChordsForRoot(NoteStep root, Alteration rootAlteration) {
            //TODO : gérer un choix de durée max pour les accords (ex: si on est sur une croche, ne pas proposer des accords de 4/4)
            List<Model.Chord> chords = new();

            RhythmRationalDuration minDuration = new(1, 8, 0);

            if (minDuration.IsGreaterOrEqual(_maxDuration)) {
                return chords;
            }

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.NoneOrMajor,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Minor,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Seventh,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorSeventh,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MajorSeventh,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.PowerChord,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Sixth,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorSixth,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.SuspendedSecond,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.SuspendedFourth,
                Duration = _maxDuration
            });

            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Diminished,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Augmented,
                Duration = _maxDuration
            }); 
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.DiminishedSeventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.AugmentedSeventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.AddSecond,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.AddFourth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.AddSixth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.AddNinth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Ninth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorNinth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MajorNinth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Eleventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorEleventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MajorEleventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.Thirteenth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorThirteenth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MajorThirteenth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorMajorSeventh,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.SixthNinth,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.SeventhMinusFive,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.SeventhPlusFive,
                Duration = _maxDuration
            });
            chords.Add(new Model.Chord {
                RootNote = root,
                RootNoteAlteration = rootAlteration,
                ChordType = ChordType.MinorSeventhFlatFive,
                Duration = _maxDuration
            });

            return chords;
        }

        private FlowBox CreateFlow() {
            return new FlowBox {
                MaxChildrenPerLine = 4,
                SelectionMode = SelectionMode.None,
                RowSpacing = 4,
                ColumnSpacing = 4
            };
        }

        private Button CreateButton(string label) {
            Button btn = new(label) {
                WidthRequest = 60,
                HeightRequest = 40,
            };

            return btn;
        }
    }
}
