using Microsoft.AspNetCore.Mvc;

namespace ConsultasMedicasOnline.Controllers
{
    public class PaginasController : Controller
    {
        // Sobre Nós
        public IActionResult SobreNos()
        {
            ViewData["Title"] = "Sobre Nós - MedConsulta";
            return View();
        }

        // Como Funciona
        public IActionResult ComoFunciona()
        {
            ViewData["Title"] = "Como Funciona - MedConsulta";
            return View();
        }

        // Especialidades
        public IActionResult Especialidades()
        {
            ViewData["Title"] = "Especialidades - MedConsulta";
            return View();
        }

        // Contato
        public IActionResult Contato()
        {
            ViewData["Title"] = "Contato - MedConsulta";
            return View();
        }

        // Central de Ajuda
        public IActionResult CentralAjuda()
        {
            ViewData["Title"] = "Central de Ajuda - MedConsulta";
            return View();
        }

        // Política de Privacidade
        public IActionResult PoliticaPrivacidade()
        {
            ViewData["Title"] = "Política de Privacidade - MedConsulta";
            return View();
        }

        // Termos de Uso
        public IActionResult TermosUso()
        {
            ViewData["Title"] = "Termos de Uso - MedConsulta";
            return View();
        }

        // FAQ
        public IActionResult FAQ()
        {
            ViewData["Title"] = "Perguntas Frequentes - MedConsulta";
            return View();
        }
    }
}
