using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private  readonly ApplicationDbContext _context;

    // ฟังก์ชัน Constructor สำหรับการ initial ค่าของ ApplicationDbContext
    public AuthenticateController(ApplicationDbContext context)
    {
        _context = context;
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
}
