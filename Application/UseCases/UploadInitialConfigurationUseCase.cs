using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderManagerWebApi.Domain.Services.InfrastructureServices;
using LaunderWebApi.Entities;
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

            // Validation des données avant l'insertion
            foreach (var proprietor in proprietors)
            {
                if (string.IsNullOrWhiteSpace(proprietor.Name) || string.IsNullOrWhiteSpace(proprietor.Email))
                    throw new ArgumentException("Proprietor name and email are required.");

                if (proprietor.Laundries == null || !proprietor.Laundries.Any())
                    throw new ArgumentException("At least one laundry is required for each proprietor.");
            }

            // Sauvegarder dans la base de données
            foreach (var proprietor in proprietors)
            {
                try
                {
                    await _proprietorRepository.AddProprietor(proprietor);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error saving proprietor {proprietor.Name}.", ex);
                }
            } 

            // Diffuser la configuration via WebSocket
            try
            {
                var jsonConfiguration = JsonSerializer.Serialize(configurationModel);
                await _webSocketService.BroadcastMessageAsync(jsonConfiguration);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending configuration to WebSocket.", ex);
            }
        }
    }
}
