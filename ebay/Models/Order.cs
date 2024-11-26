using System;
using System.Collections.Generic;

namespace ebay.Models;

public partial class Order
{
    public string OrderId { get; set; }

    public string? UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
