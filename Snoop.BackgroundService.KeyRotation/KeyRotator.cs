using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Snoop.Background.KeyRotation.Interfaces;

namespace Snoop.Background.KeyRotation
{
    public class KeyRotator : BackgroundService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeyRotator> _logger;
        private readonly IEncryptionServiceWrapper _encryptionServiceWrapper;
        private int executionCount = 0;
        private Timer _timer;

        public KeyRotator(ILogger<KeyRotator> logger, IConfiguration configuration, IEncryptionServiceWrapper encryptionServiceWrapper)
        {
            _logger = logger;
            _configuration = configuration;
            _encryptionServiceWrapper = encryptionServiceWrapper;
            _timer = new Timer(RotateKeys, null, TimeSpan.Zero, TimeSpan.FromSeconds(this.KeyRotationIntervalSeconds));
        }

        public int KeyRotationIntervalSeconds => _configuration.GetValue<int>("KeyRotator:KeyRotationIntervalSeconds");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("KeyRotator running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private async void RotateKeys(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation("RotateKey: KeyRotation triggered. Count: {count}. Interval: {interval}s", count, this.KeyRotationIntervalSeconds);
            
            var result = await _encryptionServiceWrapper.InvokeRotateKeys();

            if (result.Success)
            {
                _logger.LogInformation("RotateKey: KeyRotation {status}.", "succeeded");
            }
            else
            {
                _logger.LogWarning("RotateKey: KeyRotation {status}. {reason}}", "failed", result.ErrorMessage);
            }

        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KeyRotator Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
