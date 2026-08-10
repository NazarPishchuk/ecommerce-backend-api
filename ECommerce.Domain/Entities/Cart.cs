using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public string UserId { get; set; } = null!;
}
