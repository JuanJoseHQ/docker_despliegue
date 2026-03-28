using GestionUsuariosAPI.Data;
using GestionUsuariosAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuariosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Iniciar sesión con nombre de usuario y contraseña
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(LoginResponseDto), 401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario);

            if (usuario == null || !BCryptSimple.VerifyPassword(dto.Password, usuario.PasswordHash))
            {
                return Unauthorized(new LoginResponseDto
                {
                    Exitoso = false,
                    Mensaje = "Credenciales inválidas"
                });
            }

            if (!usuario.Activo)
            {
                return Unauthorized(new LoginResponseDto
                {
                    Exitoso = false,
                    Mensaje = "La cuenta está desactivada. Contacte al administrador."
                });
            }

            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                Exitoso = true,
                Mensaje = $"Bienvenido, {usuario.NombreCompleto}",
                Usuario = new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    NombreUsuario = usuario.NombreUsuario,
                    Telefono = usuario.Telefono,
                    Activo = usuario.Activo,
                    FechaRegistro = usuario.FechaRegistro,
                    UltimoAcceso = usuario.UltimoAcceso,
                    Rol = usuario.Rol?.Nombre ?? "Sin rol"
                }
            });
        }

        /// <summary>
        /// Cambiar contraseña de un usuario
        /// </summary>
        [HttpPost("{id}/cambiar-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(ApiResponse<object>.Error("Usuario no encontrado"));

            if (!BCryptSimple.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
                return BadRequest(ApiResponse<object>.Error("La contraseña actual es incorrecta"));

            usuario.PasswordHash = BCryptSimple.HashPassword(dto.NuevaPassword);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { }, "Contraseña actualizada exitosamente"));
        }
    }
}
