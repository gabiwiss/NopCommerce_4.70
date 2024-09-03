using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.MercadoPago.Models;

public class Data
{
    public string id { get; set; }

}
public class MpWebHookModel
{
    public string id { get; set; }
    public bool live_mode { get; set; }
    public string type { get; set; }
    public DateTime date_created { get; set; }
    public int user_id { get; set; }
    public string api_version { get; set; }
    public string action { get; set; }
    public Data data { get; set; }

}

