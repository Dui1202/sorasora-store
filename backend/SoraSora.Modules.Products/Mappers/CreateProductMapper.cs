using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Mappers
{
    internal static class CreateProductDtoMapper
    {
        internal static CreateProductCommand ToCreateProductCommand(CreateProductRequest request)
        {
            return new CreateProductCommand(
                request.Name,
                request.Description,
                request.SupplierCountry,
                request.IsPublished,
                request.ProductImages.Select(ToCreateProductImageCommand).ToList()
            );
        }

        private static CreateProductImageCommand ToCreateProductImageCommand(
            CreateProductImageRequest request
        )
        {
            return new CreateProductImageCommand(request.File, request.Alt, request.Position);
        }
    }
}
