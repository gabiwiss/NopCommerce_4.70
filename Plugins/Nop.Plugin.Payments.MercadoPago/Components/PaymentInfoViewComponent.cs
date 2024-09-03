using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

public class PaymentInfoViewComponent : NopViewComponent
{
    public PaymentInfoViewComponent()
    {
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        return View("~/Plugins/Payments.MercadoPago/Views/PaymentInfo.cshtml");
    }

}