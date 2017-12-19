using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public class ScriptIdTable<T> where T : class, IHasScriptID
    {
        private T[] _table = new T[6000];

        private Exception SetInternal(T @object, uint? previousScriptId, uint? newScriptId)
        {
            if (previousScriptId.HasValue && newScriptId.HasValue && previousScriptId.Value.Equals(newScriptId.Value))
                return null;

            // Check table state
            if (previousScriptId.HasValue)
                if (_table[previousScriptId.Value] != @object)
                    return new ApplicationException("Global script id table is corrupted. This should never happen.");
            if (newScriptId.HasValue)
                if (_table[newScriptId.Value] != null)
                    return new ScriptIdCollisionException { ScriptId = newScriptId.Value, OldObject = _table[newScriptId.Value], NewObject = @object };

            // Update the table to reflect the changes
            if (previousScriptId.HasValue)
                _table[previousScriptId.Value] = null;
            if (newScriptId.HasValue)
                _table[newScriptId.Value] = @object;
            return null;
        }

        public void Update(T @object, uint? previousScriptId, uint? newScriptId)
        {
            lock (_table)
            {
                Exception exception = SetInternal(@object, previousScriptId, newScriptId);
                if (exception != null)
                    throw exception;
            }
        }

        public bool TryUpdate(T @object, uint? previousScriptId, uint? newScriptId)
        {
            lock (_table)
            {
                Exception exception = SetInternal(@object, previousScriptId, newScriptId);
                return exception == null;
            }
        }

        public uint UpdateWithNewId(T @object, uint? previousScriptId)
        {
            lock (_table)
                for (uint i = 0; i < _table.Length; i++)
                    if (_table[i] == null)
                    {
                        Exception exception = SetInternal(@object, previousScriptId, i);
                        if (exception != null)
                            throw exception;
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

        public void MergeFrom(ScriptIdTable<T> otherTable)
        {
            if (Length != otherTable.Length)
                throw new ArgumentException();

            for (int i = 0; i < otherTable.Length; ++i)
                if (otherTable._table[i] != null)
                    if (_table[i] == null)
                        _table[i] = otherTable._table[i];
                    else
                        otherTable._table[i].ScriptId = null;
            otherTable._table = null; // Prevent accidental accesses to the old table.
        }
    }

    public class ScriptIdCollisionException : ApplicationException
    {
        public uint ScriptId { get; set; }
        public IHasScriptID OldObject { get; set; }
        public IHasScriptID NewObject { get; set; }
        public override string Message => "Global scripting id table slot " + ScriptId + " is already full! " +
            "(Currently occupied by " + (OldObject.ToString() ?? "<NULL>") + ", needed for " + (NewObject.ToString() ?? "<NULL>") + ")";
    }

    public interface IHasScriptID
    {
        uint? ScriptId { get; set; }
        void AllocateNewScriptId();
        bool TrySetScriptId(uint? newScriptId);
    }
}
