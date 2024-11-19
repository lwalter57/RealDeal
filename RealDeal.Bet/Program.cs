using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RealDeal;
using RealDeal.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<RealDealDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    var secret = "SecretBetApi";
    if (!context.Request.Headers.TryGetValue("X-Api-Secret", out var providedSecret) || providedSecret != secret)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Forbidden");
        return;
    }
    await next();
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RealDealDbContext>();
    context.Database.EnsureCreated();
}

async Task<IChannel> InitRabbitMQ()
{

    var factory = new ConnectionFactory { HostName = "localhost" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(queue: "bet", durable: false, exclusive: false, autoDelete: false,
        arguments: null);
    return channel;
}

var channel = await InitRabbitMQ();



app.MapPost("/bet", async ([FromBody] Bett bet, [FromServices] RealDealDbContext context) =>
{
    context.Bets.Add(bet);
    await context.SaveChangesAsync();

    const string message = "Votre pari a été pris en compte";
    var body = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "bet", body: body);

    return Results.Created($"/bet/{bet.Id}", bet);
});

app.MapGet("/bet/list", ([FromServices] RealDealDbContext context) =>
{
    var result = context.Bets;
    return Results.Ok(result);
});

app.MapGet("/bet/{id:guid}", ([FromRoute] Guid id, [FromServices] RealDealDbContext context) =>
{
    var result = context.Bets.Where(b => b.Id == id).FirstOrDefault();
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapPut("/bet", ([FromBody] Bett bet, [FromServices] RealDealDbContext context) =>
{
    var result = context.Bets
        .Where(b => b.Id == bet.Id)
        .ExecuteUpdate(b =>
            b.SetProperty(be => be.Status, bet.Status)
            .SetProperty(be => be.Rating, bet.Rating)
            .SetProperty(be => be.Type, bet.Type)
            .SetProperty(be => be.Amount, bet.Amount)
            .SetProperty(be => be.Gain, bet.Gain));
    return result > 0 ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/bet/{id:guid}", ([FromRoute] Guid id, [FromServices] RealDealDbContext context) =>
{
    var result = context.Bets.Where(b => b.Id == id).ExecuteDelete();
    return result > 0 ? Results.NoContent() : Results.NotFound();
});

app.Run();
