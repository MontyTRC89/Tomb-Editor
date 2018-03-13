using System.Collections;
using System.Collections.Generic;

namespace TombEditor
{
    public static class ListExtensions
    {
        public struct ListCast<CollectionT, SourceT, DestinationT> : IList<DestinationT> where CollectionT : IList<SourceT> where DestinationT : class, SourceT
        {
            private readonly CollectionT _baseList;

            public ListCast(CollectionT baseCollection)
            {
                _baseList = baseCollection;
            }

            public int Count => _baseList.Count;

            public bool IsReadOnly => _baseList.IsReadOnly;

            public DestinationT this[int index]
            {
                get { return (DestinationT)_baseList[index]; }
                set { _baseList[index] = value; }
            }

            public int IndexOf(DestinationT item)
            {
                return _baseList.IndexOf(item);
            }

            public void Insert(int index, DestinationT item)
            {
                _baseList.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _baseList.RemoveAt(index);
            }

            public void Add(DestinationT item)
            {
                _baseList.Add(item);
            }

            public void Clear()
            {
                _baseList.Clear();
            }

            public bool Contains(DestinationT item)
            {
                return _baseList.Contains(item);
            }

            public void CopyTo(DestinationT[] array, int arrayIndex)
            {
                foreach (SourceT item in _baseList)
                    array[arrayIndex++] = (DestinationT)item;
            }

            public bool Remove(DestinationT item)
            {
                return _baseList.Remove(item);
            }

            public IEnumerator<DestinationT> GetEnumerator()
            {
                foreach (SourceT item in _baseList)
                    yield return (DestinationT)item;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static IList<DestinationT> Cast<SourceT, DestinationT>(this IList<SourceT> SourceList) where DestinationT : class, SourceT
        {
            return new ListCast<IList<SourceT>, SourceT, DestinationT>(SourceList);
        }
    }
}
