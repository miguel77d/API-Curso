using API_Cursos_Online.Interfaces;
using API_Cursos_Online.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace API_Cursos_Online.Services
{
    public class EstudianteService : IEstudianteService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public EstudianteService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task RegistrarEstudiante(RegistrarEstudianteDTO dto)
        {
            // Verificar si el correo ya está registrado
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new Exception("El correo electrónico ya está registrado.");

            // Crear el usuario (estudiante)
            var estudiante = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(estudiante, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al registrar el estudiante: {errors}");
            }

            // Asignar el rol "Estudiante" al usuario
            if (!await _roleManager.RoleExistsAsync("Estudiante"))
                await _roleManager.CreateAsync(new IdentityRole("Estudiante"));

            await _userManager.AddToRoleAsync(estudiante, "Estudiante");
        }

        public async Task<string> Login(LoginDTO dto)
        {
            // Buscar al estudiante por correo electrónico
            var estudiante = await _userManager.FindByEmailAsync(dto.Email);

            if (estudiante == null || !await _userManager.CheckPasswordAsync(estudiante, dto.Password))
                throw new Exception("Credenciales inválidas");

            // Verificar configuraciones del JWT
            var keyValue = _configuration["Jwt:Key"] ?? throw new Exception("La clave JWT (Jwt:Key) no está configurada.");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new Exception("El emisor (Jwt:Issuer) no está configurado.");
            var audience = _configuration["Jwt:Audience"] ?? throw new Exception("La audiencia (Jwt:Audience) no está configurada.");

            // Definir los claims del estudiante
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, estudiante.Id),
                new Claim(ClaimTypes.Email, estudiante.Email),
                new Claim(ClaimTypes.Role, "Estudiante")
            };

            // Generar clave de seguridad y credenciales de firma
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Generar el token JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
