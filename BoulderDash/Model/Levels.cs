using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoulderDashGUI.Data;

namespace BoulderDashGUI.Model
{
    static class Levels
    {
        public static readonly KeyValuePair<GameObject[,], int>[] Instances;

        static Levels()
        {
            Instances = new KeyValuePair<GameObject[,], int>[2];
            var levelFiles = Directory.GetFiles(@"Data\Maps\");

            Instances = levelFiles.Select(t => LevelParserFromString.Instance.ParseFromFile(t)).ToArray();
        }


    }
}
