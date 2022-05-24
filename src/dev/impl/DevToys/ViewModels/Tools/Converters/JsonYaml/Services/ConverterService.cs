#nullable enable
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;

namespace DevToys.ViewModels.Tools.JsonYaml.Services
{
    [Export(typeof(ITextFormatterContainer<GeneratorLanguages>))]
    [Shared]
    public class ConverterService : ITextFormatterContainer<GeneratorLanguages>
    {
        private readonly Lazy<Dictionary<GeneratorLanguages, ITextConverterAggregator>> _lazyServices;
        public Dictionary<GeneratorLanguages, ITextConverterAggregator> Services => _lazyServices.Value;

        public ConverterService()
        {
            _lazyServices = new Lazy<Dictionary<GeneratorLanguages, ITextConverterAggregator>>(Initialize);
        }

        public bool TryConvert(string input, out string output, GeneratorLanguages inputLang, GeneratorLanguages outputLang)
        {
            if (!(Services.ContainsKey(inputLang) && Services.ContainsKey(outputLang)) ||
                string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }
            object? generic = Services[inputLang].Read(input);
            output = Services[outputLang].Write(generic);
            return true;
        }

        public Dictionary<GeneratorLanguages, ITextConverterAggregator> Initialize()
        {
            var temp = new Dictionary<GeneratorLanguages, ITextConverterAggregator>();
            foreach (TypeInfo? type in Assembly.GetExecutingAssembly().DefinedTypes.Where(t => t.GetCustomAttribute<ServiceTypeAttribute>() is not null && t.GetInterface(nameof(ITextConverterAggregator)) is not null))
            {
                ServiceTypeAttribute? serviceAttribute = type.GetCustomAttribute<ServiceTypeAttribute>();
                if (Activator.CreateInstance(type) is not ITextConverterAggregator service)
                {
                    // TODO: Should throw an exception or ignore type?
                    continue;
                }
                temp.Add(serviceAttribute.ServiceId, service);
            }
            return temp;
        }

        public void ConfigureService(GeneratorLanguages service, Action<ITextFormatter> configuration)
        {
            if (Services.TryGetValue(service, out ITextConverterAggregator serv))
            {
                configuration(serv);
            }
        }

        public IEnumerable<GeneratorLanguageDisplayPair> GetGenerators()
        {
            return Services.Select(v => v.Value.GeneratorDisplay);
        }
    }
}
