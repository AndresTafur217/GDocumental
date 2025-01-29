using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Gestion_documental.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Gestion_documentalContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Gestion_documentalContext") ?? throw new InvalidOperationException("Connection string 'Gestion_documentalContext' not found.")));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacion}/{action=Login}/{id?}");

app.Run();
