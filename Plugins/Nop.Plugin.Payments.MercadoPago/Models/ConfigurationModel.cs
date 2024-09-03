using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Stores;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.MercadoPago.Models;
public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        AvailableCountries = [];
        Stores = [];
    }
    public string PublicKey { get; set; }

    public string AccessToken { get; set; }

    public string CountryId { get; set; }
    public int StoreId { get; set; }
    public IList<SelectListItem> AvailableCountries { get; set; }
    public IList<Store> Stores { get; set; }
}
