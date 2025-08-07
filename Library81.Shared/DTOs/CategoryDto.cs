// Library81.Shared/DTOs/CategoryDto.cs
using System.Collections.Generic;

namespace Library81.Shared.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<SubcategoryDto> Subcategories { get; set; } = new();
    }
}