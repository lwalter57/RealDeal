using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RealDeal.Auth.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

async Task InitRabbitMQ()
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    using var connection = await factory.CreateConnectionAsync();
    using var betChannel = await connection.CreateChannelAsync();

    await betChannel.QueueDeclareAsync(queue: "bet", durable: false, exclusive: false, autoDelete: false,
        arguments: null);


    var consumer = new AsyncEventingBasicConsumer(betChannel);
    consumer.ReceivedAsync += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($" [x] Received {message}");
        return Task.CompletedTask;
    };

    await betChannel.BasicConsumeAsync("bet", autoAck: true, consumer: consumer);
}

await InitRabbitMQ();

app.MapPost("/notification", ([FromBody] User user, string message) =>
{
    Console.WriteLine($"{user.MailAdress} : {message}");
});

app.Run();

