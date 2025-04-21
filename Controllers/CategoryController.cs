using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetStockAPI.Controllers;

// [Authorize]  // All Roles

// Multiple Roles
[Authorize(Roles = UserRolesModel.Admin + "," + UserRolesModel.Manager)]
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // Constructor
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // เริ่มต้นสร้าง CRUD API สำหรับ Category

    // Function ดึงข้อมูล Category
    // GET: api/category
    [HttpGet]
    public ActionResult<category> GetCategories()
    {
        // ใช้ LINQ เพื่อดึงข้อมูล Category ทั้งหมดจาก Database ("LINQ = Language Integrated Query")
        var category = _context.categories.ToList(); // เหมือนใช้ Select * from categories ใน SQL

        // ส่งข้อมูลกลับไปยัง Client เป็น JSON
        return Ok(category);
    }

    // Function ดึงข้อมูล Category ตาม ID
    // GET: api/category/{id}
    [HttpGet("{id}")]
    public ActionResult<category> GetCategory(int id)
    {
        var category = _context.categories.Find(id); // เหมือนใช้ Select * from categories where id = {id} ใน SQL
        if (category == null)
        {
            return NotFound(); // ถ้าไม่พบข้อมูลให้ส่ง 404 Not Found
        }
        return Ok(category);
    }

    // Function สร้าง Category ใหม่
    // POST: api/category
    [HttpPost]
    public ActionResult<category> CreateCategory([FromBody] category category)
    {
        _context.categories.Add(category); // เหมือนใช้ Insert into categories (name) values ({name}) ใน SQL
        _context.SaveChanges(); // บันทึก/commit การเปลี่ยนแปลงใน Database

        return Ok(category); // ส่งข้อมูล Category ที่สร้างใหม่กลับไปยัง Client
    }

    // Function อัพเดท Category/แก้ไข Category
    // PUT: api/category/{id}
    [HttpPut("{id}")]
    public ActionResult<category> UpdateCategory(int id, [FromBody] category category)
    {
        // ตรวจสอบว่า Category ที่ต้องการอัพเดทมีอยู่ใน Database หรือไม่
        var cat = _context.categories.Find(id); // เหมือนใช้ Select * from categories where id = {id} ใน SQL

        if (cat == null)
        {
            return NotFound(); // ถ้าไม่พบข้อมูลให้ส่ง 404 Not Found
        }

        // อัพเดทข้อมูล Category
        cat.categoryname = category.categoryname; // อัพเดทชื่อ Category
        cat.categorystatus = category.categorystatus; // อัพเดทสถานะ Category
        _context.SaveChanges();

        return Ok(cat); // ส่งข้อมูล Category ที่อัพเดทกลับไปยัง Client
    }

    // Function ลบ Category
    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteCategory(int id)
    {
        var category = _context.categories.Find(id); // เหมือนใช้ Select * from categories where id = {id} ใน SQL

        if (category == null)
        {
            return NotFound(); // ถ้าไม่พบข้อมูลให้ส่ง 404 Not Found
        }

        _context.categories.Remove(category); // เหมือนใช้ Delete from categories where id = {id} ใน SQL
        _context.SaveChanges(); // บันทึก/commit การเปลี่ยนแปลงใน Database

        return NoContent(); // ส่ง 204 No Content กลับไปยัง Client
    }
        
}