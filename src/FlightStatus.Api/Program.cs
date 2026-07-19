using System.Text.Json.Serialization;
using FlightStatus.Application.Services;
using FlightStatus.Domain.Interfaces;
using FlightStatus.Infrastructure.Providers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Flight Status API",
        Version = "v1"
    });
});


builder.Services.AddSingleton<IFlightStatusProvider, AeroTrackProvider>();
builder.Services.AddSingleton<IFlightStatusProvider, QuickFlightProvider>();
builder.Services.AddSingleton<FlightStatusService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithOpenApi();

app.MapGet("/flights/status", async (string? flightNumber, string? date, FlightStatusService service, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
    {
        return Results.BadRequest(new { error = "flightNumber and date are required" });
    }

    var result = await service.GetFlightStatusAsync(flightNumber, date, cancellationToken);
    return Results.Ok(result);
})
.WithName("GetFlightStatus")
.WithOpenApi(operation =>
{
    operation.Summary = "Get flight status for a flight number and date";
    operation.Description = "Queries the stub providers, normalizes their results, and returns the merged flight status.";
    return operation;
});

app.Run();
