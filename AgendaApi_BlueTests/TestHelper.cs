using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AgendaApi_BlueTests;

public static class TestHelper
{
    public static ValidationResult ValidResult() => new();

    public static ValidationResult InvalidResult(string propertyName = "Field", string message = "Invalid") =>
        new(new[] { new ValidationFailure(propertyName, message) });

    public static void SetupValidatorSuccess<T>(Mock<IValidator<T>> validatorMock) where T : class
    {
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidResult());
    }

    public static void SetupMapper<TSource, TDest>(Mock<IMapper> mapperMock, TDest destination)
    {
        mapperMock
            .Setup(m => m.Map<TDest>(It.IsAny<TSource>()))
            .Returns(destination!);
    }
}
