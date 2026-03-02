using FluentAssertions;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.UnitTests.Carriers.Domain;

/// <summary>
/// Unit tests for the AmBestRating value object.
/// </summary>
public class AmBestRatingTests
{
    [Theory]
    [InlineData("A++")]
    [InlineData("A+")]
    [InlineData("A")]
    [InlineData("A-")]
    [InlineData("B++")]
    [InlineData("B+")]
    [InlineData("NR")]
    public void Create_ValidRating_CreatesRating(string rating)
    {
        // Act
        var amBestRating = AmBestRating.Create(rating);

        // Assert
        amBestRating.Should().NotBeNull();
        amBestRating.Value.Should().Be(rating.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrNullRating_ThrowsException(string? rating)
    {
        // Act
        var act = () => AmBestRating.Create(rating!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    [Theory]
    [InlineData("AA")]
    [InlineData("AAA")]
    [InlineData("X")]
    [InlineData("Invalid")]
    public void Create_InvalidRating_ThrowsException(string rating)
    {
        // Act
        var act = () => AmBestRating.Create(rating);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid*");
    }

    [Theory]
    [InlineData("A++", true)]
    [InlineData("A+", true)]
    [InlineData("A", true)]
    [InlineData("A-", true)]
    [InlineData("B++", false)]
    [InlineData("B+", false)]
    [InlineData("NR", false)]
    public void IsSecure_ReturnsCorrectValue(string rating, bool expectedSecure)
    {
        // Arrange
        var amBestRating = AmBestRating.Create(rating);

        // Assert
        amBestRating.IsSecure.Should().Be(expectedSecure);
    }

    [Theory]
    [InlineData("A+", true)]
    [InlineData("NR", false)]
    public void IsRated_ReturnsCorrectValue(string rating, bool expectedRated)
    {
        // Arrange
        var amBestRating = AmBestRating.Create(rating);

        // Assert
        amBestRating.IsRated.Should().Be(expectedRated);
    }

    [Fact]
    public void NotRated_ReturnsNRRating()
    {
        // Act
        var rating = AmBestRating.NotRated();

        // Assert
        rating.Value.Should().Be("NR");
        rating.IsRated.Should().BeFalse();
    }

    [Fact]
    public void Create_LowercaseRating_NormalizesToUppercase()
    {
        // Act
        var rating = AmBestRating.Create("a+");

        // Assert
        rating.Value.Should().Be("A+");
    }
}
