using System;
using System.Collections.Generic;
using System.IO;

namespace TombLib.Wad
{
    public static class ObjectNames
    {
        private static Dictionary<uint, string> _moveableNames = new Dictionary<uint, string>();
        private static Dictionary<uint, string> _staticNames = new Dictionary<uint, string>();

        static ObjectNames()
        {
            for (uint i = 0; i < 100; i++)
                _staticNames.Add(i, "Static Mesh #" + i);

            using (StreamReader reader = new StreamReader("Editor\\Objects.txt"))
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(';');
                    _moveableNames.Add(uint.Parse(tokens[0]), tokens[1]);
                }
        }

        public static string GetMoveableName(uint MovableID)
        {
            if (_moveableNames.ContainsKey(MovableID))
                return _moveableNames[MovableID];
            else
                return "Unkown";
        }

        public static string GetStaticName(uint StaticID)
        {
            if (_staticNames.ContainsKey(StaticID))
                return _staticNames[StaticID];
            else
                return "Unkown";
        }

        public static Dictionary<uint, string> MoveablesNames
        {
            get
            {
                return _moveableNames;
            }
        }

        public static Dictionary<uint, string> StaticNames
        {
            get
            {
                return _staticNames;
            }
        }
    }
}
