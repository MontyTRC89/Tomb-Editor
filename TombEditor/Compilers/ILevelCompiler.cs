using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TombEditor;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public abstract class ILevelCompiler
    {
        protected Level _level;
        protected string _dest;
        protected Editor _editor;
        protected BackgroundWorker _worker;

        public ILevelCompiler(Level level, string dest, BackgroundWorker bw = null)
        {
            _level = level;
            _dest = dest;
            _editor = Editor.Instance;
            _worker = bw;
        }
         
        public abstract bool CompileLevel();

        protected void ReportProgress(int percentage, string message)
        {
            if (_worker != null)
            {
                _worker.ReportProgress(percentage, message);
            }
        }
    }
}
