// Library81/Services/BookService.cs
using Library81.Models;
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Library81.Services;

public class BookService : IBookService
{
    private readonly LibraryContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(LibraryContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<BookDto>>> GetAllBooksAsync()
    {
        try
        {
            var books = await _context.Books
                .Include(b => b.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .Include(b => b.Genre)
                .Select(b => MapToDto(b))
                .ToListAsync();

            return new ApiResponse<List<BookDto>>
            {
                Success = true,
                Data = books
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all books");
            return new ApiResponse<List<BookDto>>
            {
                Success = false,
                Message = "Une erreur est survenue lors de la récupération des livres",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<BookDto>> GetBookByIdAsync(int id)
    {
        try
        {
            var book = await _context.Books
                .Include(b => b.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return new ApiResponse<BookDto>
                {
                    Success = false,
                    Message = "Livre non trouvé"
                };
            }

            return new ApiResponse<BookDto>
            {
                Success = true,
                Data = MapToDto(book)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book {BookId}", id);
            return new ApiResponse<BookDto>
            {
                Success = false,
                Message = "Une erreur est survenue",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<BookDto>> CreateBookAsync(BookDto bookDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Créer d'abord l'item
            var item = new Item
            {
                Title = bookDto.Title,
                Creator = bookDto.Creator,
                Publisher = bookDto.Publisher,
                Year = bookDto.Year,
                Description = bookDto.Description,
                SubcategoryId = bookDto.SubcategoryId,
                ImageUrl = bookDto.ImageUrl,
                DateAdded = DateTime.Now
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            // Créer ensuite le livre
            var book = new Book
            {
                ItemId = item.ItemId,
                GenreId = bookDto.GenreId,
                Isbn = bookDto.Isbn
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            bookDto.ItemId = item.ItemId;
            bookDto.BookId = book.BookId;

            return new ApiResponse<BookDto>
            {
                Success = true,
                Data = bookDto,
                Message = "Livre créé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating book");
            return new ApiResponse<BookDto>
            {
                Success = false,
                Message = "Erreur lors de la création du livre",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<BookDto>> UpdateBookAsync(int id, BookDto bookDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var book = await _context.Books
                .Include(b => b.Item)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return new ApiResponse<BookDto>
                {
                    Success = false,
                    Message = "Livre non trouvé"
                };
            }

            // Mettre à jour l'item
            book.Item.Title = bookDto.Title;
            book.Item.Creator = bookDto.Creator;
            book.Item.Publisher = bookDto.Publisher;
            book.Item.Year = bookDto.Year;
            book.Item.Description = bookDto.Description;
            book.Item.SubcategoryId = bookDto.SubcategoryId;
            book.Item.ImageUrl = bookDto.ImageUrl;

            // Mettre à jour le livre
            book.GenreId = bookDto.GenreId;
            book.Isbn = bookDto.Isbn;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<BookDto>
            {
                Success = true,
                Data = bookDto,
                Message = "Livre mis à jour avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating book {BookId}", id);
            return new ApiResponse<BookDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteBookAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var book = await _context.Books
                .Include(b => b.Item)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Livre non trouvé"
                };
            }

            _context.Books.Remove(book);
            _context.Items.Remove(book.Item);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Livre supprimé avec succès"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting book {BookId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la suppression",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PaginatedResult<BookDto>>> GetPaginatedBooksAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        try
        {
            var query = _context.Books
                .Include(b => b.Item)
                    .ThenInclude(i => i.Subcategory)
                        .ThenInclude(s => s!.Category)
                .Include(b => b.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    b.Item.Title.Contains(searchTerm) ||
                    (b.Item.Creator != null && b.Item.Creator.Contains(searchTerm)) ||
                    (b.Isbn != null && b.Isbn.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var books = await query
                .OrderBy(b => b.Item.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => MapToDto(b))
                .ToListAsync();

            var result = new PaginatedResult<BookDto>
            {
                Items = books,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PaginatedResult<BookDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated books");
            return new ApiResponse<PaginatedResult<BookDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération des livres",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            BookId = book.BookId,
            ItemId = book.ItemId,
            Title = book.Item.Title,
            Creator = book.Item.Creator,
            Publisher = book.Item.Publisher,
            Year = book.Item.Year,
            Description = book.Item.Description,
            SubcategoryId = book.Item.SubcategoryId,
            DateAdded = book.Item.DateAdded,
            ImageUrl = book.Item.ImageUrl,
            GenreId = book.GenreId,
            Isbn = book.Isbn,
            GenreName = book.Genre?.Name,
            SubcategoryName = book.Item.Subcategory?.Name,
            CategoryName = book.Item.Subcategory?.Category?.Name
        };
    }
}