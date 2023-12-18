using FacadeAccountCreation.Core.Extensions;
using FluentAssertions;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class StringExtensionTests
{

    [TestMethod]
    public void ToReferenceNumberFormat_WhenSixCharacters_ItShouldReturnCorrectFormat()
    {
        var expected = "123 456";
        var testString = "123456";
        var result = testString.ToReferenceNumberFormat();
        result.Should().Be(expected);
    }

    [TestMethod]
    public void ToReferenceNumberFormat_WhenFiveCharacters_ItShouldReturnCorrectFormat()
    {
        var expected = "12 345";
        var testString = "12345";
        var result = testString.ToReferenceNumberFormat();
        result.Should().Be(expected);
    }

    [TestMethod]
    public void ToReferenceNumberFormat_WhenNullString_ItShouldReturnEmptyString()
    {
        var expected = string.Empty;
        string testString = null!;
        var result = testString.ToReferenceNumberFormat();
        result.Should().Be(expected);

    }

    [TestMethod]
    public void ToReferenceNumberFormat_WhenLessThanThreeCharacters_ItShouldOriginalString()
    {
        var expected = "12";
        var testString = "12";
        var result = testString.ToReferenceNumberFormat();
        result.Should().Be(expected);
    }

    [TestMethod]
    public void ToReferenceNumberFormat_WhenLongString_ItShouldReturnCorrectFormat()
    {
        var expected = "123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789 123 456 789";
        var testString = "123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789";
        var result = testString.ToReferenceNumberFormat();
        result.Should().Be(expected);
    }
}
