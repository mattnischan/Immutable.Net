using ImmutableNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ImmutableDotNet.Serialization.Newtonsoft
{
    /// <summary>
    /// A JSON.Net JsonConverter implementation for serializing Immutable instances.
    /// </summary>
    public class ImmutableJsonConverter : JsonConverter
    {
        /// <summary>
        /// A cache of delegates that access the internal wrapped object instance.
        /// </summary>
        private static ConcurrentDictionary<Type, Func<object, object>> _accessors = new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// A cache of delegates that create new Immutable instances of the key type.
        /// </summary>
        private static ConcurrentDictionary<Type, Func<object, object>> _factories = new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type to convert.</param>
        /// <returns>True if it can be converted, false otherwise.</returns>
        public override bool CanConvert(Type objectType)
        {

            return objectType.GetTypeInfo().IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Immutable<>);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var wrappedType = objectType.GetGenericArguments()[0];

            var instance = serializer.Deserialize(reader, wrappedType);
            var factory = _factories.GetOrAdd(wrappedType, type => BuildFactory(type));

            return factory(instance);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var immutableType = value.GetType();
            var accessor = _accessors.GetOrAdd(immutableType, type => BuildAccessor(type));

            var wrappedInstance = accessor(value);
            serializer.Serialize(writer, wrappedInstance);
        }

        /// <summary>
        /// Builds a factory delegate.
        /// </summary>
        /// <param name="instanceType">The wrapped type of the Immutable.</param>
        /// <returns>A new factory delegate.</returns>
        private Func<object, object> BuildFactory(Type instanceType)
        {
            var type = typeof(Immutable<>).MakeGenericType(instanceType);

            var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var constructor = constructors.First(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == instanceType);

            var wrappedInstance = Expression.Parameter(typeof(object), "wrappedInstance");
            return Expression.Lambda<Func<object, object>>(Expression.New(constructor, Expression.Convert(wrappedInstance, instanceType)), wrappedInstance).Compile();
        }

        /// <summary>
        /// Builds an accessor delegate that gets the wrapped instance.
        /// </summary>
        /// <param name="immutableType">The type of Immutable to get the instance from.</param>
        /// <returns>A new accessor delegate.</returns>
        private Func<object, object> BuildAccessor(Type immutableType)
        {
            var selfField = immutableType.GetField("_self", BindingFlags.Instance | BindingFlags.NonPublic);

            var immutable = Expression.Parameter(typeof(object), "immutable");
            return Expression.Lambda<Func<object, object>>(Expression.MakeMemberAccess(Expression.Convert(immutable, immutableType), selfField), immutable).Compile();
        }
    }
}
