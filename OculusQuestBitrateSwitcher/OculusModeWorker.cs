using System.Diagnostics;
using System.Management;

namespace OculusQuestBitrateSwitcher;

public class OculusModeWorker : BackgroundService
{
    private readonly ILogger<OculusModeWorker> _logger;

    public OculusModeWorker(ILogger<OculusModeWorker> logger)
    {
        _logger = logger;
        _oculusLinkOptimizer = new OculusLinkOptimizer();
    }

    private OculusLinkMode _linkMode = OculusLinkMode.Wired;

#pragma warning disable CA1416
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var managementEventWatcher = GetManagementEventWatcher();

        _linkMode = GetCurrentLinkMode();
        _oculusLinkOptimizer.OptimizeOculusLink(_linkMode);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            managementEventWatcher.Stop();
        }
    }

    private DateTime _lastUpdate = DateTime.MinValue;
    private readonly OculusLinkOptimizer _oculusLinkOptimizer;

    private ManagementEventWatcher GetManagementEventWatcher()
    {
        var managementEventWatcher =
            new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3");

        managementEventWatcher.EventArrived += (watcher, e) =>
        {
            if (DateTime.UtcNow - _lastUpdate < TimeSpan.FromSeconds(1)) return;
            
            _lastUpdate = DateTime.UtcNow;
            var currentLinkMode = GetCurrentLinkMode();

            if (currentLinkMode != _linkMode)
            {
                _linkMode = currentLinkMode;
                _oculusLinkOptimizer.OptimizeOculusLink(_linkMode);
            }
        };  
                        

        managementEventWatcher.Start();
        return managementEventWatcher;
    }

    

    private static OculusLinkMode GetCurrentLinkMode()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        using var searcher =
            new ManagementObjectSearcher(
                @"Select name From Win32_PnPEntity where name = 'Oculus XRSP Interface'");

        var currentLinkMode = searcher.Get().Count > 0 ? OculusLinkMode.Wired : OculusLinkMode.AirLink;

        stopWatch.Stop();

        Console.WriteLine($"Link checking took {stopWatch.ElapsedMilliseconds}ms Link Mode = {currentLinkMode}");
        return currentLinkMode;
    }
    
    private static OculusLinkMode GetCurrentLinkModeFast()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        using var searcher =
            new ManagementObjectSearcher(
                @"Select name From Win32_PnPEntity where name = 'Oculus XRSP Interface'");

        var currentLinkMode = searcher.Get().Count > 0 ? OculusLinkMode.Wired : OculusLinkMode.AirLink;

        stopWatch.Stop();

        Console.WriteLine($"Link checking took {stopWatch.ElapsedMilliseconds}ms Link Mode = {currentLinkMode}");
        return currentLinkMode;
    }

#pragma warning restore CA1416
}