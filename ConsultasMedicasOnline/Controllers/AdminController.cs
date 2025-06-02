using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ConsultasMedicasOnline.Controllers
{
    // [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeedDemoData()
        {
            await ApplicationDbContext.SeedData(_context, _userManager, _roleManager);
            TempData["Message"] = "Dados de demonstração foram adicionados com sucesso!";
            return RedirectToAction("Index");
        }
    }
}
