using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenDXP.Application.Search;

/// <summary>
/// Feature hashing ("hashing trick"): a real, if less semantically rich, embedding technique
/// used in production systems (e.g. Vowpal Wabbit) that needs no external API or model - every
/// word hashes into one of N buckets, and the resulting bag-of-words vector is L2-normalized so
/// cosine similarity is meaningful. SHA256 (not string.GetHashCode, which is randomized per
/// process in .NET) keeps the mapping stable across restarts, so the same text always produces
/// the same vector.
/// </summary>
public partial class HashingEmbeddingService : IEmbeddingService
{
    public const int Dimensions = 128;

    public float[] Embed(string text)
    {
        var vector = new float[Dimensions];
        if (string.IsNullOrWhiteSpace(text))
        {
            return vector;
        }

        foreach (Match match in WordPattern().Matches(text.ToLowerInvariant()))
        {
            vector[BucketFor(match.Value)] += 1f;
        }

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0)
        {
            for (var i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }

    private static int BucketFor(string word)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(word));
        return (int)(BitConverter.ToUInt32(hash, 0) % Dimensions);
    }

    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex WordPattern();
}
