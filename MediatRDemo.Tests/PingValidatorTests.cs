using FluentAssertions;
using MediatRDemo.RequestHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediatRDemo.Tests
{
    [TestClass]
    public class PingValidatorTests
    {
        [TestMethod]
        public void EmptyResponseMessage_ShouldThrowValidationException()
        {
            var _sut = new PingValidator();
            var ping = new Ping { ResponseMessage = null };

            var validationResult = _sut.Validate(ping);

            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().NotBeEmpty();
            validationResult.Errors[0].ErrorMessage.Should().Be("We need to know what you want from us");
        }

        [TestMethod]
        public void LargeResponseMessage_ShouldThrowValidationException()
        {
            var _sut = new PingValidator();
            var ping = new Ping { ResponseMessage = new string('*', 513) };

            var validationResult = _sut.Validate(ping);

            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().NotBeEmpty();
            validationResult.Errors[0].ErrorMessage.Should().Be("We will not reply with more than 512 bytes");
        }
    }
}
