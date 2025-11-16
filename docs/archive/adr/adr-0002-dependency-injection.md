# ADR-0002: Dependency Injection

## Status
Accepted

## Context
The application requires multiple services (Audio, Philips Hue, Effect Engine) that need to be loosely coupled and easily testable. We need a way to manage dependencies and object lifetimes effectively.

## Decision
We will use Microsoft.Extensions.DependencyInjection for dependency injection throughout the application.

## Consequences

### Positive
- Loose coupling between components
- Easier unit testing through dependency mocking
- Centralized service registration and configuration
- Built-in lifetime management (Singleton, Transient, Scoped)
- Standard Microsoft library with good documentation

### Negative
- Additional setup required in application startup
- Potential runtime errors if dependencies are not properly registered
- Slight performance overhead

## Implementation Notes
- Services are registered in `App.xaml.cs` in the `ConfigureServices` method
- Constructor injection is the primary pattern
- Singletons are used for stateful services (e.g., AudioService, HueService)
- ViewModels are registered to enable proper dependency resolution
