using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public struct ConditionallyOwned<T> : IDisposable where T : class, IDisposable
    {
        public T Value;
        public bool Destroy;

        public ConditionallyOwned(T value, bool destroy)
        {
            Value = value;
            Destroy = destroy;
        }

        public void Dispose()
        {
            if (Destroy)
                Value?.Dispose();
        }

        public static bool operator ==(ConditionallyOwned<T> first, ConditionallyOwned<T> second) => first.Value == second.Value;
        public static bool operator !=(ConditionallyOwned<T> first, ConditionallyOwned<T> second) => first.Value != second.Value;
        public static bool operator ==(T first, ConditionallyOwned<T> second) => first == second.Value;
        public static bool operator !=(T first, ConditionallyOwned<T> second) => first != second.Value;
        public static bool operator ==(ConditionallyOwned<T> first, T second) => first.Value == second;
        public static bool operator !=(ConditionallyOwned<T> first, T second) => first.Value != second;

        public static implicit operator T(ConditionallyOwned<T> value) => value.Value;
        public static implicit operator ConditionallyOwned<T>(T value) => new ConditionallyOwned<T>(value, false);

        public override string ToString() => Value.ToString();
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => object.Equals(Value, obj);
    }
}
