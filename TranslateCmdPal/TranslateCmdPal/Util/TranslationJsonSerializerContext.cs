using TranslateCmdPal.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TranslateCmdPal.Util
{
    [JsonSerializable(typeof(List<TranslationEntity>))]
    internal sealed partial class TranslationJsonSerializerContext : JsonSerializerContext
    {
    }
}
