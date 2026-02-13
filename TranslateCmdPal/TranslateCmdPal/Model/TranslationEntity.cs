using System;
using System.Text.Json.Serialization;

namespace TranslateCmdPal.Model
{
    public class TranslationEntity
    {
        [JsonPropertyName(nameof(OriginalText))]
        public string OriginalText { get; set; } = string.Empty;

        [JsonPropertyName(nameof(TranslatedText))]
        public string TranslatedText { get; set; } = string.Empty;

        [JsonPropertyName(nameof(OriginalLangCode))]
        public string OriginalLangCode { get; set; } = string.Empty;

        [JsonPropertyName(nameof(TargetLangCode))]
        public string TargetLangCode { get; set; } = string.Empty;

        [JsonPropertyName(nameof(Timestamp))]
        public DateTime Timestamp { get; set; }
    }
}
