# Task 1 — DI, Logging, Global Error Handling

**Status**: ✅ Completed  
**PR**: Task1_DI_Logging  
**Date**: 2025-11-11

## Objective

Implement Dependency Injection, Serilog logging infrastructure, and global error handling for the LightJockey application.

## Implementation

### 1. NuGet Packages Added

The following Serilog packages were added to the main project:

```xml
<PackageReference Include="Serilog" Version="4.2.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
```

For testing:
```xml
<PackageReference Include="Moq" Version="4.20.72" />
```

### 2. Serilog Configuration

Serilog is configured in `App.xaml.cs` with the following features:

#### Console Sink
- Outputs logs to the console during debugging
- Useful for development and troubleshooting

#### File Sink
- Writes logs to `logs/lightjockey-{Date}.log`
- Rolling interval: Daily
- Retained file count: 7 days
- Custom output template with timestamp, level, message, and exception details

#### Log Levels
- Minimum level set to `Debug` for comprehensive logging
- Supports all standard log levels: Trace, Debug, Information, Warning, Error, Fatal

#### Configuration Code

```csharp
private void ConfigureLogging()
{
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
```

### 3. Dependency Injection Integration

#### Microsoft.Extensions.Logging Integration

Serilog is integrated with `Microsoft.Extensions.Logging` in the DI container:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(dispose: true);
    });

    // Register services
    services.AddSingleton<IExampleService, ExampleService>();
    services.AddSingleton<MainWindow>();
}
```

This allows services to inject `ILogger<T>` for typed logging.

### 4. Global Error Handling

Two levels of exception handling are implemented:

#### WPF Dispatcher Unhandled Exceptions

Catches exceptions from the UI thread:

```csharp
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
```

Features:
- Logs the exception with full stack trace
- Displays user-friendly error message
- Marks exception as handled to prevent app crash
- Directs users to log files for details

#### AppDomain Unhandled Exceptions

Catches exceptions from non-UI threads:

```csharp
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
```

Features:
- Logs fatal exceptions before app termination
- Handles both Exception objects and other error objects
- Ensures errors are recorded even if app crashes

### 5. Example Service with Logging

Created `IExampleService` and `ExampleService` to demonstrate best practices:

#### Interface (`IExampleService.cs`)

```csharp
public interface IExampleService
{
    void PerformOperation(bool shouldThrow = false);
}
```

#### Implementation (`ExampleService.cs`)

```csharp
public class ExampleService : IExampleService
{
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(ILogger<ExampleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("ExampleService initialized");
    }

    public void PerformOperation(bool shouldThrow = false)
    {
        try
        {
            _logger.LogInformation("Starting example operation (shouldThrow: {ShouldThrow})", shouldThrow);

            if (shouldThrow)
            {
                throw new InvalidOperationException("Example error to demonstrate error handling");
            }

            _logger.LogInformation("Example operation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during example operation");
            throw;
        }
    }
}
```

Key features demonstrated:
- Constructor dependency injection of `ILogger<T>`
- Null checking for dependencies
- Structured logging with parameters
- Try-catch with logging before re-throwing
- Different log levels for different scenarios

### 6. Unit Tests for Error Handling

Created comprehensive unit tests in `ExampleServiceTests.cs`:

#### Test Coverage

1. **Constructor_WithNullLogger_ThrowsArgumentNullException**
   - Verifies null checking for dependencies

2. **Constructor_WithValidLogger_LogsDebugMessage**
   - Verifies initialization logging

3. **PerformOperation_WithShouldThrowFalse_CompletesSuccessfully**
   - Verifies successful operation path
   - Confirms information logging

4. **PerformOperation_WithShouldThrowTrue_LogsErrorAndThrows**
   - Verifies error handling
   - Confirms error logging
   - Ensures exception is re-thrown

5. **PerformOperation_WithShouldThrowTrue_DoesNotLogSuccessMessage**
   - Verifies that success messages aren't logged on error

#### Testing Approach

- Uses **Moq** for mocking `ILogger<T>`
- Verifies log levels and message content
- Ensures exceptions are properly thrown and logged
- Demonstrates testing patterns for future services

### 7. Application Lifecycle Logging

The application logs key lifecycle events:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    // ... configuration ...
    Log.Information("LightJockey application starting");
    // ... show main window ...
}

protected override void OnExit(ExitEventArgs e)
{
    Log.Information("LightJockey application shutting down");
    _serviceProvider?.Dispose();
    Log.CloseAndFlush();
    base.OnExit(e);
}
```

