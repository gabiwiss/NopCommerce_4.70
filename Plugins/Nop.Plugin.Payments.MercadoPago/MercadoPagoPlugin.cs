using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Manual;
using Nop.Plugin.Payments.MercadoPago.Components;
using Nop.Plugin.Payments.MercadoPago.Countries;
using Nop.Plugin.Payments.MercadoPago.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.MercadoPago;

public class MercadoPagoPlugin : BasePlugin, IPaymentMethod, IWidgetPlugin
{
    #region Fields
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly MpService _mpService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ValidationService _validationService;
    private readonly INotificationService _notificationService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IReturnRequestService _returnRequestService;
    private readonly IOrderService _orderService;
    private readonly ILanguageService _languageService;
    #endregion
    //public PluginDescriptor PluginDescriptor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #region Ctor
    public MercadoPagoPlugin(IWebHelper webHelper,
                             ILocalizationService localizationService,
                             MpService mpService,
                             ISettingService settingService,
                             ValidationService validationService,
                             IStoreContext storeContext,
                             INotificationService notificationService,
                             ICurrencyService currencyService,
                             CurrencySettings currencySettings,
                             IGenericAttributeService genericAttributeService,
                             IOrderService orderService,
                             ILanguageService languageService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _mpService = mpService;
        _settingService = settingService;
        _validationService = validationService;
        _storeContext = storeContext;
        _notificationService = notificationService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _genericAttributeService = genericAttributeService;
        _orderService = orderService;
        _languageService = languageService;
    }

    public bool SupportCapture => false;

    public bool SupportPartiallyRefund => true;

    public bool SupportRefund => true;

    public bool SupportVoid => false;

    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard; // DEBE DEVOLVER Redirection

    public bool SkipPaymentInfo => true;

    public bool HideInWidgetList => false;
    public Preference Preference { get; set; }

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanRePostProcessPaymentAsync(Order order) // REINTENTAR PAGO EN CASO DE QUE NO FUNCIONE
    {
        return Task.FromResult(false);
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult((decimal)0);
    }
    #endregion

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentsMercadoPago/Configure";
    }

    public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        return Task.FromResult(new ProcessPaymentRequest());
    }

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payment.MercadoPagoPaymentMethodDescription");
    }

    public Type GetPublicViewComponent()
    {
        return typeof(PaymentInfoViewComponent);
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.OpCheckoutConfirmBottom)
        {
            var type = typeof(PaymentInfoViewComponent);
            return type;
        }
        if (widgetZone == PublicWidgetZones.CheckoutCompletedTop)
        {
            var type = typeof(CheckoutCompletedViewComponent);
            return type;
        }
        return null;
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        var isActive = _settingService.LoadSetting<MercadoPagoSettings>().IsActive;
        if (isActive)
            return await Task.FromResult(new List<string>() { "op_checkout_confirm_bottom", "checkout_completed_top" });
        else
            return await Task.FromResult(new List<string>());
    }

    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        var store = _storeContext.GetCurrentStore();

        var isValid = await _validationService.ValidateStoreAsync(store);
        var settings = await _settingService.LoadSettingAsync<MercadoPagoSettings>();
        settings.IsActive = isValid;
        await _settingService.SaveSettingAsync(settings);

        return !isValid;
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new MercadoPagoSettings
        {
            AccessToken = "",
            PublicKey = "",
            CountryId = "",
            IsActive = false,
        });
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payment.MercadoPagoPaymentMethodDescription"] = "Pay using MercadoPago payment method",
            ["Plugins.Payment.MercadoPagoPayment.Country.None"] = "None"
        });

        var countries = CountryCurrencyData.GetCountryInfoList();
        foreach (var country in countries)
        {
            var currency = await _currencyService.GetCurrencyByCodeAsync(country.CurrencyId);
            if (currency == null)
                await _currencyService.InsertCurrencyAsync(new Core.Domain.Directory.Currency()
                {
                    CurrencyCode = country.CurrencyId,
                    Name = country.Name,
                    CustomFormatting = country.Symbol,
                    DisplayLocale = country.Locale,
                    CreatedOnUtc = DateTime.UtcNow,
                });
        }

        await base.InstallAsync();
    }

    public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest) // USAR PARA MERCADO PAGO
    {
        //var orderSubtotal = postProcessPaymentRequest.Order.OrderSubTotalDiscountInclTax;
        //var preference = _mpService.ConfigureAsync(orderSubtotal);


        //return View("~/Plugins/Nop.Plugin.Payments.MercadoPago/Views/Index.cshtml.", preference);

        return Task.FromResult(true);
    }

    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult());
    }

    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        //var orderItems = _orderService.GetOrderItemsAsync(orderId: refundPaymentRequest.Order.Id);
        //var returnRequest = await _returnRequestService.SearchReturnRequestsAsync(,);
        //if (returnRequest.)
        //{
            
        //}

        //refund previously captured payment
        var amount = refundPaymentRequest.AmountToRefund != refundPaymentRequest.Order.OrderTotal
            ? (decimal?)refundPaymentRequest.AmountToRefund
            : null;

        //get the primary store currency
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)
                       ?? throw new NopException("Primary store currency cannot be loaded");

        var (refund, error) = await _mpService.RefundAsync(
             refundPaymentRequest.Order.CaptureTransactionId, currency.CurrencyCode, amount);

        if (!string.IsNullOrEmpty(error))
            return new RefundPaymentResult { Errors = new[] { error } };

        //request succeeded
        var refundIds = await _genericAttributeService
                            .GetAttributeAsync<List<string>>(refundPaymentRequest.Order, MercadoPagoDefaults.RefundIdAttributeName)
                        ?? [];
        if (!refundIds.Contains(refund.Id.ToString()))
            refundIds.Add(refund.Id.ToString());
        await _genericAttributeService.SaveAttributeAsync(refundPaymentRequest.Order, MercadoPagoDefaults.RefundIdAttributeName, refundIds);

        if (refund.Status == "approved")
            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? Core.Domain.Payments.PaymentStatus.PartiallyRefunded : Core.Domain.Payments.PaymentStatus.Refunded
            };
        else if (refund.Status == "in_proces")
            return new RefundPaymentResult
            {
                NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Pending
            };
        else
        {
            return new RefundPaymentResult
            {
                NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Voided
            };
        }

    }

    //public Task PreparePluginToUninstallAsync()
    //{
    //    throw new NotImplementedException();
    //}

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<MercadoPagoSettings>();
        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        return Task.FromResult<IList<string>>(new List<string>());
    }

    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        throw new NotImplementedException();
    }

}
