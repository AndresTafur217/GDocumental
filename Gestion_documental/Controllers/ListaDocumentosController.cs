using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gestion_documental.Data;
using Gestion_documental.Models;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using X.PagedList;

namespace Gestion_documental.Controllers;

public class ListaDocumentosController : Controller
{
    private readonly Gestion_documentalContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ListaDocumentosController(Gestion_documentalContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Documentoes
    public async Task<IActionResult> Index(string nombre, int? categoriaId, DateTime? fecha, int? page)
    {
        // Obtener la lista de categorías para el dropdown
        ViewBag.Categorias = new SelectList(_context.Categoria, "Id", "Nombre");

        // Guardar los valores de búsqueda en ViewBag para mantenerlos en el formulario
        ViewBag.Nombre = nombre;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");

        // Consulta base
        var query = _context.Documento
            .Include(d => d.Categoria)
            .Include(d => d.Usuario)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(nombre))
        {
            query = query.Where(d => d.Nombre.Contains(nombre));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(d => d.CategoriaId == categoriaId.Value);
        }

        if (fecha.HasValue)
        {
            query = query.Where(d => d.FechaCreacion.Date == fecha.Value.Date);
        }

        // Paginación
        int pageSize = 8; // Número de elementos por página
        int pageNumber = (page ?? 1); // Si page es null, se establece en 1

        // Obtener el total de registros
        int totalRecords = await query.CountAsync();

        // Obtener los registros paginados
        var documentos = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        // Crear la lista paginada
        var pagedList = new StaticPagedList<Documento>(documentos, pageNumber, pageSize, totalRecords);

        // Pasar la lista paginada a la vista
        return View(pagedList);
    }

    // GET: Documentoes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var documento = await _context.Documento
            .Include(d => d.Categoria)
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (documento == null)
        {
            return NotFound();
        }

        return View(documento);
    }

    public IActionResult Create()
    {
        ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre");
        ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,UsuarioId,CategoriaId,Ruta")] Documento documento, IFormFile archivo)
    {
        try
        {
            var nuevoDocumento = new Documento
            {
                Nombre = documento.Nombre,
                UsuarioId = documento.UsuarioId,
                CategoriaId = documento.CategoriaId,
                Ruta = documento.Ruta, // Ubicación física en el cuarto de archivos
                FechaCreacion = DateTime.Now
            };

            if (archivo != null && archivo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Archivos_subidos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(fileStream);
                }

                nuevoDocumento.Copia = "/Archivos_subidos/" + uniqueFileName; // Ruta de la copia digital
            }

            _context.Documento.Add(nuevoDocumento);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al crear el documento: {ex.Message}");
        }

        ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nombre", documento.CategoriaId);
        ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Nombre", documento.UsuarioId);
        return View(documento);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var documento = await _context.Documento.FindAsync(id);
        if (documento == null)
        {
            return NotFound();
        }
        ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre", documento.CategoriaId);
        ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre", documento.UsuarioId);
        return View(documento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,UsuarioId,CategoriaId,Ruta,Copia,FechaCreacion")] Documento documento, IFormFile archivo)
    {
        if (id != documento.Id)
        {
            return NotFound();
        }

        try
        {
            var documentoExistente = await _context.Documento.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (documentoExistente == null)
            {
                return NotFound();
            }

            documento.FechaCreacion = documentoExistente.FechaCreacion;

            if (archivo != null && archivo.Length > 0)
            {
                // Eliminar archivo anterior si existe
                if (!string.IsNullOrEmpty(documentoExistente.Copia))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, documentoExistente.Copia.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Guardar nuevo archivo
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Archivos_subidos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + archivo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(fileStream);
                }

                documento.Copia = "/Archivos_subidos/" + uniqueFileName;
            }
            else
            {
                documento.Copia = documentoExistente.Copia;
            }

            _context.Update(documento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (ex is DbUpdateConcurrencyException && !DocumentoExists(documento.Id))
            {
                return NotFound();
            }

            ModelState.AddModelError("", $"Error al actualizar el documento: {ex.Message}");
        }

        ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nombre", documento.CategoriaId);
        ViewData["UsuarioId"] = new SelectList(_context.Set<Usuario>(), "Id", "Nombre", documento.UsuarioId);
        return View(documento);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var documento = await _context.Documento.FindAsync(id);
        if (documento == null || string.IsNullOrEmpty(documento.Copia))
        {
            return NotFound();
        }

        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documento.Copia.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out string contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileName = Path.GetFileName(filePath);
        return PhysicalFile(filePath, contentType, fileName);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var documento = await _context.Documento
            .Include(d => d.Categoria)
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (documento == null)
        {
            return NotFound();
        }

        return View(documento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var documento = await _context.Documento.FindAsync(id);
        if (documento != null)
        {
            // Eliminar el archivo físico si existe
            if (!string.IsNullOrEmpty(documento.Copia))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documento.Copia.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Documento.Remove(documento);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Preview(int id)
    {
        var documento = await _context.Documento.FindAsync(id);
        if (documento == null || string.IsNullOrEmpty(documento.Copia))
        {
            return NotFound();
        }

        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, documento.Copia.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // Obtener el tipo MIME del archivo
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        // Leer el contenido del archivo
        var fileStream = System.IO.File.OpenRead(filePath);
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // Pasar el contenido del archivo a la vista
        ViewBag.FileContent = Convert.ToBase64String(memoryStream.ToArray());
        ViewBag.ContentType = contentType;
        ViewBag.FileName = Path.GetFileName(filePath);

        return View(documento);
    }

    private bool DocumentoExists(int id)
    {
        return _context.Documento.Any(e => e.Id == id);
    }
}

