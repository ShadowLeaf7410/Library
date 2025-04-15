using Microsoft.AspNetCore.Mvc;
using BlazorApp2.Data.Services.Auth;
using BlazorApp2.Data.Models;
using Microsoft.JSInterop;
namespace BlazorApp2.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserSession _userSession;
        public AccountController(UserSession userSession)
        {
            _userSession = userSession;
        }
        [HttpGet("login-handler")]
        public async Task<IActionResult> LoginHandler(string email)
        {
            await _userSession.Login(email);
            return Redirect("/");
        }
        [HttpGet("logoff")]
        public async Task<IActionResult> Logoff()
        {
            await _userSession.Logoff();
            return Redirect("/");
        }
    }
}
