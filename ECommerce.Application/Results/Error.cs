namespace ECommerce.Application.Results;

public record Error(string Code, ErrorType Type, string Message);

