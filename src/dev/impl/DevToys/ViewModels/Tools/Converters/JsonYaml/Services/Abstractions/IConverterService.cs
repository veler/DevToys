using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.ViewModels.Tools.Converters.JsonYaml.Services;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    /// <summary>
    /// Provides a service to convert from/to a <typeparamref name="T"/> object using a custom format
    /// </summary>
    /// <typeparam name="T">Type of the object to work with</typeparam>
    public interface IConverterService<T, M> where M : class
    {
        /// <summary>
        /// Custom metadata for the converter
        /// </summary>
        M GeneratorDisplay { get; }
        /// <summary>
        /// Converts a <typeparamref name="T"/> object into a standard representation using a custom format
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A standard representation language object deserialized using a custom format</returns>
        object Read(T input);
        /// <summary>
        /// Converts from a standard representation object into a <typeparamref name="T"/> object using a custom format
        /// </summary>
        /// <param name="_object">A standard representation object to be serialized</param>
        /// <returns>A representation of <paramref name="_object"/> into a <typeparamref name="T"/> using a custom format</returns>
        T Write(object _object);
    }
}
