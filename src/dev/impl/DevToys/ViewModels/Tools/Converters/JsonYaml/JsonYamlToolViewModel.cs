#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.JsonYaml;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using YamlDotNet.Core;
using DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions;
using DevToys.Core;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(JsonYamlToolViewModel))]
    public sealed class JsonYamlToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Gets or sets the input code editor's language.
        /// </summary>
        private static readonly SettingDefinition<GeneratorLanguages> InputLanguage
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(InputLanguage)}",
                isRoaming: true,
                defaultValue: GeneratorLanguages.Json);

        /// <summary>
        /// Gets or sets the output code editor's language.
        /// </summary>
        private static readonly SettingDefinition<GeneratorLanguages> OutputLanguage
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(OutputLanguage)}",
                isRoaming: true,
                defaultValue: GeneratorLanguages.Yaml);

        /// <summary>
        /// The indentation to apply while converting.
        /// </summary>
        private static readonly SettingDefinition<Indentation> Indentation
            = new(
                name: $"{nameof(JsonYamlToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: Models.Indentation.TwoSpaces);

        /// <summary>
        /// Get a list of supported Indentation
        /// </summary>
        internal IReadOnlyList<IndentationDisplayPair> Indentations = new ObservableCollection<IndentationDisplayPair> {
            Models.IndentationDisplayPair.TwoSpaces,
            Models.IndentationDisplayPair.FourSpaces,
            Models.IndentationDisplayPair.OneTab,
            Models.IndentationDisplayPair.Minified,
        };

        /// <summary>
        /// Get a dictionary of supported languages
        /// </summary>
        internal readonly IReadOnlyList<GeneratorLanguageDisplayPair> Languages;

        private readonly IMarketingService _marketingService;
        private readonly Queue<string> _conversionQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _conversionInProgress;
        private string? _inputValue;
        private string? _outputValue;

        public Type View { get; } = typeof(JsonYamlToolPage);

        internal JsonYamlStrings Strings => LanguageManager.Instance.JsonYaml;

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal IndentationDisplayPair IndentationMode
        {
            get
            {
                Indentation settingsValue = SettingsProvider.GetSetting(Indentation);
                IndentationDisplayPair? indentation = Indentations.FirstOrDefault(x => x.Value == settingsValue);
                return indentation ?? IndentationDisplayPair.TwoSpaces;
            }
            set
            {
                if (IndentationMode != value)
                {
                    SettingsProvider.SetSetting(Indentation, value.Value);
                    Converters.ConfigureService(OutputValueLanguage.Value, (service) => service.SetSerializerIndentation(IndentationMode.Value));
                    OnPropertyChanged();
                    QueueConversion();
                }
            }
        }

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                SetProperty(ref _inputValue, value);
                QueueConversion();
            }
        }

        /// <summary>
        /// Gets or sets the input code editor's language.
        /// </summary>
        internal GeneratorLanguageDisplayPair InputValueLanguage
        {
            get
            {
                if (Languages is null)
                {
                    return GeneratorLanguageDisplayPair.Json;
                }
                GeneratorLanguages settingsValue = SettingsProvider.GetSetting(InputLanguage);
                GeneratorLanguageDisplayPair language = Languages.FirstOrDefault(v => settingsValue == v.Value);
                return language ?? GeneratorLanguageDisplayPair.Json;
            }
            set
            {
                if (InputValueLanguage != value)
                {
                    SettingsProvider.SetSetting(InputLanguage, value.Value);
                    OnPropertyChanged();
                    QueueConversion();
                }
            }
        }

        /// <summary>
        /// Gets or sets the output text.
        /// </summary>
        internal string? OutputValue
        {
            get => _outputValue;
            set => SetProperty(ref _outputValue, value);
        }

        /// <summary>
        /// Gets or sets the output code editor's language.
        /// </summary>
        internal GeneratorLanguageDisplayPair OutputValueLanguage
        {
            get
            {
                if (Languages is null)
                {
                    return GeneratorLanguageDisplayPair.Yaml;
                }
                GeneratorLanguages settingsValue = SettingsProvider.GetSetting(OutputLanguage);
                GeneratorLanguageDisplayPair language = Languages.FirstOrDefault(v => settingsValue == v.Value);
                return language ?? GeneratorLanguageDisplayPair.Yaml;
            }
            set
            {
                if (OutputValueLanguage != value)
                {
                    SettingsProvider.SetSetting(OutputLanguage, value.Value);
                    Converters.ConfigureService(value.Value, (service) => service.SetSerializerIndentation(IndentationMode.Value));
                    OnPropertyChanged();
                    QueueConversion();
                }
            }
        }

        internal ISettingsProvider SettingsProvider { get; }
        private readonly ITextFormatterContainer<GeneratorLanguages> Converters;

        [ImportingConstructor]
        public JsonYamlToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService, ITextFormatterContainer<GeneratorLanguages> converterService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;
            Converters = converterService;
            Converters.ConfigureService(OutputValueLanguage.Value, (service) => service.SetSerializerIndentation(IndentationMode.Value));
            Languages = new ObservableCollection<GeneratorLanguageDisplayPair>(Converters.GetGenerators());
        }

        private void QueueConversion()
        {
            _conversionQueue.Enqueue(InputValue ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_conversionInProgress)
            {
                return;
            }

            _conversionInProgress = true;

            await TaskScheduler.Default;

            while (_conversionQueue.TryDequeue(out string? text))
            {
                bool success = Convert(text, out string result);
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    OutputValue = result;

                    if (!string.IsNullOrWhiteSpace(result) && !_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _conversionInProgress = false;
        }

        private bool Convert(string input, out string output)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                output = string.Empty;
                return false;
            }

            try
            {
                return Converters.TryConvert(input, out output, InputValueLanguage.Value, OutputValueLanguage.Value);
            }
            catch (Exception ex) when (ex is SemanticErrorException or JsonReaderException)
            {
                output = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogFault($"{InputValueLanguage.Value} to {OutputValueLanguage.Value} Converter", ex);
                output = string.Empty;
            }

            return false;
        }
    }
}
