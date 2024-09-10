namespace Trenning_NotificationsExample.Services
{
    public class PassportUpdateHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider; //Провайдер услуг для создания scope и получения служб(зарегать в program)

        public PassportUpdateHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdatePassports, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void UpdatePassports(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var passportUpdateService = scope.ServiceProvider.GetRequiredService<PassportUpdateService>();
                await passportUpdateService.UpdatePassportsAsync("passports.txt");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
