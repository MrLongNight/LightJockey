using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using LightJockey.ViewModels;
using LightJockey.Services;
using System.Collections.Generic;

namespace LightJockey.Tests.ViewModels
{
    public class EffectControlViewModelTests
    {
        private readonly Mock<ILogger<EffectControlViewModel>> _mockLogger;
        private readonly Mock<IEffectEngine> _mockEffectEngine;
        private readonly Mock<ILogger<HueControlViewModel>> _mockHueLogger;
        private readonly Mock<IHueService> _mockHueService;
        private readonly HueControlViewModel _hueViewModel;
        private readonly EffectControlViewModel _viewModel;

        public EffectControlViewModelTests()
        {
            _mockLogger = new Mock<ILogger<EffectControlViewModel>>();
            _mockEffectEngine = new Mock<IEffectEngine>();
            _mockHueLogger = new Mock<ILogger<HueControlViewModel>>();
            _mockHueService = new Mock<IHueService>();

            // Setup mock for GetAvailableEffects
            _mockEffectEngine.Setup(e => e.GetAvailableEffects()).Returns(new List<string> { "Effect1", "Effect2" });

            _hueViewModel = new HueControlViewModel(_mockHueLogger.Object, _mockHueService.Object);
            _viewModel = new EffectControlViewModel(_mockLogger.Object, _mockEffectEngine.Object, _hueViewModel);
        }

        [Fact]
        public void StartEffectCommand_CanExecute_ChangesWithDependencies()
        {
            // Arrange
            var command = _viewModel.StartEffectCommand;

            // Assert initial state: Cannot execute (Hue not connected, effect not selected)
             _viewModel.SelectedEffect = null; // Ensure no effect is selected initially
            Assert.False(command.CanExecute(null));

            // Act: Select an effect
            _viewModel.SelectedEffect = "Effect1";

            // Assert: Still cannot execute (Hue not connected)
            Assert.False(command.CanExecute(null));

            // Act: Connect Hue
            _hueViewModel.IsHueConnected = true;

            // Assert: Can now execute
            Assert.True(command.CanExecute(null));

            // Act: Start the effect
            _viewModel.IsEffectRunning = true;

            // Assert: Cannot execute when effect is already running
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void StopEffectCommand_CanExecute_ChangesWithEffectState()
        {
            // Arrange
            var command = _viewModel.StopEffectCommand;

            // Assert initial state: Cannot execute, effect is not running
            Assert.False(command.CanExecute(null));

            // Act: Start the effect
            _viewModel.IsEffectRunning = true;

            // Assert: Can now execute
            Assert.True(command.CanExecute(null));

            // Act: Stop the effect
            _viewModel.IsEffectRunning = false;

            // Assert: Cannot execute again
            Assert.False(command.CanExecute(null));
        }
    }
}
