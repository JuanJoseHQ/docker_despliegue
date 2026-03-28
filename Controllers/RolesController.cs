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
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todos los roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RolResponseDto>>), 200)]
        public async Task<IActionResult> ObtenerTodos()
        {
            var roles = await _context.Roles
                .Select(r => new RolResponseDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    Activo = r.Activo,
                    CantidadUsuarios = r.Usuarios.Count(u => u.Activo)
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RolResponseDto>>.Ok(roles));
        }

        /// <summary>
        /// Obtener un rol por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RolResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var rol = await _context.Roles
                .Include(r => r.Usuarios)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null)
                return NotFound(ApiResponse<object>.Error("Rol no encontrado"));

            var response = new RolResponseDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo,
                CantidadUsuarios = rol.Usuarios.Count(u => u.Activo)
            };

            return Ok(ApiResponse<RolResponseDto>.Ok(response));
        }

        /// <summary>
        /// Crear un nuevo rol
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RolResponseDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Crear([FromBody] CrearRolDto dto)
        {
            if (await _context.Roles.AnyAsync(r => r.Nombre == dto.Nombre))
                return BadRequest(ApiResponse<object>.Error("Ya existe un rol con ese nombre"));

            var rol = new Rol
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            var response = new RolResponseDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo,
                CantidadUsuarios = 0
            };

            return CreatedAtAction(nameof(ObtenerPorId), new { id = rol.Id },
                ApiResponse<RolResponseDto>.Ok(response, "Rol creado exitosamente"));
        }

        /// <summary>
        /// Actualizar un rol existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RolResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CrearRolDto dto)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound(ApiResponse<object>.Error("Rol no encontrado"));

            if (await _context.Roles.AnyAsync(r => r.Nombre == dto.Nombre && r.Id != id))
                return BadRequest(ApiResponse<object>.Error("Ya existe otro rol con ese nombre"));

            rol.Nombre = dto.Nombre;
            rol.Descripcion = dto.Descripcion;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<RolResponseDto>.Ok(new RolResponseDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo
            }, "Rol actualizado exitosamente"));
        }

        /// <summary>
        /// Desactivar un rol (borrado lógico)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Desactivar(int id)
        {
            var rol = await _context.Roles.Include(r => r.Usuarios).FirstOrDefaultAsync(r => r.Id == id);
            if (rol == null)
                return NotFound(ApiResponse<object>.Error("Rol no encontrado"));

            if (rol.Usuarios.Any(u => u.Activo))
                return BadRequest(ApiResponse<object>.Error(
                    $"No se puede desactivar el rol porque tiene {rol.Usuarios.Count(u => u.Activo)} usuario(s) activo(s)"));

            rol.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { }, "Rol desactivado exitosamente"));
        }
    }
}
