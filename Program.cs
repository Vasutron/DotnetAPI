
using System.Text;
using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Entity Framework connection string to the database postgresql
builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Adding Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options  => {
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("JWT:ValidAudience").Value!,
        ValidIssuer = builder.Configuration.GetSection("JWT:ValidIssuer").Value!,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Secret").Value!))
    };
});

// Allow CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MultipleOrigins",
    policy =>
    {
        policy.WithOrigins(
            // "*", // Allow all origins
            "http://localhost:3000", // React app
            "http://localhost:5173", // Vite app
            "http://localhost:8080", // Vue app
            "http://localhost:8081", // Angular app
            "http://localhost:4200", // Angular app
            "http://localhost:8082", // Svelte app
            "http://localhost:8083", // Ember app
            "http://localhost:8084", // Backbone app
            "http://localhost:8085", // Preact app
            "http://localhost:8086", // Lit app
            "http://localhost:8087", // Alpine app
            "http://localhost:8088", // Solid app
            "http://localhost:8089", // Astro app
            "http://localhost:8090", // Qwik app
            "http://localhost:8091", // SvelteKit app
            "http://localhost:8092", // Remix app
            "http://localhost:8093", // Next.js app
            "http://localhost:8094", // Laravel app
            "http://localhost:8095", // Django app
            "http://localhost:8096", // Flask app
            "http://*.azurewebsites.net", // Azure app
            "http://*.herokuapp.com", // Heroku app
            "http://*.ngrok.io", // ngrok app   
            "http://*.vercel.app", // Vercel app
            "http://*.netlify.app", // Netlify app
            "http://*.surge.sh", // Surge app
            "http://*.github.io", // GitHub Pages app
            "http://*.gitlab.io", // GitLab Pages app
            "http://*.bitbucket.io", // Bitbucket Pages app
            "http://*.bitbucket.org" // Bitbucket Pages app
        )
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.SwaggerDoc("v1", new() { Title = "StockAPI", Version = "v1" });

        options.AddSecurityDefinition("Bearer",  new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat= "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    }

);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// User CORS
app.UseCors("MultipleOrigins");

// Add Authentication
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
