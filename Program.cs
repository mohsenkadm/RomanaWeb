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
builder.Services.AddDbContext<DB_Context>(option => option.UseSqlServer(DBConn.ConnectionString),
    ServiceLifetime.Scoped, ServiceLifetime.Scoped);
builder.Services.AddSignalR();
builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromDays(10);
});
AppRegisterServices.RegisterServices<IRegisterScopped>(builder.Services);
AppRegisterServices.RegisterServices<IRegisterSingleton>(builder.Services);
builder.Services.AddSingleton(new MapperConfiguration(config => config.AddProfile(new MappingProfile())).CreateMapper());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
                              
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();