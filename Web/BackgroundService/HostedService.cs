using Web.Database;

namespace Web.BackgroundService
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(15));

            return;
        }

        private async void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            using (var scope = _services.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var articleSender = scope.ServiceProvider.GetRequiredService<ArticleSender>();

                foreach (var user in await userService.GetUsers())
                {
                    _logger.LogInformation($"sending for user: {user.PocketUsername}");
                    await articleSender.SendArticlesAsync(user);
                    _logger.LogInformation($"done for user: {user.PocketUsername}");
                    await userService.UpdateLastProcessingDateAsync(user);
                }
            }

            _logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
