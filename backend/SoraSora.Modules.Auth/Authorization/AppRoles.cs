namespace SoraSora.Modules.Auth.Authorization;

public static class AppRoles
{
    public const string Owner = nameof(Owner);
    public const string Admin = nameof(Admin);
    public const string Staff = nameof(Staff);

    public static readonly IReadOnlyCollection<string> All = [Owner, Admin, Staff];
}
