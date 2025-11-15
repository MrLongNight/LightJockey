using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using LightJockey.ViewModels;
using LightJockey.Services;
using LightJockey.Models;
using System.Collections.Generic;
using System.Linq;

namespace LightJockey.Tests.ViewModels
{
    public class AudioControlViewModelTests
    {
        private readonly Mock<ILogger<AudioControlViewModel>> _mockLogger;
        private readonly Mock<IAudioService> _mockAudioService;
        private readonly AudioControlViewModel _viewModel;

        public AudioControlViewModelTests()
        {
            _mockLogger = new Mock<ILogger<AudioControlViewModel>>();
            _mockAudioService = new Mock<IAudioService>();

            // Setup mock for GetOutputDevices
            var devices = new List<AudioDevice> { new AudioDevice { Id = "1", Name = "Device 1" } };
            _mockAudioService.Setup(s => s.GetOutputDevices()).Returns(devices);

            _viewModel = new AudioControlViewModel(_mockLogger.Object, _mockAudioService.Object);
        }

        [Fact]
        public void StartAudioCaptureCommand_CanExecute_ChangesWithDependencies()
        {
            // Arrange
            var command = _viewModel.StartAudioCaptureCommand;

            // Assert initial state: ViewModel constructor calls RefreshAudioDevices, so a device should be selected.
            Assert.NotNull(_viewModel.SelectedAudioDevice);
            Assert.True(command.CanExecute(null));

            // Act: Deselect device (simulate no devices found)
            _viewModel.SelectedAudioDevice = null;

            // Assert: Cannot execute when no device is selected
            Assert.False(command.CanExecute(null));

            // Act: Select a device again
            _viewModel.SelectedAudioDevice = new AudioDevice { Id = "1", Name = "Device 1" };

            // Assert: Can execute now
            Assert.True(command.CanExecute(null));

            // Act: Start capturing
            _viewModel.IsAudioCapturing = true;

            // Assert: Cannot execute when already capturing
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void StopAudioCaptureCommand_CanExecute_ChangesWithCaptureState()
        {
            // Arrange
            var command = _viewModel.StopAudioCaptureCommand;

            // Assert initial state: Cannot execute, not capturing
            Assert.False(command.CanExecute(null));

            // Act: Start capturing
            _viewModel.IsAudioCapturing = true;

            // Assert: Can now execute
            Assert.True(command.CanExecute(null));

            // Act: Stop capturing
            _viewModel.IsAudioCapturing = false;

            // Assert: Cannot execute again
            Assert.False(command.CanExecute(null));
        }
    }
}
