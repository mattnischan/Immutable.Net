using ImmutableNet;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImmutableDotNet.Serialization.Json
{
    /// <summary>
    /// A JsonConverter for specific Immutable types.
    /// </summary>
    /// <typeparam name="T">The type of the enclosed Immutable instance.</typeparam>
    public class ImmutableJsonConverter<T> : JsonConverter<Immutable<T>>
    {
        /// <summary>
        /// The cached enclosed type.
        /// </summary>
        private readonly Type _enclosedType = typeof(T);

        /// <summary>
        /// A cached accessor that gets the enclosed instance from the _self field.
        /// </summary>
        private readonly Func<Immutable<T>, T> _accessor = BuildAccessor();

        /// <summary>
        /// Reads an Immutable type from a JSON object.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="typeToConvert">The immutable type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A new Immutable instance.</returns>
        public override Immutable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = (T)JsonSerializer.Deserialize(ref reader, _enclosedType, options);
            return Immutable.Create(value);
        }

        /// <summary>
        /// Writes an Immutable type to a JSON object.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, Immutable<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, _accessor(value), options);
        }

        /// <summary>
        /// Builds an accessor delegate that gets the wrapped instance.
        /// </summary>
        /// <returns>A new accessor delegate.</returns>
        private static Func<Immutable<T>, T> BuildAccessor()
        {
            var selfField = (typeof(Immutable<T>)).GetField("_self", BindingFlags.Instance | BindingFlags.NonPublic);

            var immutable = Expression.Parameter(typeof(Immutable<T>), "immutable");
            return Expression.Lambda<Func<Immutable<T>, T>>(Expression.MakeMemberAccess(immutable, selfField), immutable).Compile();
        }
    }
}
