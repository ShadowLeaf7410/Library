using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Models;
using BlazorApp2.Data.Services.Auth;
namespace BlazorApp2.Data.Services.Fines
{
    public class FineService 
    {
        private readonly IDbContextFactory<LibDbContext> _context;
        private readonly UserSession _userSession;
        public FineService(IDbContextFactory<LibDbContext> context, UserSession userSession)
        {
            _context = context;
            _userSession = userSession;
        }
        public async Task<bool> CreateFine(Guid userId, Guid borId, decimal money)
        {
            await using var context = _context.CreateDbContext();
            var borr = await context.Borrowing.FirstOrDefaultAsync(b => b.BorrowId == borId);
            if (borr == null || borr.Status != "Returned")
                return false;
            var fine = new Fine
            { 
                UserId = userId,
                BorrowId=borId,
                Amount = money,
                IssueDate = DateTime.UtcNow
            };
            context.Fines.Add(fine);
            await context.SaveChangesAsync();
            return true;
        }
        public async Task DealPay(Guid fId)
        {
            await using var context = _context.CreateDbContext();
            var fine = await context.Fines.FirstOrDefaultAsync(f => f.FineId==fId);
            fine.Status = "Paid";
            await context.SaveChangesAsync();
            await _userSession.AddLog(action: "Pay", entitytype: "fine");
            await context.SaveChangesAsync();
        }
        public async Task<List<Fine>> GetUserFines(Guid userId)
        {
            await using var context = _context.CreateDbContext();
            return await context.Fines
                .Where(f=>f.UserId==userId)
                .OrderByDescending(f=>f.IssueDate)
                .ToListAsync(); 
        }
    }
}
