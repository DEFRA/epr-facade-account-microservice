using FacadeAccountCreation.Core.Extensions;

namespace FacadeAccountCreation.UnitTests.Core.Extensions;

[TestClass]
public class ServiceRolesControllerTests
{
    [TestMethod]
    [DataRow("123456", "123 456")]
    [DataRow("ABCDEF", "ABC DEF")]
    [DataRow(null, "")]
    [DataRow("1234", "1 234")]
    [DataRow("1234567", "1 234 567")]
    [DataRow("123 456", "123 456")]
    [DataRow(" 1 2 3 4 5 6  ", "123 456")]
    [DataRow("123", "123")]
    [DataRow(" 123 ", "123")]
    public void ToReferenceNumberFormat_WhenCalled_CorrectlyFormattedResult(string testValue, string expectedResult)
    {
        var actual = testValue.ToReferenceNumberFormat();
        actual.Should().Be(expectedResult);
    }
}