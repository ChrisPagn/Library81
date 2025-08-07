using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Library81.Shared/DTOs/GameDto.cs
namespace Library81.Shared.DTOs
{
    public class GameDto : ItemDto
    {
        public int GameId { get; set; }
        public string? Platform { get; set; }
        public string? AgeRange { get; set; }

        public GameDto()
        {
            ItemType = "Game";
        }
    }
}
