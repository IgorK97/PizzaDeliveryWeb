using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaDeliveryWeb.Domain.Entities
{
    public class Delivery
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CourierId { get; set; }
        public DateTime? AcceptanceTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? Comment { get; set; }
        public virtual Order Order { get; set; }
        public virtual User Courier { get; set; }
    }
}
