using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
using BlazorApp2.Data.Services.Fines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorApp2.Data.Services.Books
{ 
    public class BorrowService 
    {
        private readonly IDbContextFactory<LibDbContext> _context;
        private readonly BookService _bookService;
        private readonly ReservationService _reservationService;
        private readonly FineService _fineService;
        private readonly UserSession _userSession;
        public BorrowService(IDbContextFactory<LibDbContext> context,BookService bookService, 
            ReservationService reservationService,FineService fineService,UserSession userSession)
        {
            _context = context;
            _bookService = bookService;
            _reservationService = reservationService;
            _fineService = fineService;
            _userSession = userSession;
        }
        // 借阅图书
        public async Task<bool> BorrowBook(Guid userId, Guid bookId, int days = 14,decimal fine=0.5m)
        {
            await using var context = _context.CreateDbContext();
            var book = await context.Books.FindAsync(bookId);
            if (book == null || book.Status!="Available")
                return false;

            var borrowing = new Borrowing
            {
                BorrowId=Guid.NewGuid(),
                UserId = userId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(days),
                Status = "Borrowed",
                FineAmount=fine
            };

            // 更新图书状态和可用数量
            book.AvailableCopies--;
            if (book.AvailableCopies == 0)
            {
                book.Status = "CheckedOut";
            }

            var rese = await context.Reservations.FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId && (r.Status == "Pending" || r.Status == "Fulfilled"));
            if (rese != null)
            {
                rese.Status = "Expired";
            }
            book.UpdatedAt = DateTime.UtcNow;
            context.Borrowing.Add(borrowing);
            await context.SaveChangesAsync();
            await _reservationService.FreshEx(bId: book.BookId);
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "borrow", entitytype: "book,reservation");
            await context.SaveChangesAsync();
            return true;
        }
        // 归还图书
        public async Task<bool> ReturnBook(Guid userId,Guid borrowId)
        {
            await using var context = _context.CreateDbContext();
            var borrowing = await context.Borrowing
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrowing == null || borrowing.Status=="Returned")
                return false;

            borrowing.Status = "Returned";
            borrowing.ReturnDate = DateTime.UtcNow;

            // 更新图书状态
            var book = borrowing.Book;
            book.AvailableCopies++;
            book.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await _reservationService.FreshEx(bId:book.BookId);
            await context.SaveChangesAsync();
            if (ColFine(borrowing)!=0)
            {
                await _fineService.CreateFine(userId, borrowId, ColFine(borrowing));
                await context.SaveChangesAsync();
            }
            await _userSession.AddLog(action: "return", entitytype: "book,reservation,fine");
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ReportLost(Guid userId,Guid borrowId)
        {
            await using var context = _context.CreateDbContext();
            var borrowing = await context.Borrowing
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);
            if (borrowing == null)
                return false;
            var book = borrowing.Book;
            book.TotalCopies--;
            book.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await _fineService.CreateFine(userId, borrowId, ColFine(borrowing, true));
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "reportLost", entitytype: "book,fine");
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<Borrowing> GetBorrowingById(Guid borrowid)
        {
            await using var context = _context.CreateDbContext();
            return await context.Borrowing.FindAsync(borrowid);
        }
        public async Task<List<Borrowing>> GetUserBorrwing(Guid userId)
        {
            await using var context = _context.CreateDbContext();
            await FrsehBor(userId);
            await context.SaveChangesAsync();
            return await context.Borrowing
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();
        }
        public async Task FrsehBor(Guid userId)
        {
            await using var context = _context.CreateDbContext();
            var borr= await context.Borrowing
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();
            foreach(var bor in borr)
            {
                if(bor.Status=="Borrowed"&&bor.DueDate<DateTime.Now)
                {
                    bor.Status = "Overdue";
                }
            }
            await context.SaveChangesAsync();
        }
        public decimal ColFine(Borrowing bor,bool isLost=false)
       {
            if(!isLost)
            {
                if (bor.ReturnDate == null)
                {
                    if (bor.DueDate < DateTime.Now)
                    {
                        TimeSpan difference = DateTime.Now - bor.DueDate;
                        int days = difference.Days + 1;
                        return bor.FineAmount * days;
                    }
                    return default;
                }
                else
                {
                    if (bor.DueDate < bor.ReturnDate)
                    {
                        TimeSpan difference = (TimeSpan)(bor.ReturnDate - bor.DueDate);
                        int days = difference.Days + 1;
                        return bor.FineAmount * days;
                    }
                    return default;
                }
            }
            else
            {
                return bor.FineAmount * 100;
            }
            
       }
    }
}
