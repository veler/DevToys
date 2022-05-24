using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    /// <summary>
    /// Provides a service to be used as a container for different text formatters
    /// </summary>
    /// <typeparam name="T">Key type to difference between text formatters</typeparam>
    public interface ITextFormatterContainer<T>
    {
        /// <summary>
        /// Dictionary store for the conversion services
        /// </summary>
        Dictionary<T, ITextConverterAggregator> Services { get; }
        /// <summary>
        /// Convert from a <paramref name="input"/> value with <paramref name="inputLang"/> format into <paramref name="outputLang"/> format stored in <paramref name="output"/>
        /// </summary>
        /// <param name="input">Input text to be converted</param>
        /// <param name="output">Conversion result</param>
        /// <param name="inputLang">Input format</param>
        /// <param name="outputLang">Output format</param>
        /// <returns><c>true</c> if the conversion was sucessful, <c>false</c> otherwise</returns>
        bool TryConvert(string input, out string output, T inputLang, T outputLang);
        /// <summary>
        /// Gives an access to configure a specified service
        /// </summary>
        /// <param name="service">Service key to be selected to configure</param>
        /// <param name="configuration">Pass the selected service into an Action where it should be configured</param>
        void ConfigureService(T service, Action<ITextFormatter> configuration);
        /// <summary>
        /// Retrieves the metadata for the converters stored in the container
        /// </summary>
        /// <returns>A metadata enumerable of the different converters loaded</returns>
        IEnumerable<GeneratorLanguageDisplayPair> GetGenerators();
    }
}
