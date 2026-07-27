namespace SoraSora.Modules.Auth.Contracts;

public sealed record SignInRequest(string Email, string Password, bool RememberMe = false);
