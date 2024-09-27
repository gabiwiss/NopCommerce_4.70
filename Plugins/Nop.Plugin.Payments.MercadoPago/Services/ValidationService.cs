using MercadoPago.Client.Preference;
using MercadoPago.Config;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Payments.MercadoPago.Countries;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.MercadoPago.Services;
public class ValidationService
{
    private readonly IWorkContext _workContext;
    private readonly ISettingService _settingService;
    public ValidationService(IWorkContext workContext,
                             ISettingService settingService)
    {
        _settingService = settingService;
        _workContext = workContext;
    }
    public async Task<bool> ValidateStoreAsync(Store store)
    {
        var multiStoreSetting = _settingService.LoadSetting<MercadoPagoMultiStoreSettings>();
        var mercadoPagoSettings = _settingService.LoadSetting<MercadoPagoSettings>(multiStoreSetting.Enabled ? store.Id : 0);
        
        if (multiStoreSetting.Enabled && !UrlAvailables.GetUrlsAvailables().Contains(store.Url)) // validar urls cargadas
            return false;

        MercadoPagoConfig.AccessToken = mercadoPagoSettings.AccessToken;

        var currentCountryId = mercadoPagoSettings.CountryId;
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var countries = CountryCurrencyData.GetCountryInfoList();
        var country = countries.Where(x => x.Id.ToString() == currentCountryId).FirstOrDefault();

        if (country == null || currentCurrency.CurrencyCode != country.CurrencyId)
        {
            return false;
        }

        var client = new PreferenceClient();
        var request = new PreferenceRequest()
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = "",
                    Quantity = 1,
                    CurrencyId = currentCurrency.CurrencyCode,
                    UnitPrice = 1
                },
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = "test",
                Failure = "test",
                Pending = "test",
            },
            AutoReturn = "approved",
        };
        try
        {
            var preference = await client.CreateAsync(request);
        }
        catch (Exception)
        {

            return false;
        }

        return true;
    }
}
