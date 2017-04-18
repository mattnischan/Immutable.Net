using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static Func<T, TValue, T> BuildAccessorDelegate<T, TValue>(Expression<Func<T, TValue>> expression)
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
            BlockExpression assignmentExpression;

            if (assignTo.Member is FieldInfo)
            {           
                //Create the expression block
                assignmentExpression = Expression.Block(new ParameterExpression[] { }, new Expression[]{

                //Assign the input value to the new object's property
                Expression.Assign(Expression.PropertyOrField(inputObject, assignTo.Member.Name), Expression.Convert(valueToAssignTo, ((FieldInfo)assignTo.Member).FieldType)),

                //Return the object
                inputObject
                });
            }
            else
            {
                var setter = ((PropertyInfo)assignTo.Member).GetSetMethod(true);
                assignmentExpression = Expression.Block(new ParameterExpression[] { }, new Expression[]{

                //Assign the input value to the new object's property using the setter method.
                Expression.Call(inputObject, setter, Expression.Convert(valueToAssignTo, ((PropertyInfo)assignTo.Member).PropertyType)),

                //Return the object
                inputObject
                });
            }

            //Build our delegate and return it
            return Expression.Lambda<Func<T, TValue, T>>(assignmentExpression, new ParameterExpression[] { inputObject, valueToAssignTo }).Compile();
        }

        /// <summary>
        /// Builds a delegate to be used for cloning an enclosed type.
        /// </summary>
        /// <typeparam name="T">The type of to clone.</typeparam>
        /// <returns>A delegate that clones an Immutable's self instance.</returns>
        internal static Func<T, T, T> BuildCloner<T>()
        {
            var thisObject = Expression.Parameter(typeof(T), "obj");
            var newObject = Expression.Parameter(typeof(T), "new");
            var assignmentExpressions = new List<Expression>();

            //Getting all fields works for properties as well, as it includes the backing fields.
            foreach(var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                assignmentExpressions.Add(Expression.Assign(Expression.MakeMemberAccess(newObject, field), Expression.MakeMemberAccess(thisObject, field)));
            }

            var block = Expression.Block(new Expression[] {
                Expression.Block(assignmentExpressions),
                newObject
            });

            return Expression.Lambda<Func<T, T, T>>(block, new ParameterExpression[] { newObject, thisObject }).Compile();
        }

        /// <summary>
        /// Builds a factory to be used for creating an enclosed type.
        /// </summary>
        /// <typeparam name="T">The enclosed type.</typeparam>
        /// <returns>A delegate that creates a new instance of an enclosed type.</returns>
        internal static Func<T> BuildFactory<T>()
        {
            return Expression.Lambda<Func<T>>(Expression.New(typeof(T)), new ParameterExpression[] { }).Compile();
        }
    }
}
