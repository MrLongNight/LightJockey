using LightJockey.ViewModels;

namespace LightJockey.Tests.ViewModels;

public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = string.Empty;

        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }
    }

    [Fact]
    public void SetProperty_ShouldRaisePropertyChanged_WhenValueChanges()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.TestProperty))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TestProperty = "New Value";

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal("New Value", viewModel.TestProperty);
    }

    [Fact]
    public void SetProperty_ShouldNotRaisePropertyChanged_WhenValueIsSame()
    {
        // Arrange
        var viewModel = new TestViewModel();
        viewModel.TestProperty = "Initial Value";
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TestViewModel.TestProperty))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TestProperty = "Initial Value";

        // Assert
        Assert.False(propertyChangedRaised);
    }
}
