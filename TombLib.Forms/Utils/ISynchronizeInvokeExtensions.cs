using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TombLib.Utils
{
    public static class ISynchronizeInvokeExtensions
    {
        public static void InvokeIfNecessary(this ISynchronizeInvoke synchronizationContext, Action action)
        {
            if (synchronizationContext.InvokeRequired)
                synchronizationContext.Invoke(action, null);
            else
                action();
        }

        public static void InvokeIfNecessary(this IWin32Window owner, Action action)
        {
            var synchronizationContext = owner as ISynchronizeInvoke;
            if (synchronizationContext != null)
                InvokeIfNecessary(synchronizationContext, action);
            else
                SynchronizationContext.Current.Send(unused => action(), null);
        }
    }
}
