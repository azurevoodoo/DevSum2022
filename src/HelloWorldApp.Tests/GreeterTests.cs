using HelloWorldApp.Common;
using Xunit;

namespace HelloWorldApp.Tests;

public class TheGretater
{
    [Fact]
    public void TheGreetMethod()
    {
        // Given
        var greeter = new Greeter();

        // When
        var result = greeter.Greet();

        // Then
        Assert.Equal("Hello World!", result);
    }
}