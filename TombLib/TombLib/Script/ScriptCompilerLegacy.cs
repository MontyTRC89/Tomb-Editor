using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public class ScriptCompilerLegacy : IScriptCompiler
    {
        private string _compilerPath;

        public ScriptCompilerLegacy(string path)
        {
            _compilerPath = path;
        }

        public bool CompileScripts(string srcPath, string dstPath)
        {
            try
            {
                Process.Start(_compilerPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
