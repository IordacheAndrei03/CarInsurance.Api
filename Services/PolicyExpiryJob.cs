using Microsoft.EntityFrameworkCore;
using CarInsurance.Api.Data;
namespace CarInsurance.Api.Services;

public class PolicyExpiryJob(ILogger<PolicyExpiryJob> logger, IServiceProvider services) : BackgroundService
{
    private readonly ILogger<PolicyExpiryJob> _logger = logger;
    private readonly IServiceProvider _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnce(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnce(stoppingToken);
        }
    }

    private async Task RunOnce(CancellationToken ct)
    {
        try
        {
            await using var scope = _services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var yesterday = today.AddDays(-1);
            var expirationStart = yesterday.AddDays(1).ToDateTime(TimeOnly.MinValue); 
            var expirationEnd = expirationStart.AddHours(1);  // schimb intervalul in care  trimit mesaj informare

            if (now < expirationStart || now > expirationEnd) return;

            var candidates = await db.Policies
                .Where(p => p.EndDate == yesterday && !p.ExpirationLogged)
                .Select(p => new { p.Id, p.CarId, p.EndDate, p.ExpirationLogged })
                .ToListAsync(ct);

            if (candidates.Count == 0) return;

            foreach (var p in candidates)
            {
                _logger.LogInformation("Policy {PolicyId} for car {CarId} expired at {At} (window {From}..{To})",
                    p.Id, p.CarId, expirationStart, expirationStart, expirationEnd);

                var entity = await db.Policies.FirstAsync(x => x.Id == p.Id, ct);
                entity.ExpirationLogged = true;
            }

            await db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PolicyExpiryJob.");
        }
    }
}
