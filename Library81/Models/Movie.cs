using System;
using System.Collections.Generic;

namespace Library81.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    public int ItemId { get; set; }

    public TimeOnly? Duration { get; set; }

    public string? Rating { get; set; }

    public virtual Item Item { get; set; } = null!;
}
