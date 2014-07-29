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
    /// Encloses a type in an immutable construct.
    /// </summary>
    /// <typeparam name="T">The type to enclose.</typeparam>
    public class Immutable<T> where T : new()
    {
        /// <summary>
        /// An instance of the enclosed immutable data type.
        /// </summary>
        private T self;

        /// <summary>
        /// An internal caching class that holds a cache of immutable accessor delegates.
        /// </summary>
        /// <typeparam name="TOutput">The immutable type for this cache.</typeparam>
        /// <typeparam name="TValue">The property type for this accessor.</typeparam>
        public static class Accessor<TValue>
        {
            /// <summary>
            /// Holds a dictionary of possible delegates for caching. Because a given
            /// immutable type may have multiple properties of the same type, the delegates
            /// must be differentiated here by MemberInfo.
            /// </summary>
            public static readonly ConcurrentDictionary<MemberInfo, Func<T, TValue, T>> AccessorDelegates = new ConcurrentDictionary<MemberInfo, Func<T, TValue, T>>();
        }

        /// <summary>
        /// An internal caching class that holds cached delegates for cloning Immutables.
        /// </summary>
        /// <typeparam name="TOutput">The type of Immutable to clone.</typeparam>
        public static Func<T, T> CloneDelegate;

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
        /// <param name="self"></param>
        private Immutable(T self)
        {
            this.self = self;
        }

        /// <summary>
        /// Creates a new instance of an Immutable using a stuffed enclosed type.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        internal static Immutable<T> Create(T self)
        {
            if (CloneDelegate == null)
            {
                CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return new Immutable<T>(CloneDelegate(self));
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
            if(CloneDelegate == null)
            {
                CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return CloneDelegate(self);
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
            return ImmutableBuilder<T>.Create(this.self);
        }
    }
}
