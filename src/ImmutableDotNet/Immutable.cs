namespace ImmutableNet
{
    /// <summary>
    /// Encloses a type in an immutable construct.
    /// </summary>
    public static class Immutable
    {
        /// <summary>
        /// Creates a new instance of an Immutable using a stuffed enclosed type.
        /// </summary>
        /// <typeparam name="T">The type to enclose.</typeparam>
        /// <param name="self">The instance to create the Immutable from.</param>
        /// <returns>A new Immutable with a cloned enclosed instance.</returns>
        public static Immutable<T> Create<T>(T self)
        {
            return Immutable<T>.Create(self);
        }
    }
}
