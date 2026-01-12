using LBRS.Member.DBContext;
using LBRS.Member.Repo;
using LBRS.Member.Repo.Interfaces;
using LBRS.Member.Service;
using LBRS.Member.Service.Helper;
using LBRS.Member.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -- SERILOG -
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "Logs/memberservice-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30, // Keep logs for 30 days
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddDbContext<MemberServiceDbContext>(options =>
    options.UseInMemoryDatabase("MemberServiceDatabase"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<IMemberRegistrationRepository, MemberRegistrationRepository>();
builder.Services.AddScoped<IMemberRegistrationService, MemberRegistrationService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtAuthTokenService, JwtAuthTokenService>();
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
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Member Service API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Member Service API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
