using OpenAmp.Application.Messaging;
using OpenAmp.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<NotificationWorker>();

var host = builder.Build();
host.Run();
