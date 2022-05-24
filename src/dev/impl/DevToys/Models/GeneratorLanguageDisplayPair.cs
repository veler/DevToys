using System;

namespace DevToys.Models
{
    public class GeneratorLanguageDisplayPair : IEquatable<GeneratorLanguageDisplayPair>
    {
        private static JsonYamlStrings Strings => LanguageManager.Instance.JsonYaml;

        public static readonly GeneratorLanguageDisplayPair Json = new(Strings.Json, GeneratorLanguages.Json);

        public static readonly GeneratorLanguageDisplayPair Yaml = new(Strings.Yaml, GeneratorLanguages.Yaml);

        public string DisplayName { get; }

        public GeneratorLanguages Value { get; }

        private GeneratorLanguageDisplayPair(string displayName, GeneratorLanguages value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(GeneratorLanguageDisplayPair other)
        {
            return other.Value == Value;
        }
    }
}
