using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Payments.MercadoPago.Countries;
using Nop.Services.Configuration;
using Nop.Services.Directory;

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
        
        if (!UrlAvailables.GetUrlsAvailables().Contains(store.Url)) // validar urls cargadas
            return false;

        var mercadoPagoSettings = _settingService.LoadSetting<MercadoPagoSettings>(store.Id);
        MercadoPagoConfig.AccessToken = !string.IsNullOrEmpty(mercadoPagoSettings.AccessToken)
            ? mercadoPagoSettings.AccessToken
            : _settingService.LoadSetting<MercadoPagoSettings>().AccessToken;

        
        var currentCountryId = !string.IsNullOrEmpty(mercadoPagoSettings.CountryId)
            ? mercadoPagoSettings.CountryId
            : _settingService.LoadSetting<MercadoPagoSettings>().CountryId;

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
