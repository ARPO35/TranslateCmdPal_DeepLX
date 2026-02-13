using System.Text.Json.Serialization;

namespace TranslateCmdPal.DTO
{
    public sealed class DeepLXTranslationResponseDTO
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("source_lang")]
        public string? SourceLang { get; set; }

        [JsonPropertyName("target_lang")]
        public string? TargetLang { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
