# ADR-0003: WPF as UI Framework

## Status
Accepted

## Context
LightJockey requires a desktop application framework for Windows that supports rich UI, audio processing, and network communication. The application needs to provide real-time visualization and control of lighting effects.

## Decision
We will use Windows Presentation Foundation (WPF) with .NET 9 as the UI framework.

## Consequences

### Positive
- Native Windows desktop framework with excellent performance
- Rich UI capabilities with XAML
- Strong data binding support (essential for MVVM)
- Access to full .NET ecosystem and libraries
- Built-in support for graphics and animations
- Good community support and documentation

### Negative
- Windows-only platform (not cross-platform)
- Requires Windows-specific deployment
- Some legacy API patterns compared to newer frameworks

## Alternatives Considered

### WinUI 3
- Pros: Modern UI, better performance
- Cons: Less mature ecosystem, more breaking changes

### Avalonia
- Pros: Cross-platform
- Cons: Smaller community, less documentation

### Electron
- Pros: Cross-platform, web technologies
- Cons: Larger memory footprint, not ideal for real-time audio processing

## Implementation Notes
- Target framework: `net9.0-windows`
- XAML is used for all UI definitions
- Code-behind is kept minimal, logic resides in ViewModels
