using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.MercadoPago.Models;
using Nop.Plugin.Payments.MercadoPago.Services;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.MercadoPago.Controllers;

public class PreferenceController : Controller
{
    private readonly MpService _mpService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    public PreferenceController(MpService mpService,
                                IOrderService orderService,
                                ICustomerService customerService,
                                IWorkContext workContext,
                                IStoreContext storeContext)
    {
        _mpService = mpService;
        _orderService = orderService;
        _customerService = customerService;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    [HttpGet]
    public async Task<IActionResult> ExecutePreference(string modelJson)
    {
        if (string.IsNullOrEmpty(modelJson))
        {
            return View("Error");
        }

        // Deserializar el modelo desde el JSON
        var model = JsonConvert.DeserializeObject<Preference>(modelJson);

        if (model == null)
        {
            return View("Error");
        }

        if (model.AdditionalInfo == "Error")
        {
            ViewBag.AdditionalInfo = model.AdditionalInfo;
        }


        return View("~/Plugins/Payments.MercadoPago/Views/Preference.cshtml", model);
    }
    [HttpPost]
    public async Task<IActionResult> CreatePreference()
    {
        var model = await _mpService.CreatePreference();

        return Json(new { success = true, model });
    }

    [HttpPost]
    public async Task<IActionResult> TryConnection([FromBody] ConnectionTestModel model)
    {
        var response = await _mpService.TryConnection(model);
        return Json(response);  
    }
}
