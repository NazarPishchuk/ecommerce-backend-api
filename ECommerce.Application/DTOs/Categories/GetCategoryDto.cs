using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.DTOs.Categories;

public class GetCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
