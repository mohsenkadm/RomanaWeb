using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi;
using RomanaWeb.Classes;
using RomanaWeb.Helper;
using RomanaWeb.Hubs;
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
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rumana Platform API",
        Version = "v1",
        Description = "REST API powering the Customer, Merchant and Driver apps."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token (without the 'Bearer ' prefix)."
    });
    // Swashbuckle v10 changed security requirement API shape; keep Bearer definition only.
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
        cfg.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();

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

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
}, NullLoggerFactory.Instance);
builder.Services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

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

app.MapHub<OrderNotificationHub>("/hubs/orders");

app.Run();
