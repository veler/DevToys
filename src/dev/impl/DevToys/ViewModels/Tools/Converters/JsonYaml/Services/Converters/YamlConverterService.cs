using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Helpers.JsonYaml.Core;
using DevToys.Models;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Converters
{
    [ServiceType(GeneratorLanguages.Yaml)]
    class YamlConverterService : ITextConverterAggregator
    {
        public GeneratorLanguageDisplayPair GeneratorDisplay { get; } = GeneratorLanguageDisplayPair.Yaml;
        int Indent { get; set; } = 2;

        public void SetSerializerIndentation(Indentation indent)
        {
            Indent = indent switch
            {
                Indentation.TwoSpaces => 2,
                Indentation.FourSpaces => 4,
                _ => Indent
            };
        }

        public object Read(string input)
        {
            using var stringReader = new StringReader(input);
            var deserializer = new DeserializerBuilder()
                .WithNodeTypeResolver(new DecimalYamlTypeResolver())
                .Build();
            return deserializer.Deserialize(stringReader);
        }

        public string Write(object input)
        {
            var serializer
                = Serializer.FromValueSerializer(
                    new SerializerBuilder().BuildValueSerializer(),
                    EmitterSettings.Default.WithBestIndent(Indent).WithIndentedSequences());

            return serializer.Serialize(input) ?? string.Empty;
        }
    }
}
