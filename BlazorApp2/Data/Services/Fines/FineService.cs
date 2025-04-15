using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
namespace BlazorApp2.Data.Services.Fines
{
    public class FineService 
    {
        private readonly LibDbContext _context;
        private readonly UserSession _userSession;
        public FineService(LibDbContext context, UserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }
        public async Task<bool> CreateFine(Guid userId, Guid borId, decimal money)
        {
            var borr = await _context.Borrowing.FirstOrDefaultAsync(b => b.BorrowId == borId);
            if (borr == null || borr.Status != "Returned")
                return false;
            var fine = new Fine
            { 
                UserId = userId,
                BorrowId=borId,
                Amount = money,
                IssueDate = DateTime.UtcNow
            };
            _context.Fines.Add(fine);
            return true;
        }
        public async Task DealPay(Guid fId)
        {
            var fine = await _context.Fines.FirstOrDefaultAsync(f => f.FineId==fId);
            fine.Status = "Paid";
            await _context.SaveChangesAsync();
            await _userSession.AddLog(action: "Pay", entitytype: "fine");
        }
        public async Task<List<Fine>> GetUserFines(Guid userId)
        {
            return await _context.Fines
                .Where(f=>f.UserId==userId)
                .OrderByDescending(f=>f.IssueDate)
                .ToListAsync(); 
        }
    }
}
