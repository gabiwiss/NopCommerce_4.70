using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Client.PaymentMethod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using MercadoPago.Resource.Preference;
using MercadoPago.Client.Preference;

namespace Nop.Plugin.Payments.MercadoPago.Controllers;
public class PaymentsMercadoPagoWebhookController : Controller
{
    #region Fields
        IOrderService _orderService;
    ISettingService _settingService;
    #endregion

    #region Ctor

    public PaymentsMercadoPagoWebhookController(IOrderService orderService,
                                                ISettingService settingService)
    {
        _orderService = orderService;
        _settingService = settingService;
    }

    #endregion

    #region Methods
    [HttpPost]
    [AllowAnonymous]
    public IActionResult WebhookHandler([FromBody] MpWebHookModel mpWebHookModel)
    {
        if (mpWebHookModel == null)
        {

        }
        return Ok();
    }

    public async Task<IActionResult> UpdateOrderStatusAsync([FromBody] PaymentResponseModel paymentResponseModel)
    {
        if (paymentResponseModel == null)
        {
            return BadRequest();
        }

        var pClient = new PaymentClient();
        Payment payment = await pClient.GetAsync(Convert.ToInt64(paymentResponseModel.payment_id));

        if (payment.Status == "approved")
        {
            var guid = new Guid(paymentResponseModel.external_reference);
            var order = await _orderService.GetOrderByGuidAsync(guid);
            order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
            order.OrderStatus = Core.Domain.Orders.OrderStatus.Complete;
            order.CaptureTransactionId=payment.Id.ToString();
            await _orderService.UpdateOrderAsync(order);
        }


        return Ok();
    }
    #endregion
}
