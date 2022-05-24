using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    /// <summary>
    /// Privdes a service to configure a serialization service
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Sets an indentation configuration for the serialization process
        /// </summary>
        /// <param name="value">Indentation mode</param>
        void SetSerializerIndentation(Indentation value);
    }
}
