namespace ImmutableNet
{
    /// <summary>
    /// A mutable container of the immutable enclosed type that can be used
    /// to build an Immutable.
    /// </summary>
    public static class ImmutableBuilder
    {
        /// <summary>
        /// Creates a new Immutable builder with the supplied enclosed type instance.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        /// <returns>A new ImmutableBuilder instance.</returns>
        public static ImmutableBuilder<T> Create<T>(T self) where T : new()
        {
            return ImmutableBuilder<T>.Create(self);
        }
    }
}
