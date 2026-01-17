using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;
using EZSong;
using EZSong.UI;

class Program {
    static void Main() {
        Application.Init();
        _ = new MainWindow();
        Application.Run();
    }
}