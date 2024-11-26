using System;
using System.Collections.Generic;

namespace ebay.Models;

public partial class User
{
    public string UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
