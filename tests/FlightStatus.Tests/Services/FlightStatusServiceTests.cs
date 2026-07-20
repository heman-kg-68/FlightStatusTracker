using FlightStatus.Application.Interfaces;
using FlightStatus.Application.Services;
using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Models;
using Xunit;

namespace FlightStatus.Tests.Services;

public class FlightStatusServiceTests
{
    private static readonly DateOnly TestDate = new(2026, 7, 19);

    [Theory]
    [InlineData("ON_TIME", FlightStatus.Domain.Enums.FlightStatus.OnTime)]
    [InlineData("SCHEDULED_ON_TIME", FlightStatus.Domain.Enums.FlightStatus.OnTime)]
    [InlineData("DELAY", FlightStatus.Domain.Enums.FlightStatus.Delayed)]
    [InlineData("Late", FlightStatus.Domain.Enums.FlightStatus.Delayed)]
    [InlineData("CANCELLED", FlightStatus.Domain.Enums.FlightStatus.Cancelled)]
    [InlineData("CANCELED", FlightStatus.Domain.Enums.FlightStatus.Cancelled)]
    [InlineData("DIVERTED", FlightStatus.Domain.Enums.FlightStatus.Diverted)]
    [InlineData("Divert", FlightStatus.Domain.Enums.FlightStatus.Diverted)]
    [InlineData("UNKNOWN", FlightStatus.Domain.Enums.FlightStatus.Unknown)]
    [InlineData(null, FlightStatus.Domain.Enums.FlightStatus.Unknown)]
    public async Task GetFlightStatusAsync_NormalizesProviderStatuses(string? providerStatus, FlightStatus.Domain.Enums.FlightStatus expectedStatus)
    {
        var providerData = providerStatus is null or "UNKNOWN"
            ? new ProviderFlightStatusData
            {
                FlightNumber = "SKY-1",
                Date = TestDate.ToString("yyyy-MM-dd"),
                Status = providerStatus,
                Message = "Provider response",
                ScheduledDepartureUtc = null,
                ActualDepartureUtc = null,
                LastUpdatedUtc = DateTimeOffset.Parse("2026-07-19T09:30:00+00:00")
            }
            : CreateProviderResult(providerStatus, DateTimeOffset.Parse("2026-07-19T09:30:00+00:00"));

        var provider = new StubProvider("AeroTrack", providerData);
        IFlightStatusService service = new FlightStatusService(new[] { provider });

        var result = await service.GetFlightStatusAsync("SKY-1", TestDate, CancellationToken.None);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal("AeroTrack", result.SourceProvider);
    }

    [Theory]
    [InlineData(10, FlightStatus.Domain.Enums.FlightStatus.OnTime)]
    [InlineData(20, FlightStatus.Domain.Enums.FlightStatus.Delayed)]
    public async Task GetFlightStatusAsync_WhenStatusIsUnknown_UsesTimingToInferStatus(int minuteDelta, FlightStatus.Domain.Enums.FlightStatus expectedStatus)
    {
        var scheduled = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero);
        var actual = scheduled.AddMinutes(minuteDelta);
        var provider = new StubProvider("QuickFlight", CreateProviderResult(null, DateTimeOffset.Parse("2026-07-19T09:30:00+00:00"), scheduled, actual));
        IFlightStatusService service = new FlightStatusService(new[] { provider });

        var result = await service.GetFlightStatusAsync("SKY-2", TestDate, CancellationToken.None);

