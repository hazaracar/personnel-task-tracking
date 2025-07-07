using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;
using PersonelTakip.Models.Seed;
using PersonelTakip.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PermissionService>();

builder.Services.AddScoped<AuditLogService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddSingleton<IConverter, SynchronizedConverter>(s => new SynchronizedConverter(new PdfTools()));


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "Requestverificationtoken";
});




// DbContext 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Microsoft Identity servisleri
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();  // Þifre sýfýrlama ve diðer token saðlayýcýlarý için
builder.Services.Configure<IdentityOptions>(options =>
{
    // Þifre gereksinimleri
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});


var context = new CustomAssemblyLoadContext();
var path = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "wkhtmltox", "libwkhtmltox.dll");
context.LoadUnmanagedLibrary(path);



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



// Authentication ve Authorization middleware'ini ekliyoruz
app.UseAuthentication();  // Kullanýcý doðrulama iþlemleri
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginInfo}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedRolesAsync(services);
    await PermissionSeed.SeedPermissionsAsync(services);

}

app.Run();
