using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetStockAPI.Controllers;

// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // IWebHostEnvironment คืออะไร
    // ContentRootPath คือ path ที่เก็บไฟล์ของโปรเจค
    // WebRootPath คือ path ที่เก็บไฟล์ static เช่น wwwroot ของเว็บแอปพลิเคชัน
    private readonly IWebHostEnvironment _env;

    // Constructor
    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // เริ่มต้นสร้าง CRUD API สำหรับ Product

    // Function ดึงข้อมูล Product
    // GET: api/product
    [HttpGet]
    public ActionResult<product> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 2,
        [FromQuery] string? searchQuery = null,
        [FromQuery] int? selectedCategory = null
    )
    {
        // script แบ่งหน้า (skip)
        var skip = (page - 1) * limit;
        // var product = _context.products.ToList();

        // where แบบมีเงื่อนไข
        // var product = _context.products.Where(p => p.unitinstock > 10).ToList(); // เหมือนใช้ Select * from products ใน SQL
        
        // join table product and category
        // var product = _context.products
        var query = _context.products
        .Join(
            _context.categories,
            p => p.categoryid,
            c => c.categoryid,
            (p, c) => new 
            {
                p.productid,
                p.productname,
                p.unitprice,
                p.unitinstock,
                p.produnctpicture,
                c.categoryname,
                c.categoryid,
                p.createddate,
                p.modifieddate
            }
        );

        // ถ้ามีการค้นหา
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(p => EF.Functions.ILike(p.productname!, $"%{searchQuery}%"));
        }

        // ถ้ามีการค้นหาตามหมวดหมู่
        if (selectedCategory.HasValue)
        {
            query = query.Where(p => p.categoryid == selectedCategory.Value);
        }

        // นับจำนวนข้อมูลทั้งหมด
        var totalRecords = query.Count();

        var product = query
        .OrderByDescending(p => p.productid)
        .Skip(skip)
        .Take(limit)
        .ToList();

        return Ok(
            new {
                Total = totalRecords,
                Products = product
            }
        );
    }

    [HttpGet("{id}")]
    public ActionResult<product> GetProduct(int id)
    {
        // var product = _context.products.Find(id);

        var product = _context.products
        .Join(
            _context.categories,
            p => p.categoryid,
            c => c.categoryid,
            (p, c) => new 
            {
                p.productid,
                p.productname,
                p.unitprice,
                p.unitinstock,
                p.produnctpicture,
                c.categoryname,
                c.categoryid,
                p.createddate,
                p.modifieddate
            }
        )
        .FirstOrDefault(p => p.productid == id);


        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<product>> CreateProduct([FromForm] product product, IFormFile? image)
    {
        _context.products.Add(product);

        // ถ้ามีการอัพโหลดไฟล์
        if (image != null)
        {
            // กำหนดชื่อรูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);


            // บกำหนดเส้นทางไปยังโฟลเดอร์ uploads
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads มีอยู่หรือไม่ ถ้าไม่มีให้สร้าง
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            product.produnctpicture = fileName;
        } else
        {
            product.produnctpicture = "noimg.png";
        }

        _context.SaveChanges();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id, [FromBody] product product)
    {
        var productData = _context.products.Find(id);

        if (productData == null)
        {
            return NotFound();
        }

        productData.productname = product.productname;
        productData.unitprice = product.unitprice;
        productData.unitinstock = product.unitinstock;
        productData.produnctpicture = product.produnctpicture;
        productData.categoryid = product.categoryid;
        productData.modifieddate = DateTime.Now;

        _context.SaveChanges();

        return Ok(productData);
    }

    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id)
    {
        var product = _context.products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.products.Remove(product);
        _context.SaveChanges();

        return Ok(product);
    }

}
