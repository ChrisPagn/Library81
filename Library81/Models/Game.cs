using System;
using System.Collections.Generic;

namespace Library81.Models;

public partial class Game
{
    public int GameId { get; set; }

    public int ItemId { get; set; }

    public string? Platform { get; set; }

    public string? AgeRange { get; set; }

    public virtual Item Item { get; set; } = null!;
}
