using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MonitorHub.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MonitorHub.Apis
{
    [Route("Service/Access")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AccessController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("SignIn")]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserIdentity identity)
        {
            if (!IsLogin())
            {
                if (identity.UserName == "" || identity.Password == null)
                {
                    return BadRequest("输入不规范");
                }
                var token = JWTAuthenticate(identity);
                return Ok(token);
            }
            else
            {
                return new ObjectResult("您已经注册过了") { StatusCode = 201 };
            }
        }

        [Route("Test")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public IActionResult Test([FromBody] UserIdentity identity)
        {
            JWTAuthenticate(identity);
            return Ok();
        }

        [Route("Test2")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public IActionResult Test2()
        {
            return Ok();
        }

        [Route("SignOut")]
        [HttpGet]
        public IActionResult Logout()
        {
            if (IsLogin())
                HttpContext.SignOutAsync().Wait();
            return Ok("已退出登录");
        }

        [Route("Current")]
        [HttpGet]
        public IActionResult 检查()
        {
            if (IsLogin())
            {
                return Ok("您已经登录");
            }
            else
            {
                return new ObjectResult("您是匿名用户") { StatusCode = 201 };
            }
        }

        /// <summary>
        /// Cookie验证
        /// </summary>
        /// <param name="identity"></param>
        private void Authenticate(UserIdentity identity)
        {
            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,identity.UserName ?? ""),
                    new Claim(ClaimTypes.Hash,identity.Password??""),
                    new Claim(ClaimTypes.Role,identity.Role="User"),
                };
            var Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, identity.Role));
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, Principal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddHours(3),
                        IsPersistent = true,
                        AllowRefresh = true
                    }).Wait();
        }
        /// <summary>
        /// JWT验证
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private string JWTAuthenticate(UserIdentity identity)
        {
            identity.UserId = Guid.NewGuid().ToString();
            var singingAlgorithm = SecurityAlgorithms.HmacSha256;
            //payload
            var claims = new Claim[] {
                new Claim(ClaimTypes.Name,identity.UserName??""),
                new Claim(JwtRegisteredClaimNames.Sid,identity.UserId),
                new Claim(JwtRegisteredClaimNames.Prn,identity.Password??""),
                new Claim(ClaimTypes.Role,identity.Role="User"),
            };
            //signiture
            var secretByte = Encoding.UTF8.GetBytes(_configuration["Authentication:secret"]);
            var signingkey = new SymmetricSecurityKey(secretByte);
            var singingCredentials = new SigningCredentials(signingkey, singingAlgorithm);
            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:issuer"],
                audience: _configuration["Authentication:audience"],
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(1),
                singingCredentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 是否验证
        /// </summary>
        /// <returns></returns>
        private bool IsLogin()
        {
            return HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated;
        }

    }



}
