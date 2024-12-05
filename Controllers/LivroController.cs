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
            //  Trabalhando com img

            if(form.Files.Count > 0) {
                // Primeiro passo:
                    // Armazenaremos o arquivo/foto enviado pelo usúario
                    var arquivo = form.Files[0];

                // Segundo passo:
                    // Criar variavel do caminho  da minha pasta para colocar as fotos dos livros
                    var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                    // Validaremos se a pasta que  será armazenada as imagens, existe. Caso não exista, criaremos uma nova pasta
                    if(!Directory.Exists(pasta)) {
                        // Criar pasta:
                        Directory.CreateDirectory(pasta);
                    }
                // Terceiro passo:
                    // Criar a variavel para armanezar o caminho em que meu arquivo estará além do nome dele
                    var caminho = Path.Combine(pasta, arquivo.FileName);
                    using (var stream = new FileStream(caminho, FileMode.Create)) {
                        // Copiou o arquivo para o meu diretório
                        arquivo.CopyTo(stream);
                    }

                    novoLivro.Imagem = arquivo.FileName;
            }else{
                    novoLivro.Imagem = "padrao.png";
            }
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
            return LocalRedirect("/Livro/Cadastro");
        }

        [Route("Editar/{id}")]
        public IActionResult Editar(int id){
            ViewBag.Ad = HttpContext.Session.GetString("Admin")!;
            ViewBag.CategoriasDoSistema = context.Categoria.ToList();
            // LivroID = 3
            
            // Buscar quemmé otal do livro 3
            Livro livroAtualizado = context.Livro.First(livro => livro.LivroID == id);
            // Buscar as categorias que o livroAtualizado possui
            var categoriasDolivroAtualizado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id).Select(livro => livro.Categoria).ToList();

            ViewBag.Livro = livroAtualizado;
            ViewBag.CAtegoria = categoriasDolivroAtualizado;

            return View();
        }
        [Route("Atualizar/{id}")]
        public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem){
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"];
            livroAtualizado.Escritor = form["Escritor"];
            livroAtualizado.Editora = form["Editora"];
            livroAtualizado.Idioma = form["Idioma"];
            livroAtualizado.Descricao = form["Descricao"];

            // Upload imagem
            if (imagem != null && imagem.Length > 0){
                // Definiremos o caminho da imagem do livro atual, que eu quero atuar
                var caminhoImagem = Path.Combine("wwwroot/imagens/Livros", imagem.FileName);

                // Verificar se o usúariomclocou uma imagem para atualizar o livro
                if (string.IsNullOrEmpty(livroAtualizado.Imagem)){
                    // Caso exista, ela irá ser apagada
                    var caminhoImagemAntiga = Path.Combine("wwwroot/imagens/Livros", livroAtualizado.Imagem);
                    if (System.IO.File.Exists(caminhoImagemAntiga)){
                        System.IO.File.Delete(caminhoImagemAntiga);
                    }
                }
                    // Salvar imagem nova
                    using (var stream = new FileStream(caminhoImagem, FileMode. Create)){
                        imagem.CopyTo(stream);
                    }
                // Subir essa mudança para o meu banco de dados
                livroAtualizado.Imagem = imagem.FileName;
            }

                // Categorias:
                var categoriasSelecionadas = form["Categoria"].ToList();
                var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id);
                foreach (var categoria in categoriasAtuais){
                    if (!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){
                        // Nós vamos resolver a categoria do nosso context
                        context.LivroCategoria.Remove(categoria);
                    }
                    
                }

                foreach(var categoria in categoriasSelecionadas){
                    // Verificando se não(!) existe a categoria nesse livro
                    if (!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria)){
                        context.LivroCategoria.Add(new LivroCategoria {
                            LivroID = id,
                            CategoriaID = int.Parse(categoria)
                        });
                    }
                }

                context.SaveChanges();
                return LocalRedirect("/Livro");
        }

        // Metódo de excluir o livro
        [Route ("Excluir/{id}")]
        public IActionResult Excluir(int id){
            // Buscar qual o livro do id que precisamos excluir
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

            // Buscar as categorias desse livro:
            var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

            foreach(var categoria in categoriasDoLivro){
                context.LivroCategoria.Remove(categoria);
            }

            context.Livro.Remove(livroEncontrado);
            context.SaveChanges();
            return LocalRedirect("/Livro");
        }
        
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // { 
        //     return View("Error!");
        // }
    }
}