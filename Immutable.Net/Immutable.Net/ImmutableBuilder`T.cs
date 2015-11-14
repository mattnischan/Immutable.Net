using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ImmutableNet
{
    /// <summary>
    /// A mutable container of the immutable enclosed type that can be used
    /// to build an Immutable.
    /// </summary>
    /// <typeparam name="T">The enclosed type of the ImmutableBuilder.</typeparam>
    public class ImmutableBuilder<T> where T : new()
    {
        /// <summary>
        /// An instance of the enclosed type.
        /// </summary>
        private T self;

        /// <summary>
        /// Creates a new Immutable builder with the supplied enclosed type instance.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        /// <returns>A new ImmutableBuilder instance.</returns>
        public static ImmutableBuilder<T> Create(T self)
        {
            return new ImmutableBuilder<T>(self);
        }

        /// <summary>
        /// Creates an instance of an ImmutableBuilder.
        /// </summary>
        public ImmutableBuilder() 
        {
            this.self = new T();
        }

        /// <summary>
        /// A private constructor that allows a new Immutable to be built
        /// from a reference to the enclosed type.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        private ImmutableBuilder(T self)
        {
            this.self = self;
        }

        /// <summary>
        /// Modifies the enclosed instance of an ImmutableBuilder.
        /// </summary>
        /// <param name="accessor">The setter lambda.</param>
        public ImmutableBuilder<T> Modify(Action<T> accessor)
        {
            accessor(self);
            return this;
        }

        /// <summary>
        /// Freezes a builder into an Immutable.
        /// </summary>
        /// <returns>A new Immutable with the enclosed instance.</returns>
        public Immutable<T> ToImmutable()
        {
            return Immutable<T>.Create(self);
        }

        /// <summary>
        /// Gets a value from the ImmutableBuilder.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value to return.</typeparam>
        /// <param name="accessor">A lambda containing the member to return.</param>
        /// <returns>A value from the provided member.</returns>
        public TReturn Get<TReturn>(Func<T, TReturn> accessor)
        {
            return accessor(self);
        }
    }

}
