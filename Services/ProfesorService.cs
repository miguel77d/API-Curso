using API_Cursos_Online.Interfaces;
using API_Cursos_Online.DTOs;
using API_Cursos_Online.Context;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace API_Cursos_Online.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public ProfesorService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task RegistrarProfesor(RegistrarProfesorDTO dto)
        {
            // Verificar si el correo ya está registrado
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new Exception("El correo electrónico ya está registrado.");

            // Crear el usuario (profesor)
            var profesor = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(profesor, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al registrar el profesor: {errors}");
            }

            // Asignar el rol "Profesor" al usuario
            if (!await _roleManager.RoleExistsAsync("Profesor"))
                await _roleManager.CreateAsync(new IdentityRole("Profesor"));

            await _userManager.AddToRoleAsync(profesor, "Profesor");
        }

        public async Task<string> Login(LoginDTO dto)
        {
            // Buscar al profesor por correo electrónico
            var profesor = await _userManager.FindByEmailAsync(dto.Email);

            if (profesor == null || !await _userManager.CheckPasswordAsync(profesor, dto.Password))
                throw new Exception("Credenciales inválidas");

            // Validar configuraciones JWT
            var keyValue = _configuration["Jwt:Key"] ?? throw new Exception("La clave JWT (Jwt:Key) no está configurada.");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new Exception("El emisor (Jwt:Issuer) no está configurado.");
            var audience = _configuration["Jwt:Audience"] ?? throw new Exception("La audiencia (Jwt:Audience) no está configurada.");

            // Definir los claims del profesor
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, profesor.Id),
                new Claim(ClaimTypes.Email, profesor.Email),
                new Claim(ClaimTypes.Role, "Profesor")
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
