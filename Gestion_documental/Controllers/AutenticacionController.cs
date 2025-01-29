
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Gestion_documental.Data;
using Gestion_documental.Models;


namespace Gestion_documental.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly Gestion_documentalContext _context;

        public AutenticacionController(Gestion_documentalContext context)
        {
            _context = context;
        }

        // Vista de Iniciar Sesión
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string contraseña)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !VerificarContraseña(contraseña, usuario.Contraseña))
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View();
            }

            // Aquí deberías implementar autenticación con HttpContext.SignInAsync
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

            return RedirectToAction("Index", "Home");
        }

        // Vista de Registro
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                
                    // Verificar si el correo ya existe
                    if (await _context.Usuario.AnyAsync(u => u.Email == usuario.Email))
                    {
                        TempData["ErrorMessage"] = "El correo ya está registrado";
                        return View(usuario);
                    }

                    // Hashear contraseña
                    usuario.Contraseña = HashearContraseña(usuario.Contraseña);

                    _context.Usuario.Add(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario registrado exitosamente";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    // Log the exception details here
                    TempData["ErrorMessage"] = "Error al registrar usuario: " + ex.Message;
                    return View(usuario);
                }
                // Si llegamos aquí, hay errores en el ModelState
                //foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    TempData["ErrorMessage"] = modelError.ErrorMessage;
                //}

            }
            return View(usuario);
        }

        // Vista de Recuperar Contraseña
        public IActionResult RecuperarContraseña()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContraseña(string email)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Verifique el correo electrónico.";
                return View();
            }

            return RedirectToAction(nameof(CambiarContraseña), new { email });
        }

        // Vista de Cambiar Contraseña
        public IActionResult CambiarContraseña(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContraseña(string email, string nuevaContraseña)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Error al cambiar la contraseña. Usuario no encontrado.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                usuario.Contraseña = HashearContraseña(nuevaContraseña);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
                return RedirectToAction(nameof(Login));
            }
            catch
            {
                TempData["ErrorMessage"] = "Error al cambiar la contraseña. Intente nuevamente.";
                return RedirectToAction(nameof(Login));
            }
        }


        // Cerrar Sesión
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // Métodos de Utilidad
        private string HashearContraseña(string contraseña)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerificarContraseña(string contraseñaIngresada, string contraseñaAlmacenada)
        {
            return HashearContraseña(contraseñaIngresada) == contraseñaAlmacenada;
        }
    }
}
