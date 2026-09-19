using EZSong.Model;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    internal class MeasuresEditorHeader : Box {

        private GlobalMeasuresEditor _globalMeasuresEditor;
        private Song _currentSong;

        private int _displayedStaffIndex;
        private Label _displayedStaffNumber;
        private Label _displayedStaffName;
        private bool _isPlaceHolder;

        public MeasuresEditorHeader(GlobalMeasuresEditor globalMeasuresEditor, Song currentSong, int displayedStaffIndex,  bool isPlaceHolder) : base(Orientation.Vertical, 0) {
            _globalMeasuresEditor = globalMeasuresEditor;
            _currentSong = currentSong; 
            _displayedStaffIndex = displayedStaffIndex;

            _isPlaceHolder = isPlaceHolder;

            _displayedStaffNumber = new Label("" + (displayedStaffIndex + 1));
            _displayedStaffName = new Label("?");

            InitializeComponent();
        }

        private void InitializeComponent() {

            Clear();

            WidthRequest = 200;
            _displayedStaffNumber = new Label("?");
            _displayedStaffNumber.StyleContext.AddClass("infoLabel");
            _displayedStaffName = new Label("?");
            _displayedStaffName.StyleContext.AddClass("infoLabel");

            if (_isPlaceHolder) {
                Label placeholderLabel = new("Créez une deuxième portée pour la voir ici");
                placeholderLabel.StyleContext.AddClass("titleLabel");
                PackStart(placeholderLabel, false, false, 0);
                return;
            }

            Label titleDisplayedStaff = new("Portée actuellement affichée :");
            titleDisplayedStaff.StyleContext.AddClass("titleLabel");

            PackStart(titleDisplayedStaff, false, false, 0);
            PackStart(_displayedStaffNumber, false, false, 0);
            PackStart(_displayedStaffName, false, false, 0);

            //Bouton de suppression de la portée
            Button deleteCurrentStaff = new();
            deleteCurrentStaff.Label = "Supprimer cette portée"; //TODO : icone à la place du texte
            deleteCurrentStaff.Clicked += (o, args) => {
                //TODO : demander confirmation avant de supprimer la portée
                throw new NotImplementedException(); //TODO : implémenter la suppression de la portée
            };
            PackStart(deleteCurrentStaff, false, false, 0);

            //Portée précédente
            Button gotToPreviousStaff = new();
            gotToPreviousStaff.Label = "🔺"; //TODO : icone à la place du texte
            gotToPreviousStaff.Clicked += (o, args) => {
                _globalMeasuresEditor.GoToPreviousStaff();
            };
            PackStart(gotToPreviousStaff, false, false, 0);

            //Portée suivante
            Button gotToNextStaff = new();
            gotToNextStaff.Label = "🔻"; //TODO : icone à la place du texte
            gotToNextStaff.Clicked += (o, args) => {
                _globalMeasuresEditor.GoToNextStaff();
            };
            PackStart(gotToNextStaff, false, false, 0);
        }

        private void Clear() {
            foreach (Widget child in Children) {
                Remove(child);
            }

        }

        public bool IsPlaceHolder {
            get {
                return _isPlaceHolder;
            }
            internal set {
                bool shouldReinitialize = _isPlaceHolder != value;
                _isPlaceHolder = value;
                if (shouldReinitialize) {
                    InitializeComponent();
                }
            }
        }

        internal void RefreshDisplayedStaff(int displayedStaffIndex) {

           

            _displayedStaffNumber.Text = (displayedStaffIndex + 1).ToString();
            _displayedStaffName.Text = _currentSong.SongSettings.StaffsSettings.Staffs[displayedStaffIndex].Name;
        }
    }
}
