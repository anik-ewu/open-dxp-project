using System.Text.Json;

namespace OpenDXP.Application.Content;

/// <summary>Shared by search re-indexing and auto-tagging - both need the same plain text.</summary>
public static class BlockTextExtractor
{
    public static string ExtractPlainText(string blocksJson)
    {
        try
        {
            using var document = JsonDocument.Parse(blocksJson);
            var texts = new List<string>();
            foreach (var block in document.RootElement.EnumerateArray())
            {
                if (block.TryGetProperty("text", out var textProperty) && textProperty.ValueKind == JsonValueKind.String)
                {
                    texts.Add(textProperty.GetString()!);
                }
            }

            return string.Join(" ", texts);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
