using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofinoy.Data.Models
{

    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [Column("OrderItemId")]
        public int Id { get; set; }

        [ForeignKey("Order")]
        [Column("OrderId")]
        public int OrderId { get; set; }

        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public string Size { get; set; }
        public string MilkType { get; set; }
        public string Temperature { get; set; }
        public int ExtraShots { get; set; }
        public string SweetnessLevel { get; set; }

        public Order Order { get; set; }


     
    }

}
