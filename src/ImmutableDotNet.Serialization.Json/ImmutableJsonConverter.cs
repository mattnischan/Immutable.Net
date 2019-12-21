using ImmutableNet;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImmutableDotNet.Serialization.Json
{
    /// <summary>
    /// Creates JsonConverter instances for Immutable types.
    /// </summary>
    public class ImmutableJsonConverter : JsonConverterFactory
    {
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
        /// Creates an ImmutableJsonConverter for a specific Immutable type.
        /// </summary>
        /// <param name="typeToConvert">The Immutable type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A new JsonConverter instance.</returns>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var enclosedType = typeToConvert.GetGenericArguments()[0];
            var converterType = (typeof(ImmutableJsonConverter<>)).MakeGenericType(new[] { enclosedType });

            var converter = Activator.CreateInstance(converterType);
            return (JsonConverter)converter;
        }
    }
}
