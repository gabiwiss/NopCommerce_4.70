using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.MercadoPago;
public static class UrlAvailables
{

    public static List<string> GetUrlsAvailables()
    {
        return new List<string>() { "https://localhost:5001/", "https://storehost2:5002/" };
    }
}
