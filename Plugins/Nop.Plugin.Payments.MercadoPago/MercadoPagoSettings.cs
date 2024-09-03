using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.MercadoPago;
public class MercadoPagoSettings : ISettings
{
    public MercadoPagoSettings()
    {   
    }
    public string AccessToken { get; set; }
    public string PublicKey { get; set; }
    public string CountryId { get; set; }
    public bool IsActive { get; set; }

}
