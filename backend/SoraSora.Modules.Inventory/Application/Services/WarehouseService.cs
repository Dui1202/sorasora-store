using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Shared.Exceptions;

public sealed partial class WarehouseService(SoraSoraDbContext dbContext) : IWarehouseService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;

    public async Task<WarehouseResponse> CreateWarehouseAsync(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken
    )
    {
        var hasNameAlreadyExisted = await _dbContext
            .Warehouses()
            .AsNoTracking()
            .AnyAsync(x => x.Name == command.Name, cancellationToken);

        if (hasNameAlreadyExisted)
        {
            throw new ConflictException("Warehouse's name must be unique.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        var warehouse = Warehouse.Create(name: command.Name);

        if (command.IsDefault)
        {
            var currentDefaultWarehouse =
                await _dbContext
                    .Warehouses()
                    .SingleOrDefaultAsync(x => x.IsDefault, cancellationToken)
                ?? throw new InvalidOperationException(
                    "There must be exactly one default warehouse."
                );

            currentDefaultWarehouse.SetDefault(false);

            await _dbContext.SaveChangesAsync(cancellationToken);

            warehouse.SetDefault(command.IsDefault);
        }

        _dbContext.Warehouses().Add(warehouse);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return WarehouseDtoMapper.ToWarehouseResponse(warehouse);
    }

    public async Task DeleteWarehouseByIdAsync(string id, CancellationToken cancellationToken)
    {
        int deletedCount = await _dbContext
            .Warehouses()
            .Where(x => x.Id == id && !x.IsDefault)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            return;
        }

        bool isDefault = await _dbContext
            .Warehouses()
            .AnyAsync(x => x.Id == id && x.IsDefault, cancellationToken);

        if (isDefault)
        {
            throw new BadRequestException("You cannot delete default warehouse.");
        }
    }

    public async Task<IReadOnlyCollection<WarehouseResponse>?> GetAllWarehousesAsync(
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .Warehouses()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => WarehouseDtoMapper.ToWarehouseResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WarehouseResponse> GetWarehouseByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var warehouse =
            await _dbContext
                .Warehouses()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Warehouse is not found.");

        return WarehouseDtoMapper.ToWarehouseResponse(warehouse);
    }

    public async Task<WarehouseResponse> UpdateWarehouseAsync(
        UpdateWarehouseCommand command,
        CancellationToken cancellationToken
    )
    {
        var hasNameAlreadyExisted = await _dbContext
            .Warehouses()
            .AsNoTracking()
            .AnyAsync(x => x.Name == command.Name && x.Id != command.Id, cancellationToken);
        if (hasNameAlreadyExisted)
        {
            throw new ConflictException("Warehouse name must be unique.");
        }

        var warehouse =
            await _dbContext
                .Warehouses()
                .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Warehouse is not found.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        warehouse.UpdateDetails(name: command.Name);

        if (command.IsDefault && !warehouse.IsDefault)
        {
            var currentDefaultWarehouse =
                await _dbContext
                    .Warehouses()
                    .Where(x => x.Id != command.Id)
                    .SingleOrDefaultAsync(x => x.IsDefault, cancellationToken)
                ?? throw new InvalidOperationException(
                    "There must be exactly one default warehouse."
                );

            currentDefaultWarehouse.SetDefault(false);
            await _dbContext.SaveChangesAsync(cancellationToken);

            warehouse.SetDefault(command.IsDefault);
        }
        else if (!command.IsDefault && warehouse.IsDefault)
        {
            throw new ConflictException("There must be exactly one default warehouse.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return WarehouseDtoMapper.ToWarehouseResponse(warehouse);
    }
}
