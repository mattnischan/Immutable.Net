using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ImmutableNet
{
    /// <summary>
    /// Encloses a type in an immutable construct.
    /// </summary>
    /// <typeparam name="T">The type to enclose.</typeparam>
    [DataContract]
    public class Immutable<T>
    {
        /// <summary>
        /// An instance of the enclosed immutable data type.
        /// </summary>
        [DataMember]
        private T _self;

        /// <summary>
        /// Creates an instance of an Immutable.
        /// </summary>
        public Immutable() 
        {
            if(DelegateCache<T>.FactoryDelegate == null)
            {
                DelegateCache<T>.FactoryDelegate = DelegateBuilder.BuildFactory<T>();
            }

            _self = DelegateCache<T>.FactoryDelegate();
        }

        /// <summary>
        /// A private constructor that allows a new Immutable to be built
        /// from a reference to the enclosed type.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        private Immutable(T self)
        {
            _self = self;
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

            if (DelegateCache<T>.FactoryDelegate == null)
            {
                DelegateCache<T>.FactoryDelegate = DelegateBuilder.BuildFactory<T>();
            }

            return new Immutable<T>(DelegateCache<T>.CloneDelegate(DelegateCache<T>.FactoryDelegate(), self));
        }

        /// <summary>
        /// Modifies and returns a copy of the modified Immutable.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to set.</typeparam>
        /// <param name="assignment">An action that assigns a new value to the immutable.</param>
        /// <returns>A new modified Immutable instance.</returns>
        public Immutable<T> Modify(Action<T> assignment)
        {
            if (DelegateCache<T>.CloneDelegate == null)
            {
                DelegateCache<T>.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            var immutable = new Immutable<T>();
            immutable._self = DelegateCache<T>.CloneDelegate(immutable._self, _self);
            assignment(immutable._self);

            return immutable;
        }

        /// <summary>
        /// Clones an enclosed immutable type.
        /// </summary>
        /// <returns>A copy of the enclosed type.</returns>
        private T Clone()
        {
            if (DelegateCache<T>.FactoryDelegate == null)
            {
                DelegateCache<T>.FactoryDelegate = DelegateBuilder.BuildFactory<T>();
            }

            var newItem = DelegateCache<T>.FactoryDelegate();

            if (DelegateCache<T>.CloneDelegate == null)
            {
                DelegateCache<T>.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return DelegateCache<T>.CloneDelegate(newItem, _self);
        }

        /// <summary>
        /// Gets a value from the Immutable.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value to return.</typeparam>
        /// <param name="accessor">A lambda expression containing the member to return.</param>
        /// <returns>A value from the provided member.</returns>
        public TReturn Get<TReturn>(Expression<Func<T, TReturn>> accessor)
        {
            return accessor.Compile()(_self);
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
        /// Returns a string that represents the current underlying object.
        /// </summary>
        /// <returns>
        /// A string that represents the current underlying object.
        /// </returns>
        public override string ToString()
        {
            return _self?.ToString() ?? base.ToString();
        }
    }
}