        Assert.Equal(expectedStatus, result.Status);
    }

    [Fact]
    public async Task GetFlightStatusAsync_WhenBothProvidersReturnData_PrefersLatestLastUpdatedUtc()
    {
        var older = CreateProviderResult("ON_TIME", DateTimeOffset.Parse("2026-07-19T09:20:00+00:00"));
        var newer = CreateProviderResult("DELAY", DateTimeOffset.Parse("2026-07-19T09:40:00+00:00"));
        IFlightStatusService service = new FlightStatusService(new IFlightStatusProvider[]
        {
            new StubProvider("AeroTrack", older),
            new StubProvider("QuickFlight", newer)
        });

        var result = await service.GetFlightStatusAsync("SKY-PAIR", TestDate, CancellationToken.None);

        Assert.Equal(FlightStatus.Domain.Enums.FlightStatus.Delayed, result.Status);
        Assert.Equal("QuickFlight", result.SourceProvider);
        Assert.Equal(DateTimeOffset.Parse("2026-07-19T09:40:00+00:00"), result.LastUpdatedUtc);
    }

    [Fact]
    public async Task GetFlightStatusAsync_WhenOnlyOneProviderReturnsData_UsesThatResult()
    {
        IFlightStatusService service = new FlightStatusService(new IFlightStatusProvider[]
        {
            new StubProvider("AeroTrack", CreateProviderResult("CANCELLED", DateTimeOffset.Parse("2026-07-19T09:30:00+00:00"))),
            new StubProvider("QuickFlight", null)
        });

        var result = await service.GetFlightStatusAsync("SKY-AERO-ONLY", TestDate, CancellationToken.None);

        Assert.Equal(FlightStatus.Domain.Enums.FlightStatus.Cancelled, result.Status);
        Assert.Equal("AeroTrack", result.SourceProvider);
    }

    [Fact]
    public async Task GetFlightStatusAsync_WhenOnlyQuickFlightReturnsData_UsesThatResult()
    {
        IFlightStatusService service = new FlightStatusService(new IFlightStatusProvider[]
        {
            new StubProvider("AeroTrack", null),
            new StubProvider("QuickFlight", CreateProviderResult("DIVERTED", DateTimeOffset.Parse("2026-07-19T09:35:00+00:00")))
        });

        var result = await service.GetFlightStatusAsync("SKY-QUICK-ONLY", TestDate, CancellationToken.None);

        Assert.Equal(FlightStatus.Domain.Enums.FlightStatus.Diverted, result.Status);
        Assert.Equal("QuickFlight", result.SourceProvider);
    }

    [Fact]
    public async Task GetFlightStatusAsync_WhenNeitherProviderReturnsData_ReturnsUnknownWithClearMessage()
    {
        IFlightStatusService service = new FlightStatusService(new IFlightStatusProvider[]
        {
            new StubProvider("AeroTrack", null),
            new StubProvider("QuickFlight", null)
        });

        var result = await service.GetFlightStatusAsync("SKY-NONE", TestDate, CancellationToken.None);

        Assert.Equal(FlightStatus.Domain.Enums.FlightStatus.Unknown, result.Status);
        Assert.Equal("No usable flight status was returned by either provider.", result.Message);
        Assert.Null(result.SourceProvider);
    }

    private static ProviderFlightStatusData CreateProviderResult(
        string? status,
        DateTimeOffset lastUpdatedUtc,
        DateTimeOffset? scheduledDepartureUtc = null,
        DateTimeOffset? actualDepartureUtc = null)
    {
        return new ProviderFlightStatusData
        {
            FlightNumber = "SKY-TEST",
            Date = TestDate.ToString("yyyy-MM-dd"),
            Status = status,
            Message = "Provider response",
            ScheduledDepartureUtc = scheduledDepartureUtc ?? new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
            ActualDepartureUtc = actualDepartureUtc ?? new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
            LastUpdatedUtc = lastUpdatedUtc
        };
    }

    private sealed class StubProvider : IFlightStatusProvider
    {
        private readonly ProviderFlightStatusData? _result;

        public StubProvider(string name, ProviderFlightStatusData? result)
        {
            Name = name;
            _result = result;
        }

        public string Name { get; }

        public Task<ProviderFlightStatusData?> GetStatusAsync(string flightNumber, DateOnly date, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }
}