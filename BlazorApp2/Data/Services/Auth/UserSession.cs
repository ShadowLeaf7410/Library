using BlazorApp2.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp2.Data.Services.Auth
{
    public class UserSession : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbContextFactory<LibDbContext> _context;
        public UserSession(IHttpContextAccessor httpContextAccessor,IDbContextFactory<LibDbContext> context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal=_httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(principal);
        }
        private async Task LoginWeb(User user)
        {
            var claims=new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Email),
                new Claim(ClaimTypes.Role,user.Role)
            };
            var identity= new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            var principal=new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties
                    );
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
        public async Task<string> VailEP(string email,string pass)
        {
            await using var context = _context.CreateDbContext();
            var taruser = await context.Users.FirstOrDefaultAsync(u => u.Email == email&&u.Status!="Deleted");
            if (taruser == null) return "email";
            if (!Veripassword(pass,taruser.HashedPassword)) return "password";
            return "null";
        }
        private async Task GenerateLoginToken(User user)
        {
            await using var context = _context.CreateDbContext();
            var token = new LoginToken
            {
                UserId = user.UserId,
                Token = Guid.NewGuid().ToString(),
                Expiration = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };
            context.LoginTokens.Add(token);
            await context.SaveChangesAsync();
        }
        public async Task Login(string email)
        {
            await using var context = _context.CreateDbContext();
            var user = await context.Users
                .Include(u=>u.LoginTokens)
                .FirstOrDefaultAsync(u => u.Email == email && u.Status != "Deleted");
            if (user == null)
                throw new InvalidOperationException("User not found");
            var loginToken = user.LoginTokens.FirstOrDefault(t=>!t.IsUsed&&t.Expiration>DateTime.UtcNow);
            if (loginToken == null)
            { 
                await GenerateLoginToken(user);
                await Login(email);
                return;
            }
            loginToken.IsUsed = true;
            loginToken.User.LastLogin= DateTime.UtcNow;
            user.Status = "Active";
            await context.SaveChangesAsync();
            await LoginWeb(user);
            await AddLog(action:"Login",entitytype:"User,LoginToken");
            await context.SaveChangesAsync();
        }
        private async Task LogoffWeb()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }
        public async Task Logoff()
        {
            await using var context = _context.CreateDbContext();
            var authState=await GetAuthenticationStateAsync();
            var user=authState.User;
            await AddLog(action: "LogOff", entitytype: "User");
            if (user.Identity.IsAuthenticated)
            {
                var username=user.Identity.Name;
                var taruser = await context.Users.FirstOrDefaultAsync(u => u.Email == username && u.Status != "Deleted");
                taruser.Status = "Suspended";
                await context.SaveChangesAsync();
            }
            await LogoffWeb();
        }
        public async Task Logout()
        {
            await using var context = _context.CreateDbContext();
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity.IsAuthenticated)
            {
                var username = user.Identity.Name;
                var taruser = await context.Users.FirstOrDefaultAsync(u => u.Email == username && u.Status != "Deleted");
                taruser.Status = "Deleted";
                await context.SaveChangesAsync();
            }
            await LogoffWeb();
        }
        public async Task<User> GetCurrentUser()
        {
            await using var context = _context.CreateDbContext();
            var authState=await GetAuthenticationStateAsync();
            var user=authState.User;
            if(user.Identity.IsAuthenticated)
            {
                var username=user.Identity.Name;
                return await context.Users.FirstOrDefaultAsync(u => u.Email == username && u.Status != "Deleted");
            }
            return null;
        }
        public async Task<string> getRole()
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;
            if(user.Identity.IsAuthenticated)
            {
                return user.FindFirst(ClaimTypes.Role)?.Value;
            }
            return null;
        }
        public async Task<bool> IsInRole(string role)
        {
            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;
            return user.Identity.IsAuthenticated && user.IsInRole(role);
        }
        public async Task<bool> IsUser()=>await IsInRole("User");
        public async Task<bool> IsLibrarian() => await IsInRole("Librarian");
        public async Task<bool> IsAdmin() => await IsInRole("Admin");
        public async Task<bool> IsAuthed()
        {
            var authState = await GetAuthenticationStateAsync();
            return authState.User.Identity.IsAuthenticated;
        }
        public async Task AddUser(string email,string password)
        {
            await using var context = _context.CreateDbContext();
            var user = new User()
            {
                UserId = Guid.NewGuid(),
                Email = email,
                Name="匿名用户",
                HashedPassword=Pltohpassword(password)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            await AddLog(action: "AddUser/SignUp", entitytype: "User");
            await context.SaveChangesAsync();
        }
        public async Task UpdateUser(string email,string name,string Hpassword)
        {
            await using var context = _context.CreateDbContext();
            bool changes = false;
            var user = await GetCurrentUser();
            if (user == null)
                return;
            if(user.Email!=email)
            {
                user.Email = email;
                changes = true;
            }
            if (user.Name != name)
            { 
                user.Name = name;
                changes = true;
            }
            if(user.HashedPassword!=Hpassword)
            {
                user.HashedPassword = Hpassword;
                changes = true;
            }
            if(changes)
            {
                await context.SaveChangesAsync();
                await AddLog(action: "UpdateUser", entitytype: "User");
                await context.SaveChangesAsync();
            }
            else
                return;
        }
        public async Task UpdateUsers(User changeduser)
        {
            await using var context = _context.CreateDbContext();
            bool changes=false;
            var user=await context.Users.FirstOrDefaultAsync(u=>u.UserId==changeduser.UserId);
            if (user == null)
                return;
            if(user.Email!=changeduser.Email)
            {
                user.Email = changeduser.Email;
                changes=true;
            }
            if(user.Name!=changeduser.Name)
            {
                user.Email = changeduser.Email;
                changes=true;
            }
            if(user.HashedPassword!=changeduser.HashedPassword)
            {
                user.HashedPassword = changeduser.HashedPassword;
                changes=true;
            }
            if(user.Role!=changeduser.Role)
            {
                user.Role = changeduser.Role;
                changes=true;
            }
            if (changes)
            {
                await context.SaveChangesAsync();
                await AddLog(action: "UpdateUser", entitytype: "User");
                await context.SaveChangesAsync();
            }
            else
                return;
        }
        public async Task FindLostPass(string email,string newpass)
        {
            await using var context = _context.CreateDbContext();
            var user=await context.Users.FirstOrDefaultAsync(u=>u.Email==email);
            user.HashedPassword = Pltohpassword(newpass);
            await context.SaveChangesAsync();
        }
        public async Task SetLibrarian(Guid userId)
        {
            await using var context = _context.CreateDbContext();
            var user=await context.Users.FirstOrDefaultAsync(u=>u.UserId == userId);
            if(user == null) return;
            user.Role = "Librarian";
            await context.SaveChangesAsync();
            await AddLog(action: "SetLibrarian", entitytype: "User");
            await context.SaveChangesAsync();
        }
        public async Task SetUser(Guid userId)
        {
            await using var context = _context.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return;
            user.Role = "User";
            await context.SaveChangesAsync();
            await AddLog(action: "SetUser", entitytype: "User");
            await context.SaveChangesAsync();
        }
        public async Task<List<User>> SearchUsers(string keyword=null,string name=null,string email=null,string status=null,string role=null)
        {
            await using var context = _context.CreateDbContext();
            var query=context.Users.AsQueryable();
            if(!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u =>
                u.Name.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword)
                );
            }
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => u.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(u => u.Status == status);
            if(!string.IsNullOrWhiteSpace(role))
                query=query.Where(u => u.Role == role);

            return await query.ToListAsync();
        }
        public async Task AddLog(string action = null, string entitytype = null, string entityid = null,
            string details = null, string ip = null)
        {
            await using var context = _context.CreateDbContext();
            var user = await GetCurrentUser();
            if (user == null)
            {
                user=await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@library.edu");
            }
            ip = GetIp();
            var log = new SystemLog
            {
                LogId = Guid.NewGuid(),
                UserId = user.UserId,
                Action = action,
                EntityType = entitytype,
                EntityId = entityid,
                Details = details,
                IpAddress = ip
            };
            context.SystemLogs.Add(log);
            await context.SaveChangesAsync();
        }

        public string GetIp()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }
        public static string Pltohpassword(string plpassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plpassword);
        }
        public static bool Veripassword(string plpassword,string hpassword)
        {
            return BCrypt.Net.BCrypt.Verify(plpassword, hpassword);
        }
    }
}
