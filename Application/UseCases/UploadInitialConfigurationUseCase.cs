using LaunderManagerWebApi.Domain.Services.InfrastructureServices;
using LaunderWebApi.Entities;
using Laundromat.Core.Interfaces;
using System.Text.Json;

namespace Laundromat.Application.UseCases
{
    public class UploadInitialConfigurationUseCase
    {
        private readonly IDaoProprietor _proprietorRepository;
        private readonly IWebSocketService _webSocketService;

        public UploadInitialConfigurationUseCase(
            IDaoProprietor proprietorRepository,
            IWebSocketService webSocketService)
        {
            _proprietorRepository = proprietorRepository;
            _webSocketService = webSocketService;
        }

        public async Task ExecuteAsync(Proprietor configurationModel)
        {
            if (configurationModel == null)
                throw new ArgumentException("Configuration file cannot be empty.");

            // Create a list of Proprietor from ConfigurationModel
            var proprietors = new List<Proprietor>
    {
        new Proprietor
        {
            Name = configurationModel.Name,
            Email = configurationModel.Email,
            TotalEarnings = configurationModel.TotalEarnings,
            Laundries = configurationModel.Laundries
        }
    };

            // Validation of the data before insertion
            foreach (var proprietor in proprietors)
            {
                if (string.IsNullOrWhiteSpace(proprietor.Name) || string.IsNullOrWhiteSpace(proprietor.Email))
                    throw new ArgumentException("Proprietor name and email are required.");

                // Additional validation can be added here for laundries or other fields
                if (proprietor.Laundries == null || !proprietor.Laundries.Any())
                    throw new ArgumentException("At least one laundry is required for each proprietor.");
            }

            // Save the proprietor to the database
            foreach (var proprietor in proprietors)
            {
                try
                {
                    await _proprietorRepository.AddProprietor(proprietor);
                }
                catch (Exception ex)
                {
                    // Log the error or handle rollback as necessary
                    throw new InvalidOperationException($"Error saving proprietor {proprietor.Name}.", ex);
                }
            }

            // Broadcast the configuration via WebSocket
            try
            {
                var jsonConfiguration = JsonSerializer.Serialize(configurationModel); // Serialize the model to JSON
                await _webSocketService.BroadcastMessageAsync(jsonConfiguration); // Send the message
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending configuration to WebSocket.", ex);
            }
        }

    }
}
