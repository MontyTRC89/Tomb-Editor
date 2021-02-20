using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public interface IScriptCompiler
    {
        bool CompileScripts(string srcPath, string dstPath);
    }
}
