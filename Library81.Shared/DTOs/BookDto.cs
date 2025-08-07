using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Library81.Shared/DTOs/BookDto.cs
namespace Library81.Shared.DTOs
{
    public class BookDto : ItemDto
    {
        public int BookId { get; set; }
        public int? GenreId { get; set; }
        public string? Isbn { get; set; }
        public string? GenreName { get; set; }

        public BookDto()
        {
            ItemType = "Book";
        }
    }

}
