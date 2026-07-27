namespace OpenAmp.Application.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "openamp";
    public string Password { get; set; } = "openamp";
    public string QueueName { get; set; } = "openamp.notifications";
}

public sealed record NotificationMessage(
    Guid Id,
    string Type,
    string Recipient,
    string Title,
    string Body,
    DateTime CreatedUtc);

public interface IMessagePublisher
{
    Task PublishAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);
}
