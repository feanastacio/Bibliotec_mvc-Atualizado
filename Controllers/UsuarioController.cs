using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }
        // Criando obj da classe
        Context context = new Context();

        // O método Index está retornando a View Usuario/Index.cshtml
        public IActionResult Index()
        {
            // Pegar informações da session que são necessárias para que apareça os detalhes do meu usúario
            int Id = int.Parse(HttpContext.Session.GetString("UsuarioID")!);
            ViewBag.Ad = HttpContext.Session.GetString("Admin")!;

            // id = 1
            Usuario usuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == Id)!;

            // Se não for encontrado ninguém
            if (usuarioEncontrado == null) {
                return NotFound();
            }

            // Procurar o curso que meu usuário está cadastrado 
            Curso cursoEncontrado = context.Curso.FirstOrDefault(curso => curso.CursoID == usuarioEncontrado.CursoID)!;

            // Verificar se o usúario possui ou não o curso
            if (cursoEncontrado == null) {
                ViewBag.Curso = "O usúario não possui curso cadastrado";
            } else {
                ViewBag.Curso = cursoEncontrado.Nome;
            }

            ViewBag.Nome = usuarioEncontrado.Nome;
            ViewBag.Email = usuarioEncontrado.Email;
            ViewBag.Contato = usuarioEncontrado.Contato;
            ViewBag.DataNascimento = usuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");

            return View();
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}