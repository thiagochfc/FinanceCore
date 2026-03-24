using FinanceCore.Common.ValueObjects;

using Shouldly;

namespace FinanceCore.Common.Tests.ValueObjects;

public class DocumentNumberTests
{
    [Fact]
    public void DocumentNumberShouldBeCreateSuccessfully()
    {
        // Arrange
        const string value = "FAT001"; 
        
        // Act
        DocumentNumber documentNumber = DocumentNumber.Create("FAT001");

        // Assert
        documentNumber.Value.ShouldBe(value);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void DocumentNumberShouldNotBeCreatedDueToEmptyValue(string? value)
    {
        // Arrange
        Should.Throw<ArgumentException>(() => DocumentNumber.Create(value!));
    }

    [Fact]
    public void DocumentNumberShouldNotBeCreatedDueTooLong()
    {
        // Arrange
        const string value = "123456789012345678901234567890123456789012345678901"; 
        
        // Assert
        Should.Throw<TooLongDocumentNumberException>(() => DocumentNumber.Create(value));
    }
    
    [Fact]
    public void DocumentNumberShouldNotContainsControlChars()
    {
        // Arrange
        const string value = "1\n2\t3\r4\0";
        const string expected = "1234";
        
        // Act
        DocumentNumber documentNumber = DocumentNumber.Create(value);
        
        // Assert
        documentNumber.Value.ShouldBe(expected);
    }
}
