using TranslateDeepLXCmdPal.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TranslateDeepLXCmdPal.Util
{
    [JsonSerializable(typeof(List<TranslationEntity>))]
    internal sealed partial class TranslationJsonSerializerContext : JsonSerializerContext
    {
    }
}

