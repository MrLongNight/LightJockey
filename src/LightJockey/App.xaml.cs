using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using LightJockey.Views;
using LightJockey.ViewModels;

namespace LightJockey;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        ConfigureLogging();

        // Set up global exception handling
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        Log.Information("LightJockey application starting");

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        
        // Subscribe to theme changes
        var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        viewModel.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsDarkTheme))
            {
                SwitchTheme(viewModel.IsDarkTheme);
            }
        };
        
        mainWindow.Show();
    }

    private void SwitchTheme(bool isDarkTheme)
    {
        var themeName = isDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
        var themeUri = new Uri($"Themes/{themeName}", UriKind.Relative);
        
        var newTheme = new ResourceDictionary { Source = themeUri };
        
        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(newTheme);
        
        Log.Information("Switched to {Theme} theme", isDarkTheme ? "Dark" : "Light");
    }

    private void ConfigureLogging()
    {
        // Create logs directory if it doesn't exist
        var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logsPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logsPath, "lightjockey-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register Serilog as the logging provider
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        // Register application services
        services.AddSingleton<Services.IExampleService, Services.ExampleService>();
        services.AddSingleton<Services.IAudioService, Services.AudioService>();

        // Register audio analysis services
        services.AddSingleton<Services.IFFTProcessor, Services.FFTProcessor>();
        services.AddSingleton<Services.ISpectralAnalyzer, Services.SpectralAnalyzer>();
        services.AddSingleton<Services.IBeatDetector, Services.BeatDetector>();

        // Register performance metrics service
        services.AddSingleton<Services.IPerformanceMetricsService, Services.PerformanceMetricsService>();

        // Register Hue services
        services.AddSingleton<Services.IHueService, Services.HueService>();
        services.AddSingleton<Services.IEntertainmentService, Services.EntertainmentService>();

        // Register effect plugins
        // Slow HTTPS effects
        services.AddTransient<Services.Effects.SlowHttpsEffect>();
        services.AddTransient<Services.Effects.RainbowCycleEffect>();
        services.AddTransient<Services.Effects.SmoothFadeEffect>();
        services.AddTransient<Services.Effects.FFTLowFrequencyEffect>();
        services.AddTransient<Services.Effects.FFTMidFrequencyEffect>();
        services.AddTransient<Services.Effects.StrobeManualEffect>();
        
        // Fast DTLS effects
        services.AddTransient<Services.Effects.FastEntertainmentEffect>();
        services.AddTransient<Services.Effects.FFTHighFrequencyEffect>();
        services.AddTransient<Services.Effects.RainbowFastEffect>();
        services.AddTransient<Services.Effects.PulseEffect>();
        services.AddTransient<Services.Effects.ChaseEffect>();
        services.AddTransient<Services.Effects.SparkleEffect>();

        // Register EffectEngine with plugin registration
        services.AddSingleton<Services.IEffectEngine>(sp =>
        {
            var engine = new Services.EffectEngine(
                sp.GetRequiredService<ILogger<Services.EffectEngine>>(),
                sp.GetRequiredService<Services.IAudioService>(),
                sp.GetRequiredService<Services.ISpectralAnalyzer>(),
                sp.GetRequiredService<Services.IBeatDetector>());
            
            // Register slow HTTPS effect plugins
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.SlowHttpsEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.RainbowCycleEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.SmoothFadeEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.FFTLowFrequencyEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.FFTMidFrequencyEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.StrobeManualEffect>());
            
            // Register fast DTLS effect plugins
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.FastEntertainmentEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.FFTHighFrequencyEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.RainbowFastEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.PulseEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.ChaseEffect>());
            engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.SparkleEffect>());
            
            return engine;
        });

        // Register ViewModels
        services.AddSingleton<MainWindowViewModel>();

        // Register PresetService
        services.AddSingleton<Services.IPresetService, Services.PresetService>();

        // Register views
        services.AddSingleton<MainWindow>();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception occurred");
        
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nPlease check the log files for more details.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Log.Fatal(exception, "Unhandled exception occurred");
        }
        else
        {
            Log.Fatal("Unhandled non-exception object: {ExceptionObject}", e.ExceptionObject);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("LightJockey application shutting down");
        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
