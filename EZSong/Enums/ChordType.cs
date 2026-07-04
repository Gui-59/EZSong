using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Enums {
    public enum ChordType {
        NoneOrMajor = 0, //X
        Minor = 1, //Xm
        Seventh = 3, //X7
        MinorSeventh = 4, //Xm7
        MajorSeventh = 5, //Xmaj7
        PowerChord = 6, //X5
        Sixth = 7, //X6
        MinorSixth = 8, //Xm6
        SuspendedSecond = 9, //Xsus2
        SuspendedFourth = 10, //Xsus4
        Diminished = 11, //Xdim
        Augmented = 12, //Xaug
        DiminishedSeventh = 13, //Xdim7
        AugmentedSeventh = 14, //Xaug7
        AddSecond = 15, //Xadd2
        AddFourth = 16, //Xadd4
        AddSixth = 17, //Xadd6
        AddNinth = 18, //Xadd9
        Ninth = 19, //X9
        MinorNinth = 20, //Xm9
        MajorNinth = 21, //Xmaj9
        Eleventh = 22, //X11
        MinorEleventh = 23, //Xm11
        MajorEleventh = 24,//Xmaj11
        Thirteenth = 25, //X13
        MinorThirteenth = 26, //Xm13
        MajorThirteenth = 27,//Xmaj13
        MinorMajorSeventh = 28, //Xm(maj7)        
        SixthNinth = 29, //X6/9     
        SeventhMinusFive = 30, //X7-5
        SeventhPlusFive = 31, //X7+5
        MinorSeventhFlatFive = 32, //Xm7b5
    }
}
