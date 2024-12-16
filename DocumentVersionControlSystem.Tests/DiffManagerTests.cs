namespace DocumentVersionControlSystem.Tests;

public class DiffManagerTests
{
    [Fact]
    public void GetDiffText_ShouldReturnDiffText()
    {
        // Arrange
        var diffManager = new DiffManager.DiffManager();
        var oldText = "Hello, world!";
        var newText = "Hello, world! How are you?";
        var expectedDiffText = "Modified: Hello, world! How are you?\r\n";

        // Act
        var diffText = diffManager.GetDiffText(oldText, newText);

        // Assert
        Assert.Equal(expectedDiffText, diffText);
    }

    [Fact]
    public void GetDiffText_ShouldReturnEmptyString()
    {
        // Arrange
        var diffManager = new DiffManager.DiffManager();
        var oldText = "Hello, world!";
        var newText = "Hello, world!";
        var expectedDiffText = "Unchanged: Hello, world!\r\n";

        // Act
        var diffText = diffManager.GetDiffText(oldText, newText);

        // Assert
        Assert.Equal(expectedDiffText, diffText);
    }

    [Fact]
    public void GetDiffText_ShouldReturnDiffTextWithMultipleChanges()
    {
        // Arrange
        var diffManager = new DiffManager.DiffManager();
        var oldText = "Hello, world!";
        var newText = "Hi, world! How are you?";
        var expectedDiffText = "Modified: Hi, world! How are you?\r\n";

        // Act
        var diffText = diffManager.GetDiffText(oldText, newText);

        // Assert
        Assert.Equal(expectedDiffText, diffText);
    }
}
