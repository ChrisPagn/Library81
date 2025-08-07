using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Library81.Shared/DTOs/ItemDto.cs
namespace Library81.Shared.DTOs
{
    public class ItemDto
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
        public string? SubcategoryName { get; set; }
        public string? CategoryName { get; set; }
        public string ItemType { get; set; } = "Item";
    }
}