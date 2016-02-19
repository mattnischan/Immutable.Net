using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace ImmutableNet
{
    /// <summary>
    /// An internal caching class that holds delegates for manipulating immutables.
    /// </summary>
    /// <typeparam name="T">The enclosed type of the immutable</typeparam>
    internal static class DelegateCache<T>
    {
        /// <summary>
        /// An internal caching class that holds a cache of immutable accessor delegates.
        /// </summary>
        /// <typeparam name="TValue">The property type for this accessor.</typeparam>
        internal static class Accessor<TValue>
        {
            /// <summary>
            /// Holds a dictionary of possible delegates for caching. Because a given
            /// immutable type may have multiple properties of the same type, the delegates
            /// must be differentiated here by MemberInfo.
            /// </summary>
            internal static readonly ConcurrentDictionary<MemberInfo, Func<T, TValue, T>> AccessorDelegates = new ConcurrentDictionary<MemberInfo, Func<T, TValue, T>>();
        }

        /// <summary>
        /// A cached delegate that clones the enclosed type.
        /// </summary>
        internal static Func<T, T> CloneDelegate;

        /// <summary>
        /// A cached delegate that calls a parameterless constructor.
        /// </summary>
        internal static Func<T> CreationDelegate;

        /// <summary>
        /// A cached delegate that serializes the enclosed type.
        /// </summary>
        internal static Func<T, SerializationInfo, T> SerializationDelegate;

        /// <summary>
        /// A cached delegate that deserializes the enclosed type.
        /// </summary>
        internal static Func<T, SerializationInfo, T> DeserializationDelegate;
    }
}
