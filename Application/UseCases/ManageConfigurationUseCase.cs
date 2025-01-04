using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LaunderWebApi.Entities;
using LaunderWebApi.Infrastructure.Dao;

public class ManageConfigurationUseCase : IConfigurationService
{
    private readonly IProprietorRepository _proprietorRepository;
    private readonly ILogger<ManageConfigurationUseCase> _logger;

    public ManageConfigurationUseCase(
        IProprietorRepository proprietorRepository,
        ILogger<ManageConfigurationUseCase> logger)
    {
        _proprietorRepository = proprietorRepository ??
            throw new ArgumentNullException(nameof(proprietorRepository));
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> AddConfigurationAsync(Proprietor configuration)
    {
        try
        {
            ValidateConfiguration(configuration);

            var proprietorId = await _proprietorRepository.AddProprietor(configuration);

            _logger.LogInformation(
                "Configuration added successfully for proprietor: {ProprietorName}",
                configuration.Name);

            return proprietorId;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(
                ex,
                "Validation error while adding configuration for {ProprietorName}",
                configuration?.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding configuration for proprietor {ProprietorName}",
                configuration?.Name);
            throw;
        }
    }

    public async Task<IEnumerable<Proprietor>> GetAllConfigurationsAsync()
    {
        try
        {
            var configurations = await _proprietorRepository.GetAllProprietors();

            _logger.LogInformation("Retrieved all configurations successfully");

            return configurations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all configurations");
            throw;
        }
    }

    private static void ValidateConfiguration(Proprietor configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ArgumentException("Proprietor name is required", nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.Email))
        {
            throw new ArgumentException("Proprietor email is required", nameof(configuration));
        }

        if (configuration.Laundries == null || !configuration.Laundries.Any())
        {
            throw new ArgumentException("At least one laundry is required", nameof(configuration));
        }
    }
}
