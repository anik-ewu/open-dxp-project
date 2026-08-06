using System.Security.Cryptography;
using System.Text;
using OpenDXP.Application.Personalization.Dtos;

namespace OpenDXP.Application.Personalization;

/// <summary>
/// Pure selection logic, given already-loaded data - no I/O here, so it's trivially unit
/// testable without a database.
/// </summary>
public static class PersonalizationEngine
{
    public static VariantSelection Select(
        Guid pageId,
        string defaultBlocksJson,
        IReadOnlyList<PageVariantDto> variants,
        string? visitorId,
        string? segment)
    {
        var eligible = variants
            .Where(v => v.TargetSegment is null ||
                        (segment is not null && v.TargetSegment.Equals(segment, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(v => v.Priority)
            .ToList();

        if (eligible.Count == 0)
        {
            return DefaultSelection(defaultBlocksJson);
        }

        var hasTrafficSplit = eligible.Any(v => v.TrafficPercentage.HasValue);
        if (!hasTrafficSplit)
        {
            var winner = eligible[0];
            return new VariantSelection(winner.Id, winner.Name, winner.BlocksJson);
        }

        // Stable per-visitor bucket (0-99) so the same visitor always sees the same variant,
        // rather than a fresh coin flip on every request.
        var bucket = StableBucket(pageId, visitorId ?? Guid.NewGuid().ToString());
        var cumulative = 0;
        foreach (var variant in eligible)
        {
            cumulative += variant.TrafficPercentage ?? 0;
            if (bucket < cumulative)
            {
                return new VariantSelection(variant.Id, variant.Name, variant.BlocksJson);
            }
        }

        // Remaining traffic (100 - sum of splits) falls through to the default/control content.
        return DefaultSelection(defaultBlocksJson);
    }

    private static VariantSelection DefaultSelection(string defaultBlocksJson) => new(null, "default", defaultBlocksJson);

    private static int StableBucket(Guid pageId, string visitorId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{pageId}:{visitorId}"));
        var value = BitConverter.ToUInt32(hash, 0);
        return (int)(value % 100);
    }
}
