namespace LightJockey.Services;

/// <summary>
/// Example service interface demonstrating service abstraction
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Performs an example operation that may throw an exception
    /// </summary>
    /// <param name="shouldThrow">Whether to simulate an error</param>
    void PerformOperation(bool shouldThrow = false);
}
