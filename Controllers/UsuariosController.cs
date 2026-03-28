using GestionUsuariosAPI.Data;
using GestionUsuariosAPI.DTOs;
using GestionUsuariosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuariosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todos los usuarios con filtros opcionales
        /// </summary>
        /// <param name="rolId">Filtrar por rol</param>
        /// <param name="activo">Filtrar por estado activo/inactivo</param>
        /// <param name="busqueda">Buscar por nombre o email</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UsuarioResponseDto>>), 200)]
        public async Task<IActionResult> ObtenerTodos(
            [FromQuery] int? rolId,
            [FromQuery] bool? activo,
            [FromQuery] string? busqueda)
        {
            var query = _context.Usuarios.Include(u => u.Rol).AsQueryable();

            if (rolId.HasValue)
                query = query.Where(u => u.RolId == rolId.Value);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo.Value);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(u =>
                    u.NombreCompleto.Contains(busqueda) ||
                    u.Email.Contains(busqueda) ||
                    u.NombreUsuario.Contains(busqueda));

            var usuarios = await query
                .OrderBy(u => u.NombreCompleto)
                .Select(u => MapToDto(u))
                .ToListAsync();

            return Ok(ApiResponse<List<UsuarioResponseDto>>.Ok(usuarios,
                $"Se encontraron {usuarios.Count} usuario(s)"));
        }

        /// <summary>
        /// Obtener un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound(ApiResponse<object>.Error("Usuario no encontrado"));

            return Ok(ApiResponse<UsuarioResponseDto>.Ok(MapToDto(usuario)));
        }

        /// <summary>
        /// Registrar un nuevo usuario
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto dto)
        {
            // Validar email único
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(ApiResponse<object>.Error("El email ya está registrado"));

            // Validar nombre de usuario único
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
                return BadRequest(ApiResponse<object>.Error("El nombre de usuario ya existe"));

            // Validar que el rol exista
            var rol = await _context.Roles.FindAsync(dto.RolId);
            if (rol == null || !rol.Activo)
                return BadRequest(ApiResponse<object>.Error("El rol especificado no existe o está inactivo"));

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                NombreUsuario = dto.NombreUsuario,
                PasswordHash = BCryptSimple.HashPassword(dto.Password),
                Telefono = dto.Telefono,
                RolId = dto.RolId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Recargar con el Rol incluido
            await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id },
                ApiResponse<UsuarioResponseDto>.Ok(MapToDto(usuario), "Usuario registrado exitosamente"));
        }

        /// <summary>
        /// Actualizar perfil de usuario
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return NotFound(ApiResponse<object>.Error("Usuario no encontrado"));

            // Validar email único si se cambia
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != usuario.Email)
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest(ApiResponse<object>.Error("El email ya está en uso por otro usuario"));
                usuario.Email = dto.Email;
            }

            // Validar rol si se cambia
            if (dto.RolId.HasValue && dto.RolId.Value != usuario.RolId)
            {
                var rol = await _context.Roles.FindAsync(dto.RolId.Value);
                if (rol == null || !rol.Activo)
                    return BadRequest(ApiResponse<object>.Error("El rol especificado no existe o está inactivo"));
                usuario.RolId = dto.RolId.Value;
            }

            if (!string.IsNullOrEmpty(dto.NombreCompleto))
                usuario.NombreCompleto = dto.NombreCompleto;

            if (dto.Telefono != null)
                usuario.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();

            return Ok(ApiResponse<UsuarioResponseDto>.Ok(MapToDto(usuario), "Usuario actualizado exitosamente"));
        }

        /// <summary>
        /// Desactivar usuario (borrado lógico)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(ApiResponse<object>.Error("Usuario no encontrado"));

            if (!usuario.Activo)
                return BadRequest(ApiResponse<object>.Error("El usuario ya está desactivado"));

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { }, "Usuario desactivado exitosamente"));
        }

        /// <summary>
        /// Reactivar un usuario desactivado
        /// </summary>
        [HttpPatch("{id}/reactivar")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(ApiResponse<object>.Error("Usuario no encontrado"));

            if (usuario.Activo)
                return BadRequest(ApiResponse<object>.Error("El usuario ya está activo"));

            usuario.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { }, "Usuario reactivado exitosamente"));
        }

        /// <summary>
        /// Obtener estadísticas de usuarios
        /// </summary>
        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var stats = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                UsuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo),
                UsuariosInactivos = await _context.Usuarios.CountAsync(u => !u.Activo),
                UsuariosPorRol = await _context.Roles
                    .Select(r => new
                    {
                        Rol = r.Nombre,
                        Cantidad = r.Usuarios.Count(u => u.Activo)
                    })
                    .ToListAsync(),
                UltimoRegistro = await _context.Usuarios
                    .OrderByDescending(u => u.FechaRegistro)
                    .Select(u => u.FechaRegistro)
                    .FirstOrDefaultAsync()
            };

            return Ok(ApiResponse<object>.Ok(stats));
        }

        // ─── Mapper helper ───
        private static UsuarioResponseDto MapToDto(Usuario u) => new()
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            NombreUsuario = u.NombreUsuario,
            Telefono = u.Telefono,
            Activo = u.Activo,
            FechaRegistro = u.FechaRegistro,
            UltimoAcceso = u.UltimoAcceso,
            Rol = u.Rol?.Nombre ?? "Sin rol"
        };
    }
}
