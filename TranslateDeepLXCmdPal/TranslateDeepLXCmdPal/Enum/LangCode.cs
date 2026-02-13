using System;

namespace TranslateDeepLXCmdPal.Enums
{
    public class LangCode
    {
        public enum Code
        {
            AR,      // Arabic
            BG,      // Bulgarian
            CS,      // Czech
            DA,      // Danish
            DE,      // German
            EL,      // Greek
            EN,      // English (unspecified variant for backward compatibility; please select EN-GB or EN-US instead)
            ENGB,    // English (British)
            ENUS,    // English (American)
            ES,      // Spanish
            ET,      // Estonian
            FI,      // Finnish
            FR,      // French
            HU,      // Hungarian
            ID,      // Indonesian
            IT,      // Italian
            JA,      // Japanese
            KO,      // Korean
            LT,      // Lithuanian
            LV,      // Latvian
            NB,      // Norwegian Bokmål
            NL,      // Dutch
            PL,      // Polish
            PT,      // Portuguese (unspecified variant for backward compatibility; please select PT-BR or PT-PT instead)
            PTBR,    // Portuguese (Brazilian)
            PTPT,    // Portuguese (all Portuguese varieties excluding Brazilian Portuguese)
            RO,      // Romanian
            RU,      // Russian
            SK,      // Slovak
            SL,      // Slovenian
            SV,      // Swedish
            TR,      // Turkish
            UK,      // Ukrainian
            ZH,      // Chinese (simplified)
            UNK,     // Unknown
        }

        public static Code Parse(int code)
        {
            if (code >= 0 && code < (int)Code.UNK)
            {
                return (Code)code;
            }
            return Code.UNK;
        }

        public static Code Parse(string codeString)
        {
            switch (codeString)
            {
                case "gb":
                case "GB":
                case "EN-GB":
                case "EN_GB":
                    return Code.ENGB;

                case "us":
                case "US":
                case "EN-US":
                case "EN_US":
                    return Code.ENUS;

                case "br":
                case "BR":
                case "PT-BR":
                case "PT_BR":
                    return Code.PTBR;

                case "pt":
                case "PT":
                case "PT-PT":
                case "PT_PT":
                    return Code.PTPT;
                default:
                    try
                    {
                        return System.Enum.Parse<Code>(codeString.ToUpperInvariant());
                    }
                    catch (Exception)
                    {
                        return Code.UNK;
                    }
            }
        }

        public static string ToString(Code code)
        {
            switch (code)
            {
                case Code.ENGB:
                    return "EN-GB";
                case Code.ENUS:
                    return "EN-US";
                case Code.PTBR:
                    return "PT-BR";
                case Code.PTPT:
                    return "PT-PT";
                default:
                    return code.ToString();
            }
        }
    }
}

