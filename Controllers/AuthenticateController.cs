using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using DotnetStockAPI.Models;
using static DotnetStockAPI.Models.UserRolesModel;

namespace DotnetStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Object จัดการ Users
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    // ฟังก์ชัน Constructor สำหรับการ initial ค่าของ ApplicationDbContext
    public AuthenticateController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
        )
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // ทดสอบเขียน function เชื่อมต่อกับ Database
    [HttpGet("testconnectdb")]
    public void TestConnection()
    {
        if (_context.Database.CanConnect())
        {
            // Console.WriteLine("Database connection successful.");
            Response.WriteAsync("Connection successful.");
        }
        else
        {
            // Console.WriteLine("Database connection failed.");
            Response.WriteAsync("Not connected!");
        }
    }

    // Function register for user
    // POST: api/authenticate/register-user
    [HttpPost("register-user")] // [Route("register-user")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterModel model)
    {
        // เช็คว่า username ซ้ำหรือไม่
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User already exists!",
                    Description = "มีผู้ใช้นี้ในระบบแล้ว"
                }
            );
        }

        // เช็คว่า email ซ้ำหรือไม่
        userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "Email already exists!",
                    Description = "มีอีเมลนี้ในระบบแล้ว"
                }
            );
        }

        // สร้าง User
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        // สร้าง User ในระบบ
        var result = await _userManager.CreateAsync(user, model.Password);

        // ถ้าสร้างไม่สำเร็จ
        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again.",
                    Description = "การสร้างผู้ใช้ล้มเหลว! กรุณาตรวจสอบรายละเอียดผู้ใช้และลองอีกครั้ง"
                }
            );
        }

        // กำหนด Roles Admin, Manager, User
        if (!await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
        }

        if (!await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Manager));
        }

        if (await _roleManager.RoleExistsAsync(UserRolesModel.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
            await _userManager.AddToRoleAsync(user, UserRolesModel.User);
        }

        return Ok(new ResponseModel
        {
            Status = "Success",
            Message = "User registered successfully",
            Description = "ลงทะเบียนสำเร็จ"
        });
    }

    // Function register for manager
    // POST: api/authenticate/register-manager
    [HttpPost("register-manager")] // [Route("register-manager")]
    public async Task<ActionResult> RegisterManager([FromBody] RegisterModel model)
    {
        // เช็คว่า username ซ้ำหรือไม่
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User already exists!",
                    Description = "มีผู้ใช้นี้ในระบบแล้ว"
                }
            );
        }

        // เช็คว่า email ซ้ำหรือไม่
        userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "Email already exists!",
                    Description = "มีอีเมลนี้ในระบบแล้ว"
                }
            );
        }

        // สร้าง User
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        // สร้าง User ในระบบ
        var result = await _userManager.CreateAsync(user, model.Password);

        // ถ้าสร้างไม่สำเร็จ
        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again.",
                    Description = "การสร้างผู้ใช้ล้มเหลว! กรุณาตรวจสอบรายละเอียดผู้ใช้และลองอีกครั้ง"
                }
            );
        }

        // กำหนด Roles Admin, Manager, User
        if (!await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
        }

        if (!await _roleManager.RoleExistsAsync(UserRolesModel.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
        }

        if (await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
        {
            await _userManager.AddToRoleAsync(user, UserRolesModel.Manager);
        }

        return Ok(new ResponseModel
        {
            Status = "Success",
            Message = "User registered successfully",
            Description = "ลงทะเบียนสำเร็จ"
        });
    }

    // Function register for admin
    // POST: api/authenticate/register-admin
    [HttpPost("register-admin")] // [Route("register-admin")]
    public async Task<ActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        // เช็คว่า username ซ้ำหรือไม่
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User already exists!",
                    Description = "มีผู้ใช้นี้ในระบบแล้ว"
                }
            );
        }

        // เช็คว่า email ซ้ำหรือไม่
        userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "Email already exists!",
                    Description = "มีอีเมลนี้ในระบบแล้ว"
                }
            );
        }

        // สร้าง User
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        // สร้าง User ในระบบ
        var result = await _userManager.CreateAsync(user, model.Password);

        // ถ้าสร้างไม่สำเร็จ
        if (!result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ResponseModel
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again.",
                    Description = "การสร้างผู้ใช้ล้มเหลว! กรุณาตรวจสอบรายละเอียดผู้ใช้และลองอีกครั้ง"
                }
            );
        }

        // กำหนด Roles Admin, Manager, User
        if (await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
            await _userManager.AddToRoleAsync(user, UserRolesModel.Admin);
        }

        if (!await _roleManager.RoleExistsAsync(UserRolesModel.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
        }

        if (!await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
        {
            await _userManager.AddToRoleAsync(user, UserRolesModel.Manager);
        }

        return Ok(new ResponseModel
        {
            Status = "Success",
            Message = "User registered successfully",
            Description = "ลงทะเบียนสำเร็จ"
        });
    }

    // Login for user
    // POST: api/authenticate/login
    [HttpPost("login")] // [Route("login")]
    public async Task<ActionResult> Login([FromBody] LoginModel model)
    {

        var user = await _userManager.FindByNameAsync(model.Username!);

        // ถ้า login สำเร็จ
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userData = new
                {
                    userName = user.UserName,
                    email = user.Email,
                    roles = userRoles
                }
            });
        }

        // ถ้า login ไม่สำเร็จ
        return Unauthorized();
    }

    // ฟังก์ชันสร้าง Token
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows time zone ID

        // Get the current time in Bangkok time zone
        var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: currentTime.AddHours(3), // กำหนดเวลา Token หมดอายุ
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    // Function Logout
    // POST: api/authenticate/logout
    [HttpPost("logout")] // [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;
        if (userName != null)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return Ok(new ResponseModel
                {
                    Status = "Success",
                    Message = "Logout successful",
                    Description = "ออกจากระบบสำเร็จ"
                });
            }
        }
        return Ok();
    }


    // Function Refresh Token
    // POST: api/authenticate/refresh-token
    [HttpPost("refresh-token")] // [Route("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!);

        try
        {
            tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidAudience"],
                ValidateLifetime = false, // Allow expired tokens for refresh
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var userRoles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

            if (userName == null || !userRoles.Any())
            {
                return Unauthorized(new ResponseModel
                {
                    Status = "Error",
                    Message = "Invalid token payload!",
                    Description = "ข้อมูลใน Token ไม่ถูกต้อง!"
                });
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var newToken = GetToken(authClaims);
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newToken),
                expiration = newToken.ValidTo,
                userData = new
                {
                    userName = userName,
                    roles = userRoles
                }
            });
        }
        catch (SecurityTokenExpiredException)
        {
            return Unauthorized(new ResponseModel
            {
                Status = "Error",
                Message = "Token expired!",
                Description = "Token หมดอายุ"
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new ResponseModel
            {
                Status = "Error",
                Message = ex.Message,
                Description = "Token ไม่ถูกต้อง!"
            });
        }
    }

    public class RefreshTokenModel
    {
        public required string Token { get; set; }
    }
}

