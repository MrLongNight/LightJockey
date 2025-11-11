using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using LightJockey.Views;

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
        mainWindow.Show();
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

        // Register Hue services
        services.AddSingleton<Services.IHueService, Services.HueService>();
        services.AddSingleton<Services.IEntertainmentService, Services.EntertainmentService>();

        // Register effect plugins
        services.AddTransient<Services.Effects.SlowHttpsEffect>();
        services.AddTransient<Services.Effects.FastEntertainmentEffect>();

        // Register EffectEngine
        services.AddSingleton<Services.IEffectEngine, Services.EffectEngine>();

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
