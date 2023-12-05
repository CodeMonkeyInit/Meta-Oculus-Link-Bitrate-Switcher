using OculusQuestBitrateSwitcher;

// new OculusLinkOptimizer().OptimizeOculusLink(OculusLinkMode.Wired);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<OculusModeWorker>(); })
    .Build();

host.Run();