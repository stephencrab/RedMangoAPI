using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.Dto;
using RedMangoAPI.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace RedMangoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApiResponse _response;
        private string secretKey;

        public AuthController(ApplicationDbContext dbContext, IConfiguration configuration,
            RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this._dbContext = dbContext;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            this._response = new ApiResponse();
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO login)
        {
            ApplicationUser userFromDb = await _dbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserName == login.UserName);

            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, login.Password);

            if (isValid == false)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username or password is incorrect");
                _response.Result = new LoginResponseDTO();

                return BadRequest(_response);
            }

            //Generate JWT Token
            var roles = await _userManager.GetRolesAsync(userFromDb);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            // 設定 JWT 的發行者、主體及過期時間
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Issuer = "your-issuer",
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("fullName", userFromDb.Name),
                    new Claim("id", userFromDb.Id),
                    new Claim(ClaimTypes.Email, userFromDb.Email),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            // 產生 JWT Token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponse = new LoginResponseDTO()
            {
                Email = userFromDb.Email,
                Token = tokenHandler.WriteToken(token)
            };

            if (loginResponse.Email == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username or password is incorrect");
                _response.Result = new LoginResponseDTO();

                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = loginResponse;

            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO register)
        {
            ApplicationUser userFromDb = _dbContext.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == register.UserName.ToLower());

            if (userFromDb != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = register.UserName,
                Email = register.UserName,
                NormalizedEmail = register.UserName.ToUpper(),
                Name = register.Name,
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser, register.Password);

                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    if (register.Role.ToLower() == SD.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {

            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error while registering");
            return BadRequest(_response);
        }
    }
}
