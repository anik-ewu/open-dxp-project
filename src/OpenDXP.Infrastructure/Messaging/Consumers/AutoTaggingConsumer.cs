using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDXP.Application.Common.Messaging;
using OpenDXP.Application.Content;
using OpenDXP.Application.Content.Commands;
using OpenDXP.Application.Tagging;
using OpenDXP.Domain.Content;

namespace OpenDXP.Infrastructure.Messaging.Consumers;

/// <summary>Fifth consumer group reacting to PagePublished - rule-based tag extraction, no LLM.</summary>
public class AutoTaggingConsumer(
    IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<AutoTaggingConsumer> logger)
    : KafkaConsumerBackgroundService(scopeFactory, configuration, logger, "opendxp-auto-tagger", KafkaTopics.ContentEvents)
{
    protected override async Task HandleAsync(
        string eventType, string payloadJson, IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        if (eventType != nameof(PagePublishedEvent))
        {
            return;
        }

        var evt = JsonSerializer.Deserialize<PagePublishedEvent>(payloadJson)
                  ?? throw new InvalidOperationException("Could not deserialize PagePublishedEvent.");

        var plainText = BlockTextExtractor.ExtractPlainText(evt.BlocksJson);
        var tags = KeywordTagger.ExtractTags(plainText);

        var mediator = scopedProvider.GetRequiredService<ISender>();
        await mediator.Send(new SetPageTagsCommand(evt.PageId, tags), cancellationToken);
    }
}
