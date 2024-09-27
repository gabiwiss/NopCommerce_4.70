using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.MercadoPago.Components;
public class CheckoutCompletedViewComponent : NopViewComponent
{
    public CheckoutCompletedViewComponent()
    {
            
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        return View("~/Plugins/Payments.MercadoPago/Views/CheckoutCompleted.cshtml");
    }
}
