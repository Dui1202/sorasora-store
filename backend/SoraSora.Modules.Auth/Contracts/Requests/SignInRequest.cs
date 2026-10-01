namespace SoraSora.Modules.Auth.Contracts.Requests;

public sealed record SignInRequest(string Email, string Password, bool RememberMe = false);
