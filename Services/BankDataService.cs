using BankProfiles.Web.Models;
using System.Text.Json;

namespace BankProfiles.Web.Services;

public interface IBankDataService
{
    Task<BankProfile?> GetBankByCodeAsync(string bankCode);
    Task<List<BankProfile>> GetAllBanksAsync();
    Task RefreshCacheAsync();
}

public class BankDataService : IBankDataService
{
    private readonly ICacheManager _cacheManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BankDataService> _logger;
    private readonly string _dataDirectory;
    private const string AllBanksCacheKey = "all_banks";

    public BankDataService(
        ICacheManager cacheManager,
        IConfiguration configuration,
        ILogger<BankDataService> logger)
    {
        _cacheManager = cacheManager;
        _configuration = configuration;
        _logger = logger;
        _dataDirectory = _configuration.GetValue<string>("BankDataSettings:DataDirectory") 
            ?? "wwwroot/data/banks";
    }

    public async Task<BankProfile?> GetBankByCodeAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return null;
        }

        var cacheKey = $"bank_{bankCode}";
        
        // Try to get from cache first
        var cachedBank = _cacheManager.Get<BankProfile>(cacheKey);
        if (cachedBank != null)
        {
            return cachedBank;
        }

        // Load from file
        try
        {
            var filePath = Path.Combine(_dataDirectory, $"{bankCode}.json");
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Bank file not found: {FilePath}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var bank = JsonSerializer.Deserialize<BankProfile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (bank != null)
            {
                // Cache the bank profile
                _cacheManager.Set(cacheKey, bank);
                _logger.LogInformation("Loaded and cached bank: {BankCode}", bankCode);
            }

            return bank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bank data for {BankCode}", bankCode);
            return null;
        }
    }

    public async Task<List<BankProfile>> GetAllBanksAsync()
    {
        // Try to get from cache first
        var cachedBanks = _cacheManager.Get<List<BankProfile>>(AllBanksCacheKey);
        if (cachedBanks != null)
        {
            return cachedBanks;
        }

        // Load all banks from files
        var banks = new List<BankProfile>();

        try
        {
            if (!Directory.Exists(_dataDirectory))
            {
                _logger.LogWarning("Bank data directory not found: {Directory}", _dataDirectory);
                return banks;
            }

            var jsonFiles = Directory.GetFiles(_dataDirectory, "*.json");
            
            foreach (var filePath in jsonFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var bank = JsonSerializer.Deserialize<BankProfile>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (bank != null)
                    {
                        banks.Add(bank);
                        
                        // Also cache individual bank
                        var cacheKey = $"bank_{bank.BankCode}";
                        _cacheManager.Set(cacheKey, bank);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading bank file: {FilePath}", filePath);
                }
            }

            // Cache the complete list
            _cacheManager.Set(AllBanksCacheKey, banks);
            _logger.LogInformation("Loaded and cached {Count} banks", banks.Count);

            return banks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all banks");
            return banks;
        }
    }

    public async Task RefreshCacheAsync()
    {
        _logger.LogInformation("Refreshing bank data cache");
        
        // Clear existing cache
        _cacheManager.Remove(AllBanksCacheKey);
        
        // Reload all banks (which will repopulate the cache)
        await GetAllBanksAsync();
    }
}
