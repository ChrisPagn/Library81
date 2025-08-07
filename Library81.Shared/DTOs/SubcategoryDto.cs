using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Library81.Shared/DTOs/SubcategoryDto.cs
namespace Library81.Shared.DTOs
{
    public class SubcategoryDto
    {
        public int SubcategoryId { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
    }
}
