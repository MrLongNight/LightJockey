using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using LightJockey.ViewModels;
using LightJockey.Services;
using LightJockey.Models;
using System.Threading.Tasks;

namespace LightJockey.Tests.ViewModels
{
    public class HueControlViewModelTests
    {
        private readonly Mock<ILogger<HueControlViewModel>> _mockLogger;
        private readonly Mock<IHueService> _mockHueService;
        private readonly HueControlViewModel _viewModel;

        public HueControlViewModelTests()
        {
            _mockLogger = new Mock<ILogger<HueControlViewModel>>();
            _mockHueService = new Mock<IHueService>();
            _viewModel = new HueControlViewModel(_mockLogger.Object, _mockHueService.Object);
        }

        [Fact]
        public void ConnectToHueBridgeCommand_CanExecute_ChangesWithDependencies()
        {
            // Arrange
            var command = _viewModel.ConnectToHueBridgeCommand;

            // Assert initial state: Cannot execute because no bridge is selected
            Assert.False(command.CanExecute(null));

            // Act: Select a bridge
            _viewModel.SelectedHueBridge = new HueBridge { Id = "1", IpAddress = "127.0.0.1" };

            // Assert: Can now execute
            Assert.True(command.CanExecute(null));

            // Act: Simulate connection
            _viewModel.IsHueConnected = true;

            // Assert: Cannot execute when already connected
            Assert.False(command.CanExecute(null));

            // Act: Disconnect
            _viewModel.IsHueConnected = false;

            // Assert: Can execute again
            Assert.True(command.CanExecute(null));
        }
    }
}
