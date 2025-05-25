using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConsultasMedicasOnline.Data;
using ConsultasMedicasOnline.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using ConsultasMedicasOnline.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=consultasonline.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuração do Identity com modelo customizado
builder.Services.AddDefaultIdentity<Usuario>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Permitir login sem confirmação de email para desenvolvimento
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Registrar serviço de email
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();

// Configuração de cultura para português brasileiro
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-BR" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

// Configuração para criar roles iniciais
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
    
    // Criar roles se não existirem
    string[] roles = { "Administrador", "Medico", "Paciente" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
