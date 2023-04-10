using System.Collections.Generic;

namespace SharpPadV2.Core.Actions {
    /// <summary>
    /// An object that stores context information, along other custom data
    /// </summary>
    public interface IHasDataContext {
        /// <summary>
        /// Returns all of the available context
        /// </summary>
        IEnumerable<object> Context { get; }

        /// <summary>
        /// Returns all of the custom data
        /// </summary>
        IEnumerable<(string, object)> CustomData { get; }

        /// <summary>
        /// Adds a new context object
        /// </summary>
        void AddContext(object context);

        /// <summary>
        /// Gets a context object of a specific type
        /// </summary>
        T GetContext<T>();

        /// <summary>
        /// Tries to get a context object of the specific type
        /// </summary>
        bool TryGetContext<T>(out T value);

        /// <summary>
        /// Sets custom data for the given key as the given value
        /// </summary>
        void SetCustomData(string key, object value);

        /// <summary>
        /// Gets custom data for the given key
        /// </summary>
        T GetCustomData<T>(string key);

        /// <summary>
        /// Tries to get custom data with the given key and of the given type
        /// </summary>
        bool TryGetCustomData<T>(string key, out T value);

        /// <summary>
        /// Merges the given <see cref="IHasDataContext"/>'s context and custom data into this context
        /// </summary>
        void Merge(IHasDataContext context);
    }
}