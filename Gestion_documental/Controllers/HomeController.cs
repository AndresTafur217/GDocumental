using System.Diagnostics;
using Gestion_documental.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gestion_documental.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
//using Gestion_documental.Data;
//using Gestion_documental.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//public class HomeController : BaseController
//{
//    public HomeController(Gestion_documentalContext context) : base(context) { }

//    public async Task<IActionResult> Index()
//    {
//        ViewBag.SearchModel = await InitializeSearchViewModel();
//        return View();
//    }

//    [HttpPost]
//    public async Task<IActionResult> Buscar(DocumentoSearchViewModel searchModel)
//    {
//        var query = _context.Documento
//            .Include(d => d.Usuario)
//            .Include(d => d.Categoria)
//            .AsQueryable();

//        if (!string.IsNullOrEmpty(searchModel.NombreDocumento))
//        {
//            query = query.Where(d => d.Nombre.Contains(searchModel.NombreDocumento));
//        }

//        if (!string.IsNullOrEmpty(searchModel.NombreUsuario))
//        {
//            query = query.Where(d => d.Usuario.Nombre.Contains(searchModel.NombreUsuario));
//        }

//        if (searchModel.CategoriaId.HasValue)
//        {
//            query = query.Where(d => d.CategoriaId == searchModel.CategoriaId);
//        }

//        if (searchModel.FechaInicio.HasValue)
//        {
//            query = query.Where(d => d.FechaCreacion.Date == searchModel.FechaInicio.Value.Date);
//        }

//        searchModel.Resultados = await query.ToListAsync();
//        searchModel.Categorias = await _context.Categoria.ToListAsync();

//        ViewBag.SearchModel = searchModel;
//        return View("Index");
//    }
//}