using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow
{
    public async Task<ProductResponse> UpdateAsync(
        UpdateProductWorkflowCommand command,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
