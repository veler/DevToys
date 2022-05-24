using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using DevToys.Models;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;
using System.Dynamic;
using DevToys.Helpers.JsonYaml.Core;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Converters
{
    [ServiceType(GeneratorLanguages.Json)]
    class JsonConverterService : ITextConverterAggregator
    {
        Formatting IndentFormat { get; set; }
        public GeneratorLanguageDisplayPair GeneratorDisplay { get; } = GeneratorLanguageDisplayPair.Json;
        char IndentChar { get; set; }
        int Indentation { get; set; }
        JsonSerializerSettings DeserializerCfg { get; set; } = new()
        {
            FloatParseHandling = FloatParseHandling.Decimal
        };

        public void SetSerializerIndentation(Indentation indentation)
        {
            switch (indentation)
            {
                case Models.Indentation.TwoSpaces:
                    IndentFormat = Formatting.Indented;
                    IndentChar = ' ';
                    Indentation = 2;
                    break;
                case Models.Indentation.FourSpaces:
                    IndentFormat = Formatting.Indented;
                    IndentChar = ' ';
                    Indentation = 4;
                    break;
                case Models.Indentation.Minified:
                    IndentFormat = Formatting.None;
                    break;
                case Models.Indentation.OneTab:
                    IndentFormat = Formatting.Indented;
                    IndentChar = '\t';
                    Indentation = 1;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public object Read(string input)
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(input, DeserializerCfg);
        }

        public string Write(object _object)
        {
            var stringBuilder = new StringBuilder();
            using var stringWriter = new StringWriter(stringBuilder);
            using var jsonTextWriter = new JsonTextWriter(stringWriter);

            jsonTextWriter.Formatting = IndentFormat;
            jsonTextWriter.IndentChar = IndentChar;
            jsonTextWriter.Indentation = Indentation;

            var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
            {
                Converters = { new DecimalJsonConverter() }
            });
            jsonSerializer.Serialize(jsonTextWriter, _object);

            return stringBuilder.ToString();
        }
    }
}
