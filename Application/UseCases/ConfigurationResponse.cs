using LaunderWebApi.Entities;

public record ConfigurationResponse(
    bool Success,
    string Message,
    Proprietor? Data = null);
