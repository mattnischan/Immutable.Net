using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ImmutableNet
{
    /// <summary>
    /// Builds accessor an cloner strongly typed delegates for caching Immutable access.
    /// </summary>
    internal static class DelegateBuilder
    {
        /// <summary>
        /// Builds an accessor delegate for caching
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Func<T, TValue, T> BuildAccessorDelegate<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            MemberExpression assignTo;
            var body = expression.Body as UnaryExpression;
            if (body != null)
            {
                assignTo = body.Operand as MemberExpression;
            }
            else
            {
                //Check to see that a property was passed in our expression
                assignTo = expression.Body as MemberExpression;
            }

            if (assignTo == null)
            {
                throw new ArgumentException("Can only assign to a class member.");
            }

            //Set up our delegate input parameters
            var valueToAssignTo = Expression.Parameter(typeof(TValue), "value");
            var inputObject = Expression.Parameter(typeof(T), "inputObject");

            //Create the expression block
            var assignmentExpression = Expression.Block(new ParameterExpression[] { }, new Expression[]{

                //Assign the input value to the new object's property
                Expression.Assign(Expression.PropertyOrField(inputObject, assignTo.Member.Name), Expression.Convert(valueToAssignTo, ((PropertyInfo)assignTo.Member).PropertyType)),

                //Return the object
                inputObject
            });

            //Build our delegate and return it
            return Expression.Lambda<Func<T, TValue, T>>(assignmentExpression, new ParameterExpression[] { inputObject, valueToAssignTo }).Compile();
        }

        /// <summary>
        /// Builds a delegate to be used for cloning an enclosed type.
        /// </summary>
        /// <typeparam name="T">The type of to clone.</typeparam>
        /// <param name="obj">The instance to clone.</param>
        /// <returns>A delegate that clones an Immutable's self instance.</returns>
        public static Func<T, T> BuildCloner<T>()
        {
            var bindings = new List<MemberBinding>();

            var thisObject = Expression.Parameter(typeof(T), "obj");

            foreach (var property in typeof(T).GetProperties())
            {
                bindings.Add(Expression.Bind(property, Expression.Property(thisObject, property)));
            }
            Expression initializer = Expression.MemberInit(Expression.New(typeof(T)), bindings);
            return Expression.Lambda<Func<T, T>>(initializer, thisObject).Compile();
        }
    }
}
