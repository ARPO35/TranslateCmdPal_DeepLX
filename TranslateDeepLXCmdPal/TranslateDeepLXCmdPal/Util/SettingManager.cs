using TranslateDeepLXCmdPal.Enums;
using TranslateDeepLXCmdPal.Model;

using Microsoft.CommandPalette.Extensions.Toolkit;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TranslateDeepLXCmdPal.Util
{
    public class SettingsManager : JsonSettingsManager
    {
        private readonly string _historyPath;

        private static readonly string _namespace = "translate-cmdpal";
        private const string LegacyApiKeySettingName = "translate-cmdpal.DeepLAPIKey";

        private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

        private static readonly List<ChoiceSetSetting.Choice> _historyChoices =
        [
            new ChoiceSetSetting.Choice(Properties.Resource.history_none, Properties.Resource.history_none),
            new ChoiceSetSetting.Choice(Properties.Resource.history_1, Properties.Resource.history_1),
            new ChoiceSetSetting.Choice(Properties.Resource.history_5, Properties.Resource.history_5),
            new ChoiceSetSetting.Choice(Properties.Resource.history_10, Properties.Resource.history_10),
            new ChoiceSetSetting.Choice(Properties.Resource.history_20, Properties.Resource.history_20),
        ];

        private static readonly List<ChoiceSetSetting.Choice> _targetLangChoices =
            System.Enum.GetValues<LangCode.Code>()
            .Select(lang => new ChoiceSetSetting.Choice(
                LangCode.ToString(lang),
                ((int)lang).ToString(CultureInfo.InvariantCulture))
            ).ToList();

        private readonly ChoiceSetSetting _showHistory = new(
            Namespaced(nameof(ShowHistory)),
            Properties.Resource.plugin_show_history,
            Properties.Resource.plugin_show_history,
            _historyChoices);

        private readonly ChoiceSetSetting _targetLang = new(
            Namespaced(nameof(DefaultTargetLang)),
            Properties.Resource.plugin_default_target_language_code_title,
            Properties.Resource.plugin_default_target_language_code_description,
            _targetLangChoices);

        private readonly TextSetting _deepLXEndpoint = new(
            Namespaced(nameof(DeepLXEndpoint)),
            Properties.Resource.plugin_deepLX_endpoint,
            Properties.Resource.plugin_deepLX_endpoint,
            "http://127.0.0.1:1188/translate");

        public string ShowHistory => _showHistory.Value ?? string.Empty;

        public string DefaultTargetLang => _targetLang.Value ?? string.Empty;

        public string DeepLXEndpoint => _deepLXEndpoint.Value ?? string.Empty;


        internal static string SettingsJsonPath()
        {
            var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");
            Directory.CreateDirectory(directory);

            return Path.Combine(directory, "settings.json");
        }

        internal static string HistoryStateJsonPath()
        {
            var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");
            Directory.CreateDirectory(directory);

            return Path.Combine(directory, "translate_cmdpal_history.json");
        }

        public void SaveHistory(TranslationEntity historyItem)
        {
            if (historyItem == null)
            {
                return;
            }

            try
            {
                List<TranslationEntity> historyItems;

                if (File.Exists(_historyPath))
                {
                    var existingContent = File.ReadAllText(_historyPath);
                    historyItems = JsonSerializer.Deserialize(
                        existingContent,
                        TranslationJsonSerializerContext.Default.ListTranslationEntity) ?? [];
                }
                else
                {
                    historyItems = [];
                }

                historyItems.Add(historyItem);

                historyItems = historyItems.DistinctBy(x => x.TranslatedText).ToList();

                if (int.TryParse(ShowHistory, out var maxHistoryItems) && maxHistoryItems > 0)
                {
                    while (historyItems.Count > maxHistoryItems)
                    {
                        historyItems.RemoveAt(0);
                    }
                }

                var historyJson = JsonSerializer.Serialize(
                    historyItems,
                    TranslationJsonSerializerContext.Default.ListTranslationEntity);
                File.WriteAllText(_historyPath, historyJson);
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
            }
        }

        public List<ListItem> LoadHistory()
        {
            try
            {
                if (!File.Exists(_historyPath))
                {
                    return [];
                }

                var fileContent = File.ReadAllText(_historyPath);
                var historyItems = JsonSerializer.Deserialize(
                    fileContent,
                    TranslationJsonSerializerContext.Default.ListTranslationEntity) ?? [];

                var listItems = new List<ListItem>();
                foreach (var historyItem in historyItems)
                {
                    try
                    {
                        if (historyItem == null)
                        {
                            ExtensionHost.LogMessage(new LogMessage() { Message = "Null history item found, skipping." });
                            continue;
                        }

                        if (historyItem.OriginalText == null ||
                            historyItem.TranslatedText == null ||
                            historyItem.OriginalLangCode == null ||
                            historyItem.TargetLangCode == null)
                        {
                            ExtensionHost.LogMessage(new LogMessage() { Message = "History item contains null fields, skipping." });
                            continue;
                        }

                        listItems.Add(new ListItem(new ResultCopyCommand(historyItem, this))
                        {
                            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
                            Title = historyItem.TranslatedText,
                            Subtitle = historyItem.OriginalText,
                            Tags = [new Tag($"{historyItem.OriginalLangCode} -> {historyItem.TargetLangCode}")],
                        });
                    }
                    catch (Exception ex)
                    {
                        ExtensionHost.LogMessage(new LogMessage() { Message = $"Error processing history item: {ex}" });
                    }
                }

                listItems.Reverse();
                return listItems;
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
                return [];
            }
        }

        public SettingsManager()
        {
            FilePath = SettingsJsonPath();
            _historyPath = HistoryStateJsonPath();

            Settings.Add(_showHistory);
            Settings.Add(_targetLang);
            Settings.Add(_deepLXEndpoint);

            LoadSettings();
            MigrateLegacyApiSettingToEndpoint();

            Settings.SettingsChanged += (s, a) => SaveSettings();
        }

        private void MigrateLegacyApiSettingToEndpoint()
        {
            if (!string.IsNullOrWhiteSpace(DeepLXEndpoint))
            {
                return;
            }

            var legacyValue = TryReadLegacySettingValue(FilePath, LegacyApiKeySettingName);
            if (string.IsNullOrWhiteSpace(legacyValue))
            {
                return;
            }

            _deepLXEndpoint.Value = legacyValue.Trim();
            SaveSettings();
        }

        private static string? TryReadLegacySettingValue(string settingsPath, string settingName)
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    return null;
                }

                var content = File.ReadAllText(settingsPath);
                var root = JsonNode.Parse(content) as JsonObject;
                if (root == null)
                {
                    return null;
                }

                if (root.TryGetPropertyValue(settingName, out var settingNode) &&
                    settingNode is JsonValue jsonValue &&
                    jsonValue.TryGetValue<string>(out var value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = $"Failed to read legacy endpoint setting: {ex}" });
            }

            return null;
        }

        private void ClearHistory()
        {
            try
            {
                if (File.Exists(_historyPath))
                {
                    File.Delete(_historyPath);

                    ExtensionHost.LogMessage(new LogMessage() { Message = "History cleared successfully." });
                }
                else
                {
                    ExtensionHost.LogMessage(new LogMessage() { Message = "No history file found to clear." });
                }
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = $"Failed to clear history: {ex}" });
            }
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
            try
            {
                if (ShowHistory == Properties.Resource.history_none)
                {
                    ClearHistory();
                }
                else if (int.TryParse(ShowHistory, out var maxHistoryItems) && maxHistoryItems > 0)
                {
                    if (File.Exists(_historyPath))
                    {
                        var existingContent = File.ReadAllText(_historyPath);
                        var historyItems = JsonSerializer.Deserialize(
                            existingContent,
                            TranslationJsonSerializerContext.Default.ListTranslationEntity) ?? [];

                        historyItems = historyItems.DistinctBy(x => x.TranslatedText).ToList();

                        if (historyItems.Count > maxHistoryItems)
                        {
                            historyItems = historyItems.Skip(historyItems.Count - maxHistoryItems).ToList();

                            var trimmedHistoryJson = JsonSerializer.Serialize(
                                historyItems,
                                TranslationJsonSerializerContext.Default.ListTranslationEntity);
                            File.WriteAllText(_historyPath, trimmedHistoryJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtensionHost.LogMessage(new LogMessage() { Message = ex.ToString() });
            }
        }
    }
}


