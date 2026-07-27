using Microsoft.EntityFrameworkCore;

namespace SoraSora.Infrastructure.Persistence.Interface;

public interface IModelBuilderContributor
{
    void Configure(ModelBuilder modelBuilder);
}
