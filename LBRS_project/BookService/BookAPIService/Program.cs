using LBRS.Book.DBContext;
using LBRS.Book.Repo;
using LBRS.Book.Repo.Interfaces;
using LBRS.Book.Service;
using LBRS.Book.Service.Helper;
using LBRS.Book.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------- SERILOG ----------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "Logs/bookservice-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30, // Keep logs for 30 days
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddDbContext<BookServiceDbContext>(options =>
    options.UseInMemoryDatabase("BookServiceDatabase"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IHttpClaimContext, HttpClaimContext>();

builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    var jwt = builder.Configuration.GetSection("JwtSettings");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Book Service API",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Service API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
