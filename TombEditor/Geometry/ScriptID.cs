using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public class ScriptIdTable<T> where T : class
    {
        private T[] _table = new T[6000];

        private void SetInternal(T @object, uint? previousScriptId, uint? newScriptId)
        {
            if (previousScriptId.HasValue && newScriptId.HasValue && previousScriptId.Value.Equals(newScriptId.Value))
                return;

            // Check table state
            if (previousScriptId.HasValue)
                if (_table[previousScriptId.Value] != @object)
                    throw new ApplicationException("Global script id table is corrupted. This should never happen.");
            if (newScriptId.HasValue)
                if (_table[newScriptId.Value] != null)
                    throw new ApplicationException("Global scripting id table slot " + newScriptId.Value + " is already full!");

            // Update the table to reflect the changes
            if (previousScriptId.HasValue)
                _table[previousScriptId.Value] = null;
            if (newScriptId.HasValue)
                _table[newScriptId.Value] = @object;
        }

        public void Update(T @object, uint? previousScriptId, uint? newScriptId)
        {
            lock (_table)
                SetInternal(@object, previousScriptId, newScriptId);
        }

        public uint UpdateWithNewId(T @object, uint? previousScriptId)
        {
            lock (_table)
                for (uint i = 0; i < _table.Length; i++)
                    if (_table[i] == null)
                    {
                        SetInternal(@object, previousScriptId, i);
                        return i;
                    }
            throw new ApplicationException("No free slot found inside global scripting id table.");
        }

        public int Length => _table.Length;

        public T this[int index]
        {
            get
            {
                if (index >= _table.Length)
                    return null;
                return _table[index];
            }
        }
    }

    public interface IHasScriptID
    {
        uint? ScriptId { get; }
    }
}
