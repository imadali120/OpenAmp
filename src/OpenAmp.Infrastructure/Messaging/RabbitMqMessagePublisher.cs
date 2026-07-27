using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAmp.Application.Messaging;
using RabbitMQ.Client;

namespace OpenAmp.Infrastructure.Messaging;

public sealed partial class RabbitMqMessagePublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqMessagePublisher> logger) : IMessagePublisher
{
    public async Task PublishAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = configuration.HostName,
                Port = configuration.Port,
                UserName = configuration.UserName,
                Password = configuration.Password,
                AutomaticRecoveryEnabled = true
            };
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(
                configuration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = message.Id.ToString()
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: configuration.QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogPublishFailure(logger, exception, message.Id);
        }
    }

    [LoggerMessage(
        EventId = 1201,
        Level = LogLevel.Error,
        Message = "RabbitMQ poruka {MessageId} nije poslana. Poslovna operacija ostaje sačuvana.")]
    private static partial void LogPublishFailure(
        ILogger logger,
        Exception exception,
        Guid messageId);
}
