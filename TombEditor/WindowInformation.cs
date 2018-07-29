using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor
{
    public class WindowInfo
    {
        public readonly Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    public class WindowInfoGetEvent : IEditorEvent
    {
        public WindowInfo Info = new WindowInfo();
    }

    public class WindowInfoSetEvent : IEditorEvent
    {
        public WindowInfo Info;
    }
}
