using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;
using EZSong;
using EZSong.UI;

class Program {

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    static void Main() {
        Application.Init();
        MainWindow win = new();
        win.StyleContext.AddClass("gtk-window");
        Application.Run();
    }
}