namespace SoraSora.Api.Contracts.Commands.ProductWorkflow;

public sealed record CreateProductVariantImageWorkflowCommand
{
    public required string ClientId { get; init; }
    public required IFormFile File { get; init; }
    public string? Alt { get; init; }
}
