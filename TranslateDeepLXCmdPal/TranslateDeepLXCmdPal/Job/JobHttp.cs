using TranslateDeepLXCmdPal.DTO;
using TranslateDeepLXCmdPal.Enums;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TranslateDeepLXCmdPal.Job
{
    public class JobHttp
    {
        private static HttpClient? httpClient;
        private static Uri? translateEndpoint;
        private static string? oldEndpoint;
        private const int MaxRetries = 3;
        private const int InitialRetryDelayMs = 1000;
        private static readonly Random Random = new Random();

        public static async Task<TranslationResultDTO> Translation(LangCode.Code targetCode, string text, string endpoint)
        {
            if (httpClient == null || !string.Equals(oldEndpoint, endpoint, StringComparison.Ordinal))
            {
                Init(endpoint);
            }

            if (httpClient != null && translateEndpoint != null)
            {
                int retryCount = 0;
                while (true)
                {
                    try
                    {
                        object body = new
                        {
                            text = new string[] { text },
                            target_lang = LangCode.ToString(targetCode)
                        };

                        using StringContent jsonContent = new(
                            JsonSerializer.Serialize(body),
                            Encoding.UTF8,
                            "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(translateEndpoint, jsonContent);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            if (responseString != null)
                            {
                                var result = JsonSerializer.Deserialize<DeepLXTranslationResponseDTO>(responseString);
                                if (result != null && !string.IsNullOrWhiteSpace(result.Data))
                                {
                                    return CreateSuccessResult(result, targetCode);
                                }

                                if (result != null && !string.IsNullOrWhiteSpace(result.Message))
                                {
                                    return CreateErrorResult(result.Message, targetCode);
                                }
                            }
                        }

                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
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

        private static TranslationResultDTO CreateSuccessResult(DeepLXTranslationResponseDTO result, LangCode.Code targetCode)
        {
            return new TranslationResultDTO
            {
                TargetLangCode = string.IsNullOrWhiteSpace(result.TargetLang)
                    ? LangCode.ToString(targetCode)
                    : result.TargetLang,
                Translations =
                [
                    new TranslationDTO
                    {
                        DetectedSourceLanguage = string.IsNullOrWhiteSpace(result.SourceLang)
                            ? LangCode.ToString(LangCode.Code.UNK)
                            : result.SourceLang,
                        Text = result.Data ?? string.Empty
                    }
                ]
            };
        }

        private static TranslationResultDTO CreateErrorResult(string message, LangCode.Code targetCode)
        {
            return new TranslationResultDTO
            {
                Translations =
                [
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
            translateEndpoint = NormalizeEndpoint(endpoint);

            if (translateEndpoint == null)
            {
                httpClient = null;
                return;
            }

            httpClient ??= new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
        }

        private static Uri? NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return null;
            }

            if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.AbsolutePath == "/")
            {
                var builder = new UriBuilder(uri)
                {
                    Path = "translate"
                };
                return builder.Uri;
            }

            return uri;
        }
    }
}


