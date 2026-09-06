namespace OpenDXP.Application.Search;

/// <summary>
/// Swap in a real LLM embedding provider (OpenAI, Voyage AI - Anthropic's recommended partner,
/// since Claude has no embeddings endpoint of its own) by implementing this against their API.
/// </summary>
public interface IEmbeddingService
{
    float[] Embed(string text);
}
