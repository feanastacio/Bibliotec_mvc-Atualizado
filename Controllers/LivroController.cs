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
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }
        Context context = new Context();
        public IActionResult Index()
        {
            ViewBag.Ad = HttpContext.Session.GetString("Admin")!;

            // Criar e nomear uma lista de livros
            List<Livro> listaLivros = context.Livro.ToList();

            // Tem que fazer a verificação o livro para saber se tem reserva ou não 
            var livroReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivroComReserva = livroReservados;

            return View();
        }

        [Route("Cadastro")]

        // Método que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {
            ViewBag.Ad = HttpContext.Session.GetString("Admin")!;
            ViewBag.Categorias = context.Categoria.ToList();
            return View();
        }

        // Mtodo para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            Livro novoLivro = new Livro();

            // O que meu usúario escrever no formulário será atribuido ao novoLivro
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();
            // img
            context.Livro.Add(novoLivro);
            context.SaveChanges();

            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();

            string[] categoriasSelecionadas = form["Categoria"].ToString().Split("'");
            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                listaLivroCategorias.Add(livroCategoria);
            }

            context.LivroCategoria.AddRange();

            context.SaveChanges();
            return LocalRedirect("/Cadastro");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // { 
        //     return View("Error!");
        // }
    }
}