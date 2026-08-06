using System.Text.RegularExpressions;

namespace OpenDXP.Application.Tagging;

/// <summary>
/// Rule-based auto-tagging: frequency-count significant words after stop-word filtering. No
/// LLM/ML involved - a real implementation would call a language model or NER service here;
/// this is a structurally-equivalent stand-in (same event-driven trigger, same output shape)
/// that needs no API key or external dependency.
/// </summary>
public static partial class KeywordTagger
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "is", "of", "to", "and", "in", "on", "for", "with", "this", "that", "it",
        "as", "are", "was", "were", "be", "by", "or", "at", "from", "your", "you", "we", "our", "us",
        "not", "but", "have", "has", "had", "will", "can", "all", "any", "its", "their", "they",
    };

    public static IReadOnlyList<string> ExtractTags(string plainText, int maxTags = 5)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return [];
        }

        var words = WordPattern()
            .Matches(plainText.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(w => w.Length > 2 && !StopWords.Contains(w));

        return words
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Take(maxTags)
            .Select(g => g.Key)
            .ToList();
    }

    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex WordPattern();
}
