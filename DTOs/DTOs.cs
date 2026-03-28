using System.ComponentModel.DataAnnotations;

namespace GestionUsuariosAPI.DTOs
{
    // ──────── USUARIO DTOs ────────

    public class RegistroUsuarioDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public int RolId { get; set; }
    }

    public class ActualizarUsuarioDto
    {
        [MaxLength(100)]
        public string? NombreCompleto { get; set; }

        [EmailAddress(ErrorMessage = "El formato de email no es válido")]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public int? RolId { get; set; }
    }

    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string Rol { get; set; } = string.Empty;
    }

    // ──────── AUTH DTOs ────────

    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public UsuarioResponseDto? Usuario { get; set; }
    }

    public class CambiarPasswordDto
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NuevaPassword { get; set; } = string.Empty;
    }

    // ──────── ROL DTOs ────────

    public class CrearRolDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }

    public class RolResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    // ──────── RESPUESTA GENÉRICA ────────

    public class ApiResponse<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string mensaje = "Operación exitosa")
            => new() { Exitoso = true, Mensaje = mensaje, Data = data };

        public static ApiResponse<T> Error(string mensaje)
            => new() { Exitoso = false, Mensaje = mensaje };
    }
}
