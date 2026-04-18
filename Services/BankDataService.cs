using BankProfiles.Web.Models;
using System.Text.Json;

namespace BankProfiles.Web.Services;

public class BankDataService : IBankDataService
{
    private readonly ICacheManager _cacheManager;
    private readonly IEventProjectionService _projectionService;
    private readonly IEventStoreService _eventStoreService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BankDataService> _logger;
    private readonly string _dataDirectory;
    private const string AllBanksCacheKey = "all_banks";

    public BankDataService(
        ICacheManager cacheManager,
        IEventProjectionService projectionService,
        IEventStoreService eventStoreService,
        IConfiguration configuration,
        ILogger<BankDataService> logger)
    {
        _cacheManager = cacheManager;
        _projectionService = projectionService;
        _eventStoreService = eventStoreService;
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
            SanitizeBrandingAssets(cachedBank, bankCode);
            return cachedBank;
        }

        // Try to project from event store
        try
        {
            if (await _eventStoreService.HasEventsAsync(bankCode))
            {
                var bank = await _projectionService.ProjectBankProfileAsync(bankCode);
                if (bank != null)
                {
                    SanitizeBrandingAssets(bank, bankCode);
                    _cacheManager.Set(cacheKey, bank);
                    _logger.LogInformation("Projected and cached bank from events: {BankCode}", bankCode);
                    return bank;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error projecting bank from events for {BankCode}", bankCode);
        }

        // Fallback: load from JSON file (migration period)
        return await LoadFromJsonFileAsync(bankCode, cacheKey);
    }

    public async Task<List<BankProfile>> GetAllBanksAsync()
    {
        var cachedBanks = _cacheManager.Get<List<BankProfile>>(AllBanksCacheKey);
        if (cachedBanks != null)
        {
            foreach (var cachedBank in cachedBanks)
            {
                SanitizeBrandingAssets(cachedBank, cachedBank.BankCode);
            }

            return cachedBanks;
        }

        var banksByCode = new Dictionary<string, BankProfile>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var eventBackedCount = 0;

            // Event-backed banks first
            var bankCodes = await _eventStoreService.GetAllBankCodesAsync();
            foreach (var bankCode in bankCodes)
            {
                var bank = await GetBankByCodeAsync(bankCode);
                if (bank == null)
                {
                    continue;
                }

                banksByCode[bank.BankCode] = bank;
                eventBackedCount++;
            }

            // Merge in legacy JSON-backed banks for mixed migration periods.
            var jsonBanks = await LoadAllFromJsonFilesAsync();
            foreach (var jsonBank in jsonBanks)
            {
                if (!banksByCode.ContainsKey(jsonBank.BankCode))
                {
                    banksByCode[jsonBank.BankCode] = jsonBank;
                }
            }

            var mergedBanks = banksByCode.Values.ToList();
            _cacheManager.Set(AllBanksCacheKey, mergedBanks);

            var jsonFallbackCount = Math.Max(0, mergedBanks.Count - eventBackedCount);
            _logger.LogInformation(
                "Loaded and cached {TotalCount} banks ({EventCount} from events, {JsonCount} from JSON fallback)",
                mergedBanks.Count,
                eventBackedCount,
                jsonFallbackCount);

            return mergedBanks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all banks");
            return banksByCode.Values.ToList();
        }
    }

    public async Task RefreshCacheAsync()
    {
        _logger.LogInformation("Refreshing bank data cache");
        _cacheManager.Remove(AllBanksCacheKey);
        await GetAllBanksAsync();
    }

    private async Task<BankProfile?> LoadFromJsonFileAsync(string bankCode, string cacheKey)
    {
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
                SanitizeBrandingAssets(bank, bankCode);
                _cacheManager.Set(cacheKey, bank);
                _logger.LogInformation("Loaded and cached bank from JSON: {BankCode}", bankCode);
            }

            return bank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bank data for {BankCode}", bankCode);
            return null;
        }
    }

    private async Task<List<BankProfile>> LoadAllFromJsonFilesAsync()
    {
        var banks = new List<BankProfile>();

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
                    SanitizeBrandingAssets(bank, bank.BankCode);
                    banks.Add(bank);
                    _cacheManager.Set($"bank_{bank.BankCode}", bank);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bank file: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Loaded and cached {Count} banks from JSON files", banks.Count);
        return banks;
    }

    private void SanitizeBrandingAssets(BankProfile bank, string bankCode)
    {
        if (bank.Overview == null)
            return;

        var originalLogoUrl = bank.Overview.LogoUrl;
        bank.Overview.LogoUrl = ValidationHelper.NormalizeBankAssetPath(originalLogoUrl);
        if (bank.Overview.LogoUrl == null && !string.IsNullOrWhiteSpace(originalLogoUrl))
        {
            _logger.LogWarning("Invalid overview.logoUrl ignored for bank {BankCode}: {LogoUrl}", bankCode, originalLogoUrl);
        }

        var originalIconUrl = bank.Overview.IconUrl;
        bank.Overview.IconUrl = ValidationHelper.NormalizeBankAssetPath(originalIconUrl);
        if (bank.Overview.IconUrl == null && !string.IsNullOrWhiteSpace(originalIconUrl))
        {
            _logger.LogWarning("Invalid overview.iconUrl ignored for bank {BankCode}: {IconUrl}", bankCode, originalIconUrl);
        }
    }
}
