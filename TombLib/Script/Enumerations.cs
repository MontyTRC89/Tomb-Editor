using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public enum Enumerations
    {
        None,
        PsxExtensions,
        PcExtensions,
        Language,
        Options,
        Title,
        Level
    }

    public enum ScriptCompilers
    {
        TRLELegacy = 0,
        TRLENew = 1,
        NGCenter = 2,
        TR5New = 3,
        TR5Main = 4
    }
}
