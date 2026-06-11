using System.Net.Http;
using System.Text;
using System.Text.Json;
using GameLauncher.Models;

namespace GameLauncher.Services;

public class AiService
{
    private static readonly HttpClient _http = new();

    public static async Task<List<string>> FetchModelsAsync(AiSettings settings)
    {
        var models = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.Endpoint))
            return models;

        try
        {
            var modelsUrl = settings.Endpoint
                .Replace("/chat/completions", "/models")
                .Replace("/completions", "/models")
                .Replace("/messages", "/models");

            var httpReq = new HttpRequestMessage(HttpMethod.Get, modelsUrl);
            httpReq.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");

            var resp = await _http.SendAsync(httpReq);
            var respJson = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return models;

            using var doc = JsonDocument.Parse(respJson);

            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var item in data.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var id))
                    {
                        var modelId = id.GetString();
                        if (!string.IsNullOrEmpty(modelId))
                            models.Add(modelId);
                    }
                }
            }

            models.Sort();
        }
        catch
        {
        }

        return models;
    }

    public static async Task<(string? result, string? error)> SimplifyDescriptionAsync(AiSettings settings, string description)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.Endpoint))
            return (null, "请先配置 API 地址和 Key");

        try
        {
            var endpoint = settings.Endpoint;

            object request;
            if (endpoint.Contains("/messages"))
            {
                request = new
                {
                    model = settings.Model,
                    max_tokens = 2000,
                    system = "你是一个游戏简介简化助手。用户会给你一段游戏描述，请用简洁的中文概括为15字以内的一句话，只输出简化后的文本，不要任何解释。",
                    messages = new[]
                    {
                        new { role = "user", content = description }
                    }
                };
            }
            else
            {
                request = new
                {
                    model = settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = "你是一个游戏简介简化助手。用户会给你一段游戏描述，请用简洁的中文概括为15字以内的一句话，只输出简化后的文本，不要任何解释。" },
                        new { role = "user", content = description }
                    },
                    temperature = 0.3,
                    max_tokens = 2000
                };
            }

            var json = JsonSerializer.Serialize(request);
            var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpReq.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
            httpReq.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(httpReq);
            var respJson = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                try
                {
                    using var errDoc = JsonDocument.Parse(respJson);
                    if (errDoc.RootElement.TryGetProperty("error", out var err))
                    {
                        var msg = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                        return (null, $"API 错误 ({(int)resp.StatusCode}): {msg ?? respJson}");
                    }
                }
                catch { }
                return (null, $"API 错误 ({(int)resp.StatusCode}): {respJson}");
            }

            using var doc = JsonDocument.Parse(respJson);

            string? content = null;
            string? reasoningContent = null;
            string? finishReason = null;

            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("finish_reason", out var fr))
                    finishReason = fr.GetString();
                if (choice.TryGetProperty("message", out var message))
                {
                    if (message.TryGetProperty("content", out var c))
                        content = c.GetString();
                    if (message.TryGetProperty("reasoning_content", out var rc))
                        reasoningContent = rc.GetString();
                }
            }

            if (!string.IsNullOrEmpty(content))
                return (content.Trim(), null);

            if (finishReason == "length")
                return (null, "模型推理被截断，请增大 max_tokens 或换用非推理模型");

            return (null, $"模型未返回结果（仅输出了推理过程）");
        }
        catch (Exception ex)
        {
            return (null, $"请求失败: {ex.Message}");
        }
    }
}
