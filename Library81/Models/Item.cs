using System;
using System.Collections.Generic;

namespace Library81.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string Title { get; set; } = null!;

    public string? Creator { get; set; }

    public string? Publisher { get; set; }

    public short? Year { get; set; }

    public string? Description { get; set; }

    public int? SubcategoryId { get; set; }

    public DateTime? DateAdded { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Game? Game { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual Subcategory? Subcategory { get; set; }
}