## Usage Guidelines

### For Service Developers

1. **Inject ILogger<T>** in service constructors:
   ```csharp
   public MyService(ILogger<MyService> logger)
   {
       _logger = logger ?? throw new ArgumentNullException(nameof(logger));
   }
   ```

2. **Use appropriate log levels**:
   - `Trace`: Very detailed debugging information
   - `Debug`: Debugging information
   - `Information`: General informational messages
   - `Warning`: Warning messages for potentially harmful situations
   - `Error`: Error messages for recoverable errors
   - `Fatal`: Critical errors that cause application termination

3. **Use structured logging**:
   ```csharp
   _logger.LogInformation("Processing item {ItemId} for user {UserId}", itemId, userId);
   ```

4. **Log exceptions with context**:
   ```csharp
   catch (Exception ex)
   {
       _logger.LogError(ex, "Failed to process item {ItemId}", itemId);
       throw;
   }
   ```

### Log File Location

Logs are written to: `{ApplicationDirectory}/logs/lightjockey-{Date}.log`

Example: `logs/lightjockey-20251111.log`

## Testing

Run the tests with:

```bash
dotnet test
```

Note: Tests will build successfully but cannot run on Linux due to WPF dependencies. Tests must be run on Windows.

## CI/CD Integration

The existing GitHub Actions workflow already includes:
- Build verification
- Test execution (runs on Windows)
- Code coverage collection

## Architecture Decisions

### Why Serilog?

- **Structured Logging**: Better than string concatenation
- **Multiple Sinks**: Can write to console, file, database, etc.
- **Performance**: Efficient and minimal overhead
- **Extensibility**: Easy to add custom sinks and enrichers
- **Industry Standard**: Widely used in .NET applications

### Why Global Error Handling?

- **User Experience**: Graceful error messages instead of crashes
- **Debugging**: All errors are logged for investigation
- **Reliability**: Prevents application crashes from unhandled exceptions
- **Diagnostics**: Complete error information in log files

### Why Microsoft.Extensions.Logging Integration?

- **Standardization**: Standard .NET logging abstraction
- **Testability**: Easy to mock ILogger<T> in unit tests
- **Flexibility**: Can switch logging providers if needed
- **DI Integration**: Works seamlessly with dependency injection

## Next Steps

This logging and error handling infrastructure is now ready for use in:
- **Task 2**: AudioService implementation
- **Task 3**: FFT Processor and Beat Detector
- **Task 4**: Hue Service
- All future service implementations

## Files Modified/Created

### Main Project
- ✅ `src/LightJockey/App.xaml.cs` - Added Serilog configuration and error handling
- ✅ `src/LightJockey/LightJockey.csproj` - Added Serilog packages
- ✅ `src/LightJockey/Services/IExampleService.cs` - Example service interface
- ✅ `src/LightJockey/Services/ExampleService.cs` - Example service with logging

### Test Project
- ✅ `tests/LightJockey.Tests/LightJockey.Tests.csproj` - Added Moq package
- ✅ `tests/LightJockey.Tests/Services/ExampleServiceTests.cs` - Error handling tests

### Documentation
- ✅ `docs/tasks/Task1_DI_Logging.md` - This document

## References

- [Serilog Documentation](https://serilog.net/)
- [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
- [WPF Exception Handling](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/exception-handling-wpf)
