using Microsoft.AspNetCore.Http.HttpResults;
using SoraSora.Shared.Exceptions;

public class InventoryDomainException(string message, string? field = null)
    : BadRequestException(
        message + (field is not null ? ": " + field : null),
        InventoryErrorCodes.DomainException
    );
