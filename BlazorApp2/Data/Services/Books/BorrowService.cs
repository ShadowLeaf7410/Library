using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
using BlazorApp2.Data.Services.Fines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorApp2.Data.Services.Books
{ 
    public class BorrowService 
    {
        private readonly LibDbContext _context;
        private readonly BookService _bookService;
        private readonly ReservationService _reservationService;
        private readonly FineService _fineService;
        private readonly UserSession _userSession;
        public BorrowService(LibDbContext context,BookService bookService, 
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
            var book = await _bookService.GetBookById(bookId);
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
            book.UpdatedAt = DateTime.UtcNow;
            _context.Borrowing.Add(borrowing);
            await _reservationService.FreshEx(bId: book.BookId);
            await _userSession.AddLog(action: "borrow", entitytype: "book,reservation");
            return true;
        }
        // 归还图书
        public async Task<bool> ReturnBook(Guid userId,Guid borrowId)
        {
            var borrowing = await _context.Borrowing
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

            if (borrowing == null || borrowing.Status != "Borrowed")
                return false;

            borrowing.Status = "Returned";
            borrowing.ReturnDate = DateTime.UtcNow;

            // 更新图书状态
            var book = borrowing.Book;
            book.AvailableCopies++;
            book.UpdatedAt = DateTime.UtcNow;
            await _reservationService.FreshEx(bId:book.BookId);
            await _fineService.CreateFine(userId, borrowId, ColFine(borrowing));
            await _userSession.AddLog(action: "return", entitytype: "book,reservation,fine");
            return true;
        }
        public async Task<bool> ReportLost(Guid userId,Guid borrowId)
        {
            var borrowing = await _context.Borrowing
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == borrowId);
            if (borrowing == null)
                return false;
            var book = borrowing.Book;
            book.TotalCopies--;
            book.UpdatedAt = DateTime.UtcNow;
            await _fineService.CreateFine(userId, borrowId, ColFine(borrowing, true));
            await _userSession.AddLog(action: "reportLost", entitytype: "book,fine");
            return true;
        }
        public async Task<Borrowing> GetBorrowingById(Guid borrowid)
        {
            return await _context.Borrowing.FindAsync(borrowid);
        }
        public async Task<List<Borrowing>> GetUserBorrwing(Guid userId)
        {
            await FrsehBor(userId);
            return await _context.Borrowing
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();
        }
        public async Task FrsehBor(Guid userId)
        {
            var borr= await _context.Borrowing
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
        }
        public decimal ColFine(Borrowing bor,bool isLost=false)
       {
            if(isLost)
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
