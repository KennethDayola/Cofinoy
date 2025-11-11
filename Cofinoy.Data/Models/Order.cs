using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data.Models
{

    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("OrderId")]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Nickname { get; set; }
        public string AdditionalRequest { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public List<OrderItem> OrderItems { get; set; }


       

    }


}
