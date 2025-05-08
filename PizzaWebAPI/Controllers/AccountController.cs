using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PizzaDeliveryWeb.Application.Services;
using PizzaDeliveryWeb.Domain.Entities;
using PizzaWebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления учётными записями пользователей: регистрация, вход, выход, валидация токена.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly CartService _cartService;
        private readonly ILogger<AccountController> _logger;


        /// <summary>
        /// Конструктор контроллера аккаунтов.
        /// </summary>
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IConfiguration configuration, CartService cartService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Регистрирует нового пользователя и автоматически авторизует его.
        /// </summary>
        /// <param name="model">Модель с регистрационными данными.</param>
        /// <returns>JWT токен и данные пользователя при успешной регистрации.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            //var user = new User { UserName = model.UserName, Email = model.Email };
            var user = new User { UserName = model.UserName,
            FirstName=model.FirstName,
            LastName=model.Lastname,
            Surname=model.Surname,
            Address=model.Address,
            Email=model.EmailAddress,
            PhoneNumber=model.PhoneNumber};
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                //await _userManager.AddToRoleAsync(user, model.Role);
                var roleResult = await _userManager.AddToRoleAsync(user, "client");
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
                await _signInManager.SignInAsync(user, isPersistent: false);
                var token = GenerateJwtToken(user);
                int cartId = await _cartService.GetOrCreateCartIdAsync(user.Id);

                return Ok(new { Token = token, userName = user.UserName, userRole = "client", cartId, Address=user.Address });
                //return Ok(new { Message = "Пользователь успешно зарегистрировался" });
            }
            return BadRequest(result.Errors);
        }


        /// <summary>
        /// Выполняет вход пользователя по имени и паролю.
        /// </summary>
        /// <param name="model">Модель входа с логином и паролем.</param>
        /// <returns>JWT токен и информация о пользователе при успешной аутентификации.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _signInManager.PasswordSignInAsync(model.UserName, 
                model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                var token = GenerateJwtToken(user);
                IList<string> roles = await _userManager.GetRolesAsync(user);
                string userRole = roles.FirstOrDefault();
                if (userRole == "client")
                {
                    int cartId = await _cartService.GetOrCreateCartIdAsync(user.Id);
                    return Ok(new { Token = token, userName = user.UserName, userRole, cartId, Address=user.Address });
                }
                //var roles = await _userManager.GetRolesAsync(user);
                //if (!roles.Contains(model.SelectedRole))
                //    return BadRequest("Выбранная роль указана неверно");
                return Ok(new { Token = token, userName = user.UserName, userRole });
            }
            return Unauthorized();
        }


        /// <summary>
        /// Выполняет выход текущего авторизованного пользователя.
        /// </summary>
        /// <returns>Результат выхода.</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Пользователь успешно вышел" });
        }


        /// <summary>
        /// Проверяет валидность текущего JWT токена.
        /// </summary>
        /// <returns>Информация о текущем пользователе, если токен действителен.</returns>
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateToken()
        {
            User usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr == null)
            {
                return Unauthorized(new { message = "Вы Гость. Пожалуйста, выполните вход" });
            }
            IList<string> roles = await _userManager.GetRolesAsync(usr);
            string userRole = roles.FirstOrDefault();
            return Ok(new { message = "Сессия активна", userName = usr.UserName, userRole });

        }

        /// <summary>
        /// Генерирует JWT токен на основе информации о пользователе.
        /// </summary>
        /// <param name="user">Пользователь, для которого создается токен.</param>
        /// <returns>JWT токен.</returns>
        private string GenerateJwtToken(User user)
        {
            try
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),

            };

            var roles = _userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            
                var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка", ex.Message);
                throw new Exception();
            }
        }

        //private string GenerateJwtToken(User user, string selectedRole)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim("SelectedRole", selectedRole.ToString())
        //    };

        //    var roles = _userManager.GetRolesAsync(user).Result;
        //    claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: expires,
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }

}
