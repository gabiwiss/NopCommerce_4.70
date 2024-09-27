using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.MercadoPago.Countries;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Services.Localization;
using Nop.Services.Stores;
using NUglify.Helpers;

namespace Nop.Plugin.Payments.MercadoPago.Factories;
public class MercadoPagoConfigurationModelFactory
{
    private readonly IStoreService _storeService;
    private readonly ILocalizationService _localizationService;
    public MercadoPagoConfigurationModelFactory(IStoreService storeService,
                                                ILocalizationService localizationService)
    {
        _storeService = storeService;
        _localizationService = localizationService;
    }

    public async Task<ConfigurationModel> PrepareMercadoPagoSettings(ConfigurationModel model)
    {
        var countries = CountryCurrencyData.GetCountryInfoList();

        model.AvailableCountries.Add(new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Plugins.Payment.MercadoPagoPayment.Country.None"),
            Value = ""
        });

        foreach (var country in countries)
        {
            model.AvailableCountries.Add(new SelectListItem
            {
                Text = country.Name,
                Value = country.Id.ToString()
            });
        }

        model.Stores = _storeService.GetAllStoresAsync().Result.Where(x => UrlAvailables.GetUrlsAvailables().Contains(x.Url)).ToList();



        return model;
    }
}
