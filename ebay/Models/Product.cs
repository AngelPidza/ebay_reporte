using System;
using System.Collections.Generic;

namespace ebay.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string Category { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
