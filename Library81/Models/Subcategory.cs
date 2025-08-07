using System;
using System.Collections.Generic;

namespace Library81.Models;

public partial class Subcategory
{
    public int SubcategoryId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
