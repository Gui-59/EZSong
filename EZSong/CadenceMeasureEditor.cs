using Cairo;
using EZSong;
using EZSong.Serializable;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;

public class CadenceMeasureEditor : Bin {

    private const string _musicFontName = "Bravura";

    public RhythmDurationKind CurrentDuration {
        get; set;
    }
    = RhythmDurationKind.Quarter;

    private readonly DrawingArea _drawingArea;

    public TimeSignature TimeSignature {
        get; set;
    }
    public QuantizationMode Quantization {
        get; set;
    }
    public bool AllowTuplets {
        get; set;
    }

    public Cadence Cadence {
        get; private set;
    }

    public event EventHandler<Cadence>? CadenceChanged;
    public event EventHandler? ValidationStateChanged;

    public CadenceValidationState ValidationState {
        get; private set;
    }

    private const int _padding = 10;
    private const int _staffY = 40;
    private const int _staffHeight = 30;

    public CadenceMeasureEditor(TimeSignature timeSignature) {
        _drawingArea = new DrawingArea();
        _drawingArea.SetSizeRequest(500, 90);

        Add(_drawingArea);

        _drawingArea.Drawn += OnDraw;
        _drawingArea.ButtonPressEvent += OnButtonPress;
        _drawingArea.AddEvents((int)Gdk.EventMask.ButtonPressMask);

        TimeSignature = timeSignature;
        Cadence = new Cadence(TimeSignature, Quantization);

        ShowAll();
    }

    public void LoadCadence(Cadence cadence) {
        Cadence = cadence;
        Revalidate();
        QueueDraw();
    }

    public void Clear() {
        Cadence = new Cadence(TimeSignature, Quantization);
        Revalidate();
        QueueDraw();
    }

    private void Revalidate() {
        RhythmRationalDuration total = Cadence.GetTotalDuration();
        RhythmRationalDuration expected = TimeSignature.TotalDuration;

        if (total.Equals(expected)) {
            ValidationState = CadenceValidationState.Complete;
        } else if (total.Numerator * expected.Denominator >
                 expected.Numerator * total.Denominator) {
            ValidationState = CadenceValidationState.Overflow;
        } else {
            ValidationState = CadenceValidationState.Incomplete;
        }

        ValidationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnDraw(object o, DrawnArgs args) {
        Context cr = args.Cr;

        DrawBackground(cr);
        DrawStaff(cr);
        DrawTimeGrid(cr);
        DrawEvents(cr);
        DrawValidationOverlay(cr);
    }

    private void DrawBackground(Context cr) {
        cr.SetSourceRGB(1, 1, 1);
        cr.Paint();
    }

    private void DrawStaff(Context cr) {
        cr.SetSourceRGB(0, 0, 0);
        cr.LineWidth = 1;

        cr.MoveTo(_padding, _staffY);
        cr.LineTo(_drawingArea.AllocatedWidth - _padding, _staffY);
        cr.Stroke();
    }

    private void DrawTimeGrid(Context cr) {
        int beats = TimeSignature.Beats;
        double width = _drawingArea.AllocatedWidth - 2 * _padding;
        double beatWidth = width / beats;

        cr.SetSourceRGB(0.7, 0.7, 0.7);
        cr.LineWidth = 1;

        for (int i = 1; i < beats; i++) {
            double x = _padding + i * beatWidth;
            cr.MoveTo(x, _staffY - 20);
            cr.LineTo(x, _staffY + 20);
        }
        cr.Stroke();
    }

    private void DrawEvents(Context cr) {
        if (Cadence == null) {
            return;
        }

        cr.SelectFontFace(_musicFontName,
        FontSlant.Normal,
        FontWeight.Normal);

        cr.SetFontSize(32);

        RhythmRationalDuration measureDuration = TimeSignature.TotalDuration;

        double totalWidth = _drawingArea.AllocatedWidth - 2 * _padding;
        double currentX = _padding;

        foreach (RhythmEvent ev in Cadence.Events) {
            double ratio =
                (double)ev.Duration.Numerator * measureDuration.Denominator /
                ((double)ev.Duration.Denominator * measureDuration.Numerator);

            double eventWidth = totalWidth * ratio;

            string glyph = GetGlyphForEvent(ev);

            // centrage horizontal
            double x = currentX + eventWidth / 2 - 10;
            double y = _staffY + 10;

            cr.MoveTo(x, y);
            cr.SetSourceRGB(0, 0, 0);
            cr.ShowText(glyph);

            currentX += eventWidth;
        }
    }

    private string GetGlyphForEvent(RhythmEvent ev) {
        if (ev is NoteEvent note) {
            return note.DurationKind switch {
                RhythmDurationKind.Whole => SmuflGlyphs.NoteWhole,
                RhythmDurationKind.Half => SmuflGlyphs.NoteHalf,
                RhythmDurationKind.Quarter => SmuflGlyphs.NoteQuarter,
                RhythmDurationKind.Eighth => SmuflGlyphs.NoteEighth,
                RhythmDurationKind.Sixteenth => SmuflGlyphs.NoteSixteenth,
                _ => throw new NotSupportedException()
            };
        }

        if (ev is RestEvent rest) {
            return rest.DurationKind switch {
                RhythmDurationKind.Whole => SmuflGlyphs.RestWhole,
                RhythmDurationKind.Half => SmuflGlyphs.RestHalf,
                RhythmDurationKind.Quarter => SmuflGlyphs.RestQuarter,
                RhythmDurationKind.Eighth => SmuflGlyphs.RestEighth,
                RhythmDurationKind.Sixteenth => SmuflGlyphs.RestSixteenth,
                _ => throw new NotSupportedException()
            };
        }

        throw new NotSupportedException();
    }




    private void DrawValidationOverlay(Context cr) {
        if (ValidationState == CadenceValidationState.Complete) {
            cr.SetSourceRGBA(0, 1, 0, 0.1);
        } else if (ValidationState == CadenceValidationState.Incomplete) {
            cr.SetSourceRGBA(1, 0.6, 0, 0.1);
        } else {
            cr.SetSourceRGBA(1, 0, 0, 0.15);
        }

        cr.Rectangle(0, 0,
            _drawingArea.AllocatedWidth,
            _drawingArea.AllocatedHeight);
        cr.Fill();
    }

    private void OnButtonPress(object o, ButtonPressEventArgs args) {
        if (args.Event.Button != 1) {
            return;
        }

        RhythmEvent ev;

        if ((args.Event.State & Gdk.ModifierType.ShiftMask) != 0) {
            ev = new RestEvent(CurrentDuration);
        } else {
            ev = new NoteEvent(CurrentDuration);
        }

        Cadence.AddEvent(ev);
        Revalidate();
        CadenceChanged?.Invoke(this, Cadence);
        QueueDraw();
    }
}
