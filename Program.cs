using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RomanaWeb.Classes;
using RomanaWeb.Helper;
using RomanaWeb.Mapping;
using RomanaWeb.Model;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());
builder.WebHost.UseWebRoot("wwwroot");

// Add services to the container.
builder.Services.AddControllersWithViews();

// OpenAPI / Swagger - documents the REST API for Customer/Merchant/Driver apps.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Rumana Platform API",
        Version = "v1",
        Description = "REST API powering the Customer, Merchant and Driver apps."
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste the JWT token (without the 'Bearer ' prefix)."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(cfg =>
    {
        cfg.SaveToken = true;
        cfg.RequireHttpsMetadata = false;
        cfg.TokenValidationParameters = new()
        {
            ValidIssuer = "RomanaWeb",
            ValidAudience = "Subscriber",
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromHours(10),
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Key.SecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<DB_Context>(
    option => option.UseSqlServer(DBConn.ConnectionString),
    ServiceLifetime.Scoped, ServiceLifetime.Scoped);

builder.Services.AddSignalR();

builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromDays(10);
});

AppRegisterServices.RegisterServices<IRegisterScopped>(builder.Services);
AppRegisterServices.RegisterServices<IRegisterSingleton>(builder.Services);

builder.Services.AddSingleton(
    new MapperConfiguration(config => config.AddProfile(new MappingProfile())).CreateMapper()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Swagger UI is available at /swagger.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rumana Platform API v1"));

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
