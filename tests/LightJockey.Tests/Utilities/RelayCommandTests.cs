using LightJockey.Utilities;

namespace LightJockey.Tests.Utilities;

public class RelayCommandTests
{
    [Fact]
    public void Execute_ShouldInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(_ => executed = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void CanExecute_ShouldReturnTrue_WhenNoPredicateProvided()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnPredicateResult_WhenPredicateProvided()
    {
        // Arrange
        var command = new RelayCommand(_ => { }, _ => false);

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.False(result);
    }
}
