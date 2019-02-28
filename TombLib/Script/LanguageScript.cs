using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public class LanguageScript
    {
        public List<string> GeneralStrings { get; private set; } = new List<string>();
        public List<string> PsxStrings { get; private set; } = new List<string>();
        public List<string> PcStrings { get; private set; } = new List<string>();

        public List<string> AllStrings
        {
            get
            {
                var result = new List<string>();
                result.AddRange(GeneralStrings);
                result.AddRange(PsxStrings);
                result.AddRange(PcStrings);
                return result;
            }
        }
    }
}
