using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;

namespace EZSong.Serializable
{

    [Serializable]
    public class Song
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Comment { get; set; } = "";
        public List<MeasureData> Measures { get; set; } = new List<MeasureData>();

        public Song() { 
        }
    }

}
