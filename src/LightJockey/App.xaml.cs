using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using LightJockey.Views;
using LightJockey.ViewModels;
using LightJockey.Services;
using LightJockey.Services.Effects;
using LightJockey.Models;
using System.Reflection;
using System.Threading.Tasks;

namespace LightJockey;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IConfigurationService? _configurationService;
    private AppSettings? _appSettings;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Temporary logger for startup
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        // Build a temporary service provider to get the logger for ConfigurationService
        var tempServices = new ServiceCollection();
        tempServices.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        var tempProvider = tempServices.BuildServiceProvider();
        var configLogger = tempProvider.GetRequiredService<ILogger<ConfigurationService>>();

        _configurationService = new ConfigurationService(configLogger);
        _appSettings = await _configurationService.LoadAppSettingsAsync();

        // Now configure the final logger
        ConfigureLogging();

        // Set up global exception handling
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        try
        {
            Log.Information("LightJockey application starting");

            // Load the default theme at startup
            SwitchTheme(isDarkTheme: true);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Subscribe to theme changes from the ViewModel
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
        catch (Exception ex)
        {
            Log.Fatal(ex, "A critical error occurred during application startup.");
            MessageBox.Show(
                $"A critical error occurred and the application must close.\n\n{ex.Message}\n\nPlease check the log files for more details.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void SwitchTheme(bool isDarkTheme)
    {
        var themeName = isDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
        var themeUri = new Uri($"Themes/{themeName}", UriKind.Relative);
        
        var newTheme = new ResourceDictionary { Source = themeUri };
        
        // Find the existing theme dictionary and replace it
        var existingTheme = Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

        if (existingTheme != null)
        {
            Resources.MergedDictionaries.Remove(existingTheme);
        }

        Resources.MergedDictionaries.Add(newTheme);
        
        Log.Information("Switched to {Theme} theme", isDarkTheme ? "Dark" : "Light");
    }

    private void ConfigureLogging()
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var logsPath = Path.Combine(baseDirectory, "logs");
            Directory.CreateDirectory(logsPath);

            if (_appSettings != null)
            {
                // Custom log file cleanup
                var logFiles = new DirectoryInfo(logsPath)
                    .GetFiles("lightjockey-*.log")
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(_appSettings.RetainedLogFileCount > 0 ? _appSettings.RetainedLogFileCount - 1 : 0) // Ensure count is positive
                    .ToList();

                foreach (var logFile in logFiles)
                {
                    try
                    {
                        logFile.Delete();
                    }
                    catch (IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting log file: {ex.Message}");
                    }
                }
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFilePath = Path.Combine(logsPath, $"lightjockey-{timestamp}.log");

            var logLevel = _appSettings != null && Enum.TryParse<LogEventLevel>(_appSettings.LogLevel, true, out var level)
                ? level
                : LogEventLevel.Information;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{ProcessId}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFilePath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{ProcessId}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to configure logging: {ex.Message}", "Logging Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Use a fallback logger
            File.AppendAllText("logging_error.txt", $"{DateTime.Now}: {ex}\n");
        }
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
        services.AddSingleton<IExampleService, ExampleService>();
        services.AddSingleton<IAudioService, AudioService>();

        // Register audio analysis services
        services.AddSingleton<IFFTProcessor, FFTProcessor>();
        services.AddSingleton<ISpectralAnalyzer, SpectralAnalyzer>();
        services.AddSingleton<IBeatDetector, BeatDetector>();

        // Register performance metrics service
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();

        // Register Hue services
        services.AddSingleton<IHueService>(sp =>
            new HueService(
                sp.GetRequiredService<ILogger<HueService>>(),
                sp.GetRequiredService<IConfigurationService>()));
        services.AddSingleton<IEntertainmentService, EntertainmentService>();
        
        // Dynamically register all effect plugins
        var effectPluginType = typeof(IEffectPlugin);
        var effectPlugins = Assembly.GetExecutingAssembly().GetTypes()
            .Where(p => effectPluginType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        foreach (var plugin in effectPlugins)
        {
            services.AddTransient(plugin);
        }

        // Register EffectEngine with dynamic plugin registration
        services.AddSingleton<IEffectEngine>(sp =>
        {
            var engine = new EffectEngine(
                sp.GetRequiredService<ILogger<EffectEngine>>(),
                sp.GetRequiredService<IAudioService>(),
                sp.GetRequiredService<ISpectralAnalyzer>(),
                sp.GetRequiredService<IBeatDetector>());

            foreach (var pluginType in effectPlugins)
            {
                var pluginInstance = (IEffectPlugin)sp.GetRequiredService(pluginType);
                engine.RegisterPlugin(pluginInstance);
            }
            
            return engine;
        });

        // Register services
        services.AddSingleton<IDialogService, DialogService>();

        // Register ViewModels
        services.AddSingleton<AudioControlViewModel>();
        services.AddSingleton<HueControlViewModel>();
        services.AddSingleton<EffectControlViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton(sp => new MetricsViewModel(
            sp.GetRequiredService<IMetricsService>(),
            sp.GetRequiredService<ILogger<MetricsViewModel>>()));

        // Register PresetService
        services.AddSingleton<IPresetService, PresetService>();

        // Register BackupService
        services.AddSingleton<IBackupService, BackupService>();

        // Register ConfigurationService for secure data storage
        if (_configurationService != null)
        {
            services.AddSingleton<IConfigurationService>(_configurationService);
        }

        // Register views
        services.AddSingleton<MainWindow>();
        services.AddTransient<SettingsWindow>();
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
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "A fatal unhandled exception occurred, forcing application shutdown. IsTerminating: {IsTerminating}", e.IsTerminating);

        MessageBox.Show(
            "A fatal error occurred and the application must close. Please check the log files for details.",
            "Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        Log.CloseAndFlush();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("LightJockey application shutting down");
        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
