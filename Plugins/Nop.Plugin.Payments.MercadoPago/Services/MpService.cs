using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Plugin.Payments.MercadoPago.Countries;

namespace Nop.Plugin.Payments.MercadoPago.Services;
public class MpService
{
    private readonly ISettingService _settingService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IOrderService _orderService;
    private readonly IWebHelper _webHelper;
    public MpService(ISettingService settingService,
                     ICustomerService customerService,
                     IWorkContext workContext,
                     IStoreContext storeContext,
                     IOrderService orderService,
                     IWebHelper webHelper)
    {
        _settingService = settingService;
        _customerService = customerService;
        _workContext = workContext;
        _storeContext = storeContext;
        _orderService = orderService;
        _webHelper = webHelper;
    }

    public async Task<Preference> CreatePreference()
    {
        var baseUrl = _webHelper.GetStoreLocation();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var order = (await _orderService.SearchOrdersAsync(storeId: store.Id,
            customerId: customer.Id, pageSize: 1)).FirstOrDefault();

        var mercadoPagoSettings = _settingService.LoadSetting<MercadoPagoSettings>(store.Id);
        MercadoPagoConfig.AccessToken = !string.IsNullOrEmpty(mercadoPagoSettings.AccessToken)
            ? mercadoPagoSettings.AccessToken
            : _settingService.LoadSetting<MercadoPagoSettings>().AccessToken;

        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = "Orden Nro" + order.Id, 
                    Quantity = 1,
                    CurrencyId = order.CustomerCurrencyCode,
                    UnitPrice = order.OrderTotal,
                },
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = baseUrl + "checkout/completed/",
                Failure = baseUrl + "checkout/completed/",
                Pending = baseUrl + "checkout/completed/",
            },
            AutoReturn = "approved",
            ExternalReference = order.OrderGuid.ToString(),
        };

        // Crea la preferencia usando el client
        var client = new PreferenceClient();
        var preference = new Preference();
        try
        {
           preference = await client.CreateAsync(request);
        }
        catch (Exception)
        {
            await _orderService.DeleteOrderAsync(order);
            return new Preference() { AdditionalInfo = "Error" };
        }
        
        return preference; 
    }

    public async Task<(PaymentRefund, string)> RefundAsync(string captureTransaccionId, string currencyCode, decimal? amount)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        MercadoPagoConfig.AccessToken = _settingService.LoadSetting<MercadoPagoSettings>(store.Id).AccessToken;

        PaymentRefundClient client = new PaymentRefundClient();
        PaymentRefund refund = new PaymentRefund();
        try
        {
            refund = client.Refund(Convert.ToInt64(captureTransaccionId), amount);
        }
        catch (Exception ex)
        {

            return (refund, ex.Message);
        }
        

        return (refund, "");
    }

    public async Task<bool> TryConnection(ConnectionTestModel model)
    {
        MercadoPagoConfig.AccessToken = model.AccessTokenTest;

        var countries = CountryCurrencyData.GetCountryInfoList();
        var country = countries.Where(x => x.Id.ToString() == model.CountryIdTest).FirstOrDefault();

        var request = new PreferenceRequest()
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = "",
                    Quantity = 1,
                    CurrencyId = country.CurrencyId,
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
        var client = new PreferenceClient();
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
