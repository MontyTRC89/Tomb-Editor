using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TombLib.Wad
{
    public static class ObjectNames
    {
        private static readonly Dictionary<uint, string> _moveableNames = new Dictionary<uint, string>();
        private static readonly Dictionary<uint, string> _staticNames = new Dictionary<uint, string>();

        static ObjectNames()
        {
            for (uint i = 0; i < 100; i++)
                _staticNames.Add(i, "Static Mesh #" + i);

            using (var reader = new StreamReader("Editor\\Objects.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    Debug.Assert(line != null);
                    string[] tokens = line.Split(';');
                    _moveableNames.Add(uint.Parse(tokens[0]), tokens[1]);
                }
            }
        }

        public static string GetMoveableName(uint movableId)
        {
            return _moveableNames.ContainsKey(movableId) ? _moveableNames[movableId] : "Unknown";
        }

        public static string GetStaticName(uint staticId)
        {
            return _staticNames.ContainsKey(staticId) ? _staticNames[staticId] : "Unknown";
        }
    }
}
