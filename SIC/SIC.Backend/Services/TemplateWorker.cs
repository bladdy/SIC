namespace SIC.Backend.Services;

public class TemplateWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IBackgroundTaskQueue _queue;

    public TemplateWorker(
        IServiceProvider services,
        IBackgroundTaskQueue queue)
    {
        _services = services;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 BACKGROUND ERROR:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}