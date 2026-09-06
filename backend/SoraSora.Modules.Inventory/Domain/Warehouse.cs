using NUlid;

namespace SoraSora.Modules.Inventory.Domain
{
    public class Warehouse
    {
        public const int MaxNameLength = 100;
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string Name { get; private set; } = null!;
        public bool IsDefault { get; private set; } = false;

        public static Warehouse Create(string name)
        {
            return new Warehouse { Name = name };
        }

        public void UpdateDetails(string name)
        {
            Name = name;
        }

        public void SetDefault(bool isDefault)
        {
            IsDefault = isDefault;
        }
    }
}
