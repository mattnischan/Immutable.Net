using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableNet
{
    internal static class DelegateCache<T>
    {
        /// <summary>
        /// A cached delegate that clones the enclosed type.
        /// </summary>
        public static Func<T, T, T> CloneDelegate;

        /// <summary>
        /// A cached delegate that creates a new enclosed type.
        /// </summary>
        public static Func<T> FactoryDelegate;
    }
}
