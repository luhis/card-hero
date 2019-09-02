using System.Threading.Tasks;
using CardHero.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardHero.NetCoreApp.Mvc.Controllers
{
    [Authorize]
    public class ProfileController : CardHeroController
    {
        public ProfileController(IUserService userService)
            : base(userService)
        {
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetUserAsync();

            return View();
        }
    }
}