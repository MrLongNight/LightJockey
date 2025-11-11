using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Example service implementation demonstrating logging and error handling
/// </summary>
public class ExampleService : IExampleService
{
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(ILogger<ExampleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("ExampleService initialized");
    }

    /// <summary>
    /// Performs an example operation that demonstrates logging and error handling
    /// </summary>
    /// <param name="shouldThrow">Whether to simulate an error</param>
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
