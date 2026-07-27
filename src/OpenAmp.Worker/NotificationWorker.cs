using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenAmp.Application.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OpenAmp.Worker;

public sealed partial class NotificationWorker(
    IOptions<RabbitMqOptions> options,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogConnectionFailure(logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
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
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<NotificationMessage>(
                    eventArgs.Body.Span);
                if (message is null)
                {
                    throw new JsonException("RabbitMQ poruka nema ispravan sadržaj.");
                }
                LogProcessed(
                    logger,
                    message.Type,
                    message.Recipient,
                    message.Title,
                    message.Body);
                await channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                LogInvalidMessage(logger, exception);
                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);
            }
        };
        await channel.BasicConsumeAsync(
            configuration.QueueName,
            autoAck: false,
            consumer,
            cancellationToken);
        LogStarted(logger, configuration.QueueName);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Error,
        Message = "RabbitMQ veza nije dostupna. Novi pokušaj za pet sekundi.")]
    private static partial void LogConnectionFailure(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Information,
        Message = "Obrađena {Type} notifikacija za {Recipient}: {Title} — {Body}")]
    private static partial void LogProcessed(
        ILogger logger,
        string type,
        string recipient,
        string title,
        string body);

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Error,
        Message = "Neispravna RabbitMQ poruka je odbačena.")]
    private static partial void LogInvalidMessage(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 2104,
        Level = LogLevel.Information,
        Message = "OpenAmp worker sluša red {QueueName}.")]
    private static partial void LogStarted(ILogger logger, string queueName);
}
