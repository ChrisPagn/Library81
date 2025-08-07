using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Library81.Shared/DTOs/MovieDto.cs
namespace Library81.Shared.DTOs
{
    public class MovieDto : ItemDto
    {
        public int MovieId { get; set; }
        public TimeOnly? Duration { get; set; }
        public string? Rating { get; set; }

        public MovieDto()
        {
            ItemType = "Movie";
        }
    }
}
