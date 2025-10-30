using Cronos;

namespace Luley_Integracion_Net.Services;

public class OrderCronJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCronJob> _logger;
    private readonly string _cronExpression;

    public OrderCronJob(IServiceScopeFactory scopeFactory, ILogger<OrderCronJob> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _cronExpression = configuration["CronJob:Schedule"] ?? "*/10 * * * *";
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cronExpression = CronExpression.Parse(_cronExpression);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = cronExpression.GetNextOccurrence(now);

            if (nextRun.HasValue)
            {
                var delay = nextRun.Value - now;
                _logger.LogInformation("Next order processing run at: {NextRun}", nextRun.Value);

                await Task.Delay(delay, stoppingToken);

                await ProcessOrdersAsync();
            }
        }
    }


    private async Task ProcessOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Starting order processing job...");

            await using var scope = _scopeFactory.CreateAsyncScope();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

            await orderService.ProcessUpdatedOrdersAsync();

            _logger.LogInformation("Order processing job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing orders");
        }
    }
}
