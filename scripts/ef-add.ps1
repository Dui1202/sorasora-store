param(
    [Parameter(Mandatory = $true)]
    [string]$Name
)

dotnet ef migrations add $Name `
  --project backend/SoraSora.Infrastructure/SoraSora.Infrastructure.csproj `
  --startup-project backend/SoraSora.Api/SoraSora.Api.csproj `
  --context SoraSoraDbContext `
  --output-dir Persistence/Migrations