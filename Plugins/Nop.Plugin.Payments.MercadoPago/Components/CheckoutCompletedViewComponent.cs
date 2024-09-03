using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
