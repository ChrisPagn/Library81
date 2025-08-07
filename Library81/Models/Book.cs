using System;
using System.Collections.Generic;

namespace Library81.Models;

public partial class Book
{
    public int BookId { get; set; }

    public int ItemId { get; set; }

    public int? GenreId { get; set; }

    public string? Isbn { get; set; }

    public virtual Genre? Genre { get; set; }

    public virtual Item Item { get; set; } = null!;
}
