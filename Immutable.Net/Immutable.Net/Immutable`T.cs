using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ImmutableNet
{
    /// <summary>
    /// Encloses a type in an immutable construct.
    /// </summary>
    /// <typeparam name="T">The type to enclose.</typeparam>
    [Serializable]
    [XmlType]
    public class Immutable<T> : ISerializable where T : new()
    {
        /// <summary>
        /// An instance of the enclosed immutable data type.
        /// </summary>
        [XmlElement(Order=1)]
        private T self;

        /// <summary>
        /// An internal caching class that holds a cache of immutable accessor delegates.
        /// </summary>
        /// <typeparam name="TOutput">The immutable type for this cache.</typeparam>
        /// <typeparam name="TValue">The property type for this accessor.</typeparam>
        private static class Accessor<TValue>
        {
            /// <summary>
            /// Holds a dictionary of possible delegates for caching. Because a given
            /// immutable type may have multiple properties of the same type, the delegates
            /// must be differentiated here by MemberInfo.
            /// </summary>
            public static readonly ConcurrentDictionary<MemberInfo, Func<T, TValue, T>> AccessorDelegates = new ConcurrentDictionary<MemberInfo, Func<T, TValue, T>>();
        }

        /// <summary>
        /// A cached delegate that clones the enclosed type.
        /// </summary>
        private static Func<T, T> cloneDelegate;

        /// <summary>
        /// A cached delegate that serializes the enclosed type.
        /// </summary>
        private static Func<T, SerializationInfo, T> serializationDelegate;

        /// <summary>
        /// A cached delegate that deserializes the enclosed type.
        /// </summary>
        private static Func<T, SerializationInfo, T> deserializationDelegate;

        /// <summary>
        /// Creates an instance of an Immutable.
        /// </summary>
        public Immutable() 
        {
            self = new T();
        }

        /// <summary>
        /// A private constructor that allows a new Immutable to be built
        /// from a reference to the enclosed type.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        private Immutable(T self)
        {
            this.self = self;
        }

        /// <summary>
        /// A private constructor used by ISerializable to deserialize the Immutable.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization streaming context.</param>
        private Immutable(SerializationInfo info, StreamingContext context)
        {
            self = new T();
            if(deserializationDelegate == null)
            {
                deserializationDelegate = DelegateBuilder.BuildDeserializationDelegate<T>();
            }

            self = deserializationDelegate(self, info);
        }

        /// <summary>
        /// Creates a new instance of an Immutable using a stuffed enclosed type.
        /// </summary>
        /// <param name="self">The instance to create the Immutable from.</param>
        /// <returns>A new Immutable with a cloned enclosed instance.</returns>
        public static Immutable<T> Create(T self)
        {
            if (cloneDelegate == null)
            {
                cloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return new Immutable<T>(cloneDelegate(self));
        }

        /// <summary>
        /// Modifies and returns a copy of the modified Immutable.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to set.</typeparam>
        /// <param name="assignment">A member to assign the value to.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>A new modified Immutable instance.</returns>
        public Immutable<T> Modify<TValue>(Expression<Func<T, TValue>> assignment, TValue value)
        {
            MemberExpression assignTo = assignment.Body as MemberExpression;
            if (assignTo == null)
            {
                var body = assignment.Body as UnaryExpression;
                if (body != null)
                {
                    assignTo = body.Operand as MemberExpression;
                }
            }

            if (assignTo == null)
            {
                throw new ArgumentException("Can only assign to a class member.");
            }
            else
            {
                Func<T, TValue, T> accessor;
                if(!Accessor<TValue>.AccessorDelegates.TryGetValue(assignTo.Member, out accessor))
                {
                    accessor = DelegateBuilder.BuildAccessorDelegate(assignment);
                    Accessor<TValue>.AccessorDelegates.AddOrUpdate(assignTo.Member, accessor, (key, item) => accessor);
                }

                return new Immutable<T>(accessor(Clone(), value));
            }
        }

        /// <summary>
        /// Clones an enclosed immutable type.
        /// </summary>
        /// <returns>A copy of the enclosed type.</returns>
        private T Clone()
        {
            if(cloneDelegate == null)
            {
                cloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return cloneDelegate(self);
        }

        /// <summary>
        /// Gets a value from the Immutable.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value to return.</typeparam>
        /// <param name="accessor">A lambda containing the member to return.</param>
        /// <returns>A value from the provided member.</returns>
        public TReturn Get<TReturn>(Func<T, TReturn> accessor)
        {
            return accessor(self);
        }

        /// <summary>
        /// Creates a builder instance of the Immutable.
        /// </summary>
        /// <returns>A builder instance.</returns>
        public ImmutableBuilder<T> ToBuilder()
        {
            return ImmutableBuilder<T>.Create(Clone());
        }

        /// <summary>
        /// Provides serialization data for ISerializable.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization streaming context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(serializationDelegate == null)
            {
                serializationDelegate = DelegateBuilder.BuildSerializationDelegate<T>();
            }

            serializationDelegate(self, info);
        }
    }
}
