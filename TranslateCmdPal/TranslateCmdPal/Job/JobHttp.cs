using TranslateCmdPal.DTO;
using TranslateCmdPal.Enum;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TranslateCmdPal.Job
{
    public class JobHttp
    {
        private static HttpClient? httpClient;
        private static string? oldEndpoint;
        private const int MaxRetries = 3;
        private const int InitialRetryDelayMs = 1000;
        private static readonly Random Random = new Random();
        private const string DefaultDeepLXEndpoint = "http://127.0.0.1:1188/translate";

        public static async Task<TranslationResultDTO> Translation(LangCode.Code targetCode, string text, string endpoint)
        {
            if (httpClient == null || oldEndpoint != endpoint)
            {
                Init(endpoint);
            }

            if (httpClient != null)
            {
                int retryCount = 0;
                while (true)
                {
                    try
                    {
                        object body = new
                        {
                            text,
                            source_lang = "auto",
                            target_lang = LangCode.ToString(targetCode)
                        };

                        using StringContent jsonContent = new(
                            JsonSerializer.Serialize(body),
                            Encoding.UTF8,
                            "application/json"
                        );

                        HttpResponseMessage response = await httpClient.PostAsync(string.Empty, jsonContent);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            if (responseString != null)
                            {
                                var deepLxResult = JsonSerializer.Deserialize<DeepLXTranslationResultDTO>(responseString);
                                var result = ToTranslationResult(deepLxResult, targetCode);
                                if (result != null)
                                {
                                    return result;
                                }
                            }
                        }

                        if (response.StatusCode == HttpStatusCode.Forbidden ||
                            response.StatusCode == HttpStatusCode.Unauthorized ||
                            response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return CreateErrorResult(Properties.Resource.invalid_endpoint, targetCode);
                        }
                        else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            if (retryCount >= MaxRetries)
                            {
                                return CreateErrorResult(Properties.Resource.error_too_many_requests, targetCode);
                            }

                            int delayMs = InitialRetryDelayMs * (int)Math.Pow(2, retryCount);
                            await Task.Delay(delayMs);
                            retryCount++;
                            continue;
                        }

                        return CreateErrorResult(Properties.Resource.error_message_during_translation, targetCode);
                    }
                    catch (Exception ex)
                    {
                        if (retryCount >= MaxRetries)
                        {
                            return CreateErrorResult($"Error: {ex.Message}", targetCode);
                        }
                        int delayMs = CalculateDelayWithJitter(retryCount);
                        await Task.Delay(delayMs);
                        retryCount++;
                    }
                }
            }

            return CreateErrorResult(Properties.Resource.error_message_during_translation, targetCode);
        }

        private static TranslationResultDTO? ToTranslationResult(DeepLXTranslationResultDTO? deepLxResult, LangCode.Code targetCode)
        {
            if (deepLxResult == null || string.IsNullOrWhiteSpace(deepLxResult.Data))
            {
                return null;
            }

            return new TranslationResultDTO
            {
                TargetLangCode = string.IsNullOrWhiteSpace(deepLxResult.TargetLang)
                    ? LangCode.ToString(targetCode)
                    : deepLxResult.TargetLang,
                Translations =
                [
                    new TranslationDTO
                    {
                        DetectedSourceLanguage = string.IsNullOrWhiteSpace(deepLxResult.SourceLang)
                            ? LangCode.ToString(LangCode.Code.UNK)
                            : deepLxResult.SourceLang,
                        Text = deepLxResult.Data
                    }
                ]
            };
        }

        private static TranslationResultDTO CreateErrorResult(string message, LangCode.Code targetCode)
        {
            return new TranslationResultDTO
            {
                Translations = [
                    new TranslationDTO
                {
                    DetectedSourceLanguage = LangCode.ToString(LangCode.Code.UNK),
                    Text = message
                }
                ],
                TargetLangCode = LangCode.ToString(targetCode)
            };
        }

        private static int CalculateDelayWithJitter(int retryCount)
        {
            double baseDelay = 1000 * Math.Pow(2, retryCount);
            const double jitterPercentage = 0.23;
            double jitter = (Random.NextDouble() * 2 - 1) * jitterPercentage * baseDelay;
            return (int)Math.Min(baseDelay + jitter, 120000);
        }


        private static void Init(string endpoint)
        {
            oldEndpoint = endpoint;

            string finalEndpoint = string.IsNullOrWhiteSpace(endpoint)
                ? DefaultDeepLXEndpoint
                : endpoint.Trim();

            if (!Uri.TryCreate(finalEndpoint, UriKind.Absolute, out var endpointUri))
            {
                httpClient = null;
                return;
            }

            httpClient = new HttpClient
            {
                BaseAddress = endpointUri,
                Timeout = TimeSpan.FromMinutes(2)
            };
        }
    }
}
