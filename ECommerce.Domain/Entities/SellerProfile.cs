using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities;

public class SellerProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string StoreName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
