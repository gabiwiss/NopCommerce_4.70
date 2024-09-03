using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.MercadoPago.Factories;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Plugin.Payments.MercadoPago.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.MercadoPago.Controllers;
[AutoValidateAntiforgeryToken]
[AuthorizeAdmin] //confirms access to the admin panel
[Area(AreaNames.ADMIN)] //specifies the area containing a controller or action

public class PaymentsMercadoPagoController : BasePaymentController
{
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly MercadoPagoConfigurationModelFactory _mercadoPagoConfigurationModelFactory;
    private readonly IStoreContext _storeContext;

    public PaymentsMercadoPagoController(INotificationService notificationService,
                                        ISettingService settingService,
                                        ILocalizationService localizationService,
                                        MercadoPagoConfigurationModelFactory mercadoPagoConfigurationModelFactory,
                                        IStoreContext storeContext)

    {
        _notificationService = notificationService;
        _settingsService = settingService;
        _localizationService = localizationService;
        _mercadoPagoConfigurationModelFactory = mercadoPagoConfigurationModelFactory;
        _storeContext = storeContext;
    }


    public async Task<IActionResult> ConfigureAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingsService.LoadSettingAsync<MercadoPagoSettings>(storeId);
        var model = new ConfigurationModel()
        {
            AccessToken = settings.AccessToken,
            PublicKey = settings.PublicKey,
            CountryId = settings.CountryId,
            StoreId = storeId
        };

       model = await _mercadoPagoConfigurationModelFactory.PrepareMercadoPagoSettings(model);

        return View("~/Plugins/Payments.MercadoPago/Views/Configure.cshtml.",model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingsService.LoadSettingAsync<MercadoPagoSettings>(storeId);

            settings.PublicKey = model.PublicKey;
            settings.AccessToken = model.AccessToken;
            settings.CountryId = model.CountryId;   

        await _settingsService.SaveSettingAsync(settings,storeId);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await ConfigureAsync();
    }



}
