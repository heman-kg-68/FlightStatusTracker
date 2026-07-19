using FlightStatus.Application.Services;
using FlightStatus.Domain.Interfaces;
using FlightStatus.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IFlightStatusProvider, AeroTrackProvider>();
builder.Services.AddSingleton<IFlightStatusProvider, QuickFlightProvider>();
builder.Services.AddSingleton<FlightStatusService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/flights/status", async (string? flightNumber, string? date, FlightStatusService service, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
    {
        return Results.BadRequest(new { error = "flightNumber and date are required" });
    }

    var result = await service.GetFlightStatusAsync(flightNumber, date, cancellationToken);
    return Results.Ok(result);
});

app.Run();
