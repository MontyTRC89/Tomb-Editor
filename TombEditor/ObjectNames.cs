using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor
{
    public static class ObjectNames
    {
        private static Dictionary<int, string> _moveableNames = new Dictionary<int, string>();
        private static Dictionary<int, string> _staticNames = new Dictionary<int, string>();

        static ObjectNames()
        {
            for (int i = 0; i < 100; i++)
                _staticNames.Add(i, "Static Mesh #" + i);
            
            using (StreamReader reader = new StreamReader("Editor\\Objects.txt"))
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(';');
                    _moveableNames.Add(Int32.Parse(tokens[0]), tokens[1]);
                }
        }

        public static string GetMovableName(int MovableID)
        {
            return _moveableNames[MovableID];
        }
        public static string GetStaticName(int StaticID)
        {
            return _staticNames[StaticID];
        }
    }
}
