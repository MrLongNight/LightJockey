# ADR-0001: MVVM Architecture Pattern

## Status
Accepted

## Context
LightJockey is a WPF application that requires a clean separation of concerns between the user interface, business logic, and data. The application needs to be testable, maintainable, and follow modern .NET development practices.

## Decision
We will use the Model-View-ViewModel (MVVM) architectural pattern for structuring the application:

- **Models**: Data structures and business entities
- **Views**: XAML-based user interface components
- **ViewModels**: Presentation logic and state management
- **Services**: Business logic and external integrations (Audio, Philips Hue)
- **Utilities**: Helper classes and common functionality

## Consequences

### Positive
- Clear separation of concerns between UI and business logic
- Improved testability - ViewModels can be unit tested without UI
- Better code organization and maintainability
- Enables data binding for reactive UI updates
- Standard pattern well-understood by WPF developers

### Negative
- Additional boilerplate code for ViewModels
- Learning curve for developers new to MVVM
- Requires discipline to maintain proper separation

## Implementation Notes
- Base classes like `ViewModelBase` provide common MVVM functionality
- `INotifyPropertyChanged` is used for property change notifications
- `ICommand` implementations (RelayCommand) handle user interactions
