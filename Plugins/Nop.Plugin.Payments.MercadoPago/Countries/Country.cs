using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.MercadoPago.Countries;
public enum CountryId
{
    AR, BR, CL, CO, MX, PE, UY, VE
}

public class CountryCurrencyInfo
{
    public CountryId Id { get; set; }
    public string Name { get; set; }
    public string Locale { get; set; }
    public string CurrencyId { get; set; }
    public string Symbol { get; set; }

    public CountryCurrencyInfo(CountryId id, string name, string locale, string currencyId, string symbol)
    {
        Id = id;
        Name = name;
        Locale = locale;
        CurrencyId = currencyId;
        Symbol = symbol;
    }
}

public static class CountryCurrencyData
{
    public static List<CountryCurrencyInfo> GetCountryInfoList()
    {
        return new List<CountryCurrencyInfo>
        {
            new CountryCurrencyInfo(CountryId.AR, "Argentine Peso", "es_AR", "ARS","AR$"),
            new CountryCurrencyInfo(CountryId.BR, "Brazilian Real", "pt_BR", "BRL", "R$"),
            new CountryCurrencyInfo(CountryId.CL, "Chilean Peso", "es_CL", "CLP", "CLP$"),
            new CountryCurrencyInfo(CountryId.CO, "Colombian Peso", "es_CO", "COP", "COP$"),
            new CountryCurrencyInfo(CountryId.MX, "Mexican Peso", "es_MX", "MXN", "MXN$"),
            new CountryCurrencyInfo(CountryId.PE, "Peruvian Sol", "es_PE", "PEN", "S/"),
            new CountryCurrencyInfo(CountryId.UY, "Uruguayan Peso", "es_UY", "UYU", "$U"),
            new CountryCurrencyInfo(CountryId.VE, "Bolivar", "es_VE", "VES", "Bs")
        };
    }
}
