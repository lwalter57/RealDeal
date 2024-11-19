using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Text.RegularExpressions;
using RealDeal.Models;
using Match = RealDeal.Models.Match;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configure the HTTP request pipeline.

var matchs = new List<RealDeal.Models.Match>
{
    new Match { Id = new Guid("a9f8b377-38f9-43e7-9d5d-25ff525ad03f") , Equipe1 = "�quipe A", Equipe2 = "�quipe B", DateMatch = DateTime.Now.AddDays(1)},
    new Match { Id = new Guid("9f1a719f-6452-44e9-b661-a6ec495d9005"), Equipe1 = "�quipe C", Equipe2 = "�quipe D", DateMatch = DateTime.Now.AddDays(2) },
    new Match { Id = new Guid("33fe08b0-9358-4437-ae58-52b9620c91bf"), Equipe1 = "�quipe E", Equipe2 = "�quipe F", DateMatch = DateTime.Now.AddDays(3)}
};

app.MapGet("/match/list", () =>
{
    return Results.Ok(matchs);
});

app.MapGet("match/{id:Guid}", (Guid id) =>
{
    var match = matchs.FirstOrDefault(m => m.Id == id);
    if (match == null)
    {
        return Results.NotFound(new { Message = $"Aucun match trouv� avec l'id {id}" });
    }
    return Results.Ok(match);
});


app.Run();


