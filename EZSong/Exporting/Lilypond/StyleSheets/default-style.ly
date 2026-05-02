\version "2.24.0"

#(set-default-paper-size "a4")
#(set-global-staff-size 17)

\paper {
  top-margin = 15\mm
  bottom-margin = 15\mm
  left-margin = 15\mm
  right-margin = 18\mm
  
  oddFooterMarkup = \markup {
    \fill-line {
      \center-column {
        \with-color #black
          \bold \fontsize #1 \fromproperty #'header:title
        \with-color #black
          \fontsize #-1 "Page" \fromproperty #'page:page-number-string
      }
    }
  }

  evenFooterMarkup = \oddFooterMarkup

  print-page-number = ##t
}

\layout {
  indent = 0\mm
  \context {
    \Score
    \override SpacingSpanner.uniform-stretching = ##t
	\override VerticalAxisGroup.staff-staff-spacing.minimum-distance = #12
  }
  \context {
    \Staff
	\override StaffSymbol.thickness = #1.5
	\override TimeSignature.style = #'numbered
	\override TimeSignature.font-name = "Arial"
	\override TimeSignature.font-size = #3
	\override TimeSignature.color = #(x11-color 'black)
  }
  \context {
    \ChordNames
    chordChanges = ##t
    \override ChordName.font-size = #1
    \override ChordName.self-alignment-X = #CENTER
	\override VerticalAxisGroup.nonstaff-relatedstaff-spacing.minimum-distance = #6
	
	
	\override ChordName.font-name = "Arial"
    \override ChordName.font-size = #3
    \override ChordName.color = #(x11-color 'black)
    \override ChordName.font-series = #'bold
	
	
  }

  \context {
    \Voice
    \override TextScript.direction = #-1
    \override TextScript.extra-offset = #'(0 . -1)
  }
}
