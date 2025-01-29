using G_Archivos.Data;
using G_Archivos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public abstract class BaseController : Controller
{
    protected readonly GArchivosContext _context;

    public BaseController(GArchivosContext context)
    {
        _context = context;
    }

    public async Task<DocumentoSearchViewModel> InitializeSearchViewModel()
    {
        return new DocumentoSearchViewModel
        {
            Categorias = await _context.Categoria.ToListAsync(),
            Resultados = new List<Documento>()
        };
    }
}