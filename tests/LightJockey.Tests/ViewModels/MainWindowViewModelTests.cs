using LightJockey.Models;
using LightJockey.Services;
using LightJockey.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly Mock<ILogger<MainWindowViewModel>> _mockLogger;
        private readonly Mock<IAudioService> _mockAudioService;
        private readonly Mock<IEffectEngine> _mockEffectEngine;
        private readonly Mock<IFFTProcessor> _mockFftProcessor;
        private readonly Mock<ISpectralAnalyzer> _mockSpectralAnalyzer;
        private readonly Mock<IBeatDetector> _mockBeatDetector;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly Mock<IHueService> _mockHueService;

        private readonly MetricsViewModel _metricsViewModel;
        private readonly AudioControlViewModel _audioControlViewModel;
        private readonly HueControlViewModel _hueControlViewModel;
        private readonly EffectControlViewModel _effectControlViewModel;

        public MainWindowViewModelTests()
        {
            _mockLogger = new Mock<ILogger<MainWindowViewModel>>();
            _mockAudioService = new Mock<IAudioService>();
            _mockEffectEngine = new Mock<IEffectEngine>();
            _mockFftProcessor = new Mock<IFFTProcessor>();
            _mockSpectralAnalyzer = new Mock<ISpectralAnalyzer>();
            _mockBeatDetector = new Mock<IBeatDetector>();
            _mockDialogService = new Mock<IDialogService>();
            _mockHueService = new Mock<IHueService>();

            _mockAudioService.Setup(s => s.GetOutputDevices()).Returns(new List<AudioDevice>());
            _mockEffectEngine.Setup(e => e.GetAvailableEffects()).Returns(new List<string>());

            var metricsServiceMock = new Mock<IMetricsService>();
            _metricsViewModel = new MetricsViewModel(metricsServiceMock.Object, new Mock<ILogger<MetricsViewModel>>().Object);
            _audioControlViewModel = new AudioControlViewModel(new Mock<ILogger<AudioControlViewModel>>().Object, _mockAudioService.Object);
            _hueControlViewModel = new HueControlViewModel(new Mock<ILogger<HueControlViewModel>>().Object, _mockHueService.Object);
            _effectControlViewModel = new EffectControlViewModel(new Mock<ILogger<EffectControlViewModel>>().Object, _mockEffectEngine.Object, _hueControlViewModel);
        }

        private MainWindowViewModel CreateViewModel()
        {
            return new MainWindowViewModel(
                _mockLogger.Object,
                _mockAudioService.Object,
                _mockFftProcessor.Object,
                _mockSpectralAnalyzer.Object,
                _mockBeatDetector.Object,
                _mockEffectEngine.Object,
                _metricsViewModel,
                _audioControlViewModel,
                _hueControlViewModel,
                _effectControlViewModel,
                _mockDialogService.Object
            );
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            var viewModel = CreateViewModel();
            Assert.NotNull(viewModel.AudioControlViewModel.AudioDevices);
            Assert.NotNull(viewModel.HueControlViewModel.HueBridges);
            Assert.NotNull(viewModel.HueControlViewModel.HueLights);
            Assert.NotNull(viewModel.EffectControlViewModel.AvailableEffects);
            Assert.False(viewModel.AudioControlViewModel.IsAudioCapturing);
            Assert.False(viewModel.HueControlViewModel.IsHueConnected);
            Assert.False(viewModel.EffectControlViewModel.IsEffectRunning);
            Assert.True(viewModel.IsDarkTheme);
        }

        [Fact]
        public void RefreshAudioDevicesCommand_LoadsDevices()
        {
            var devices = new List<AudioDevice> { new() { Name = "Device 1" }, new() { Name = "Device 2" } };
            _mockAudioService.Setup(s => s.GetOutputDevices()).Returns(devices);
            var viewModel = CreateViewModel();
            viewModel.RefreshAudioDevicesCommand.Execute(null);
            Assert.Equal(2, viewModel.AudioControlViewModel.AudioDevices.Count);
            Assert.NotNull(viewModel.AudioControlViewModel.SelectedAudioDevice);
        }
    }
}
