using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.MercadoPago.Models;
public class PaymentResponseModel
{

    public string collection_id { get; set; }
    public string collection_status { get; set; }
    public string payment_id { get; set; }
    public string status { get; set; }
    public string external_reference { get; set; }
    public string payment_type { get; set; }
    public string merchant_order_id { get; set; }
    public string preference_id { get; set; }
    public string site_id { get; set; }
    public string processing_mode { get; set; }
    public string merchant_account_id { get; set; }

}
