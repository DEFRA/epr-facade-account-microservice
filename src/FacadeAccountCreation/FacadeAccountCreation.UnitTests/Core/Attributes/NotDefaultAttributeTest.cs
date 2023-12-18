using FacadeAccountCreation.Core.Attributes;

namespace FacadeAccountCreation.UnitTests.Core.Attributes;

[TestClass]
public class NotDefaultAttributeTest
{
    [TestMethod]
    [DataRow(null, true)]
    [DataRow("The field must not have the default value", true)]
    [DataRow(10, true)]
    [DataRow(0, false)]
    [DataRow(0.0, false)]
    [DataRow(10.3, true)]
    public void IsValid_ReturnsExpectedBool(object? value, bool expectedBool)
    {
        //Act
        var sut = new NotDefaultAttribute();
        var result = sut.IsValid(value);
        
        //Assert
        Assert.AreEqual(expectedBool, result);
    }
    
}