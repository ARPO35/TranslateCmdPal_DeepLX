using System.Text.Json.Serialization;

namespace TranslateCmdPal.DTO
{
    public class DeepLXTranslationResultDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("source_lang")]
        public string? SourceLang { get; set; }

        [JsonPropertyName("target_lang")]
        public string? TargetLang { get; set; }
    }
}
