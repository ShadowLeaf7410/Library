using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
using BlazorApp2.Pages.Reservations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BlazorApp2.Data.Services.Books
{


    public class ReservationService 
    {
        private readonly LibDbContext _context;
        private readonly BookService _bookService;
        private readonly UserSession _userSession;
        public ReservationService(LibDbContext context,BookService bookService,UserSession userSession)
        {
            _context = context;
            _bookService = bookService;
            _userSession = userSession;
        }
        // 创建预约
        public async Task<Reservation> CreateReservation(Guid userId, Guid bookId, int expiryDays = 3)
        {
            var book = await _bookService.GetBookById(bookId);
            if (book == null)
                return null;

            var reservation = new Reservation
            {
                UserId = userId,
                BookId = bookId,
                ReservationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(expiryDays),
                Status = "Pending"
            };

            book.UpdatedAt = DateTime.UtcNow;

            _context.Reservations.Add(reservation);
            await Fresh(reservation.BookId);
            await _userSession.AddLog(action:"Reserve",entitytype:"book,reservation");
            return reservation;
        }
        // 取消预约
        public async Task<bool> CancelReservation(Guid reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null)
                return false;

            reservation.Status = "Cancelled";
            await Fresh(reservation.BookId);
            await _userSession.AddLog(action: "CancelReserve", entitytype: "book,reservation");
            return true;
        }
        public async Task<bool> ExistVail(Guid userId,Guid bookId)
        {
            return await _context.Reservations
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId && (r.Status == "Pending" || r.Status == "Fulfilled"));
        }
        // 获取用户预约
        public async Task<List<Reservation>> GetUserReservations(Guid userId)
        {
            await FreshEx(userId:userId);
            return await _context.Reservations
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }
        public async Task<Reservation> GetReservationByRId(Guid rId)
        {
            return await _context.Reservations
                .OrderByDescending(r => r.ReservationDate)
                .FirstOrDefaultAsync(r => r.ReservationId == rId);
        }
        public async Task FreshEx(Guid userId=default,Guid bId=default)
        {
            await _context.SaveChangesAsync();
            DateTime dateTime = DateTime.Now;
            if (userId != default)
            {
                var rese = await _context.Reservations
                    .Where(r => r.UserId == userId && (r.Status == "Pending" || r.Status == "Fulfilled"))
                    .ToListAsync();
                foreach (var reservation in rese)
                {
                    if (reservation.ExpiryDate < dateTime)
                    {
                        reservation.Status = "Expired";
                        await Fresh(reservation.BookId);
                    }
                }
            }
            if(bId!=default)
            {
                var rese=await _context.Reservations
                    .Where(r=>r.BookId==bId&& (r.Status == "Pending" || r.Status == "Fulfilled"))
                    .ToListAsync();
                foreach (var reservation in rese)
                {
                    if (reservation.ExpiryDate < dateTime)
                    {
                        reservation.Status = "Expired";
                    }
                }
                await Fresh(bId);
            }
        }
        public async Task Fresh(Guid bId)
        {
            await _context.SaveChangesAsync();
            var book=await _bookService.GetBookById(bId);
            var rese = await _context.Reservations
                .Where(r => r.BookId == bId&&(r.Status=="Pending"||r.Status=="Fulfilled"))
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();
            if(book.AvailableCopies>rese.Count)
            {
                book.Status = "Available";
                foreach(var res in rese)
                {
                    res.Status = "Fulfilled";
                }
            }else if(book.AvailableCopies<rese.Count)
            {
                book.Status = "Reserved";
                for(int i = 0;i<book.AvailableCopies;i++)
                {
                    rese[i].Status = "Fulfilled";
                }

            }else if(book.AvailableCopies==rese.Count)
            {
                if(rese.Count>0)
                {
                    book.Status="Reserved";
                    foreach (var res in rese)
                    {
                        res.Status = "Fulfilled";
                    }
                }
                else
                {
                    book.Status = "CheckOut";
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
