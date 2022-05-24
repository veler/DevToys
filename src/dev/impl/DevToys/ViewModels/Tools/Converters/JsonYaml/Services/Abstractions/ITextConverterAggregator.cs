using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    /// <summary>
    /// Privdes a service to serialize object from/to string format
    /// </summary>
    public interface ITextConverterAggregator : ITextFormatter, IConverterService<string, GeneratorLanguageDisplayPair>
    {
    }
}
