namespace LightJockey.Models;

/// <summary>
/// Represents the lifecycle state of an effect
/// </summary>
public enum EffectState
{
    /// <summary>
    /// Effect is not yet initialized
    /// </summary>
    Uninitialized = 0,

    /// <summary>
    /// Effect is initialized and ready to start
    /// </summary>
    Initialized = 1,

    /// <summary>
    /// Effect is currently running
    /// </summary>
    Running = 2,

    /// <summary>
    /// Effect is paused
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Effect has been stopped
    /// </summary>
    Stopped = 4,

    /// <summary>
    /// Effect encountered an error
    /// </summary>
    Error = 5
}
