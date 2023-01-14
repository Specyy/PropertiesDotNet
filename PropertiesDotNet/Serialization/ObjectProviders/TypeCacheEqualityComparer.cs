using PropertiesDotNet.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Comparer for registered types and constructors in <see cref="IObjectProvider"/>s.
    /// </summary>
    internal sealed class TypeCacheEqualityComparer : IEqualityComparer<Type>, IEqualityComparer<Type[]>
    {
        /// <inheritdoc/>
        public bool Equals(Type? x, Type? y) => x?.Equals(y) ?? y is null;

        /// <inheritdoc/>
        public bool Equals(Type[]? x, Type[]? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (!((x[i]?.Equals(y[i])) ?? y[i] is null))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public int GetHashCode(Type obj)
        {
#if NETSTANDARD1_3
            return obj.TypeHandle.GetHashCode();
#else
            return obj.UnderlyingSystemType.GetHashCode();
#endif
        }

        /// <inheritdoc/>
        public int GetHashCode(Type[] obj) => HashCodeHelper.GenerateHashCode(obj);
    }
}
