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
    public class Immutable<T> : ISerializable where T : class
    {
        /// <summary>
        /// An instance of the enclosed immutable data type.
        /// </summary>
        [XmlElement(Order=1)]
        private T self;

        /// <summary>
        /// Creates an instance of an Immutable.
        /// </summary>
        public Immutable() 
        {
            if (DelegateCache<T>.CreationDelegate == null)
            {
                DelegateCache<T>.CreationDelegate = DelegateBuilder.BuildCreationDelegate<T>();
            }

            self = DelegateCache<T>.CreationDelegate.Invoke();
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
            if(DelegateCache<T>.CreationDelegate == null)
            {
                DelegateCache<T>.CreationDelegate = DelegateBuilder.BuildCreationDelegate<T>();
            }

            self = DelegateCache<T>.CreationDelegate.Invoke();

            if(DelegateCache<T>.DeserializationDelegate == null)
            {
                DelegateCache<T>.DeserializationDelegate = DelegateBuilder.BuildDeserializationDelegate<T>();
            }

            self = DelegateCache<T>.DeserializationDelegate(self, info);
        }

        /// <summary>
        /// Creates a new instance of an Immutable using a stuffed enclosed type.
        /// </summary>
        /// <param name="self">The instance to create the Immutable from.</param>
        /// <returns>A new Immutable with a cloned enclosed instance.</returns>
        public static Immutable<T> Create(T self)
        {
            if (DelegateCache<T>.CloneDelegate == null)
            {
                DelegateCache<T>.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return new Immutable<T>(DelegateCache<T>.CloneDelegate(self));
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
                if(!DelegateCache<T>.Accessor<TValue>.AccessorDelegates.TryGetValue(assignTo.Member, out accessor))
                {
                    accessor = DelegateBuilder.BuildAccessorDelegate(assignment);
                    DelegateCache<T>.Accessor<TValue>.AccessorDelegates.AddOrUpdate(assignTo.Member, accessor, (key, item) => accessor);
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
            if(DelegateCache<T>.CloneDelegate == null)
            {
                DelegateCache<T>.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return DelegateCache<T>.CloneDelegate(self);
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
            if(DelegateCache<T>.SerializationDelegate == null)
            {
                DelegateCache<T>.SerializationDelegate = DelegateBuilder.BuildSerializationDelegate<T>();
            }

            DelegateCache<T>.SerializationDelegate(self, info);
        }
    }
}
