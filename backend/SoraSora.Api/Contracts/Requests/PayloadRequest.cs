namespace SoraSora.Api.Contracts.Requests;

public sealed record PayloadRequest
{
    public required string Payload { get; init; }
}
