using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
using Microsoft.EntityFrameworkCore;
namespace BlazorApp2.Data.Services.Books
{
    public class BookService 
    {
        private readonly IDbContextFactory<LibDbContext> _context;
        private readonly UserSession _userSession;
        //初始化
        public BookService(IDbContextFactory<LibDbContext> context,UserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }
        //添加书籍
        public async Task<Book> AddBook(Book book)
        {
            await using var context = _context.CreateDbContext();
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;

            context.Books.Add(book);
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "AddBook", entitytype: "Book");
            await context.SaveChangesAsync();
            return book;
        }
        //更新书籍
        public async Task<Book> UpdateBook(Book book)
        {
            await using var context = _context.CreateDbContext();
            book.UpdatedAt = DateTime.UtcNow;
            context.Books.Update(book);
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "UpdateBook", entitytype: "Book");
            await context.SaveChangesAsync();
            return book;
        }
        //删除书籍
        public async Task<bool> DeleteBook(Guid bookId)
        {
            await using var context = _context.CreateDbContext();
            var book = await context.Books.FindAsync(bookId);
            if (book == null) return false;

            context.Books.Remove(book);
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "DelBook", entitytype: "Book");
            await context.SaveChangesAsync();
            return true;
        }
        //通过id选取书籍
        public async Task<Book> GetBookById(Guid bookId)
        {
            await using var context = _context.CreateDbContext();
            return await context.Books.FindAsync(bookId);
        }
        public async Task<List<Book>> GetNewBooks()
        {
            await using var context = _context.CreateDbContext();
            return await context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToListAsync();
        }
        public async Task<List<Book>> GetCommands()
        {
            await using var context = _context.CreateDbContext();
            return await context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .ToListAsync();
        }
        //获取所有分类
        public async Task<List<string>> GetAllCategories()
        {
            await using var context = _context.CreateDbContext();
            return await context.Books
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();
        }
        //多条件查询图书
        public async Task<List<Book>> SearchBooks(string keyword=null,string title = null, string author = null,
            string category = null, string status = null, string isbn = null,
            DateTime start=default,DateTime end=default)
        {
            await using var context = _context.CreateDbContext();
            var query = context.Books.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(keyword) ||
                    b.Author.ToLower().Contains(keyword) ||
                    b.Category.ToLower().Contains(keyword) ||
                    b.ISBN.ToLower().Contains(keyword));
            }
            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(b => b.Author.Contains(author));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(b => b.Category == category);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(isbn))
                query = query.Where(b => b.ISBN == isbn);
            if(start != default)
                query = query.Where(b =>b.PublicationYear != null&& b.PublicationYear >= start.Year);
            if (end != default)
                query=query.Where(b=> b.PublicationYear != null&&b.PublicationYear <= end.Year);

            return await query.ToListAsync();
        }
        //获取所有状态选项
        public List<string> GetAllStatusOptions()
        {
            return new List<string>
            {
                "Available", "CheckedOut", "Reserved", "Lost", "Maintenance"
            };
        }
    }
}
