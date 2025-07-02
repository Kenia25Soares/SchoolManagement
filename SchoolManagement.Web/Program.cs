using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using SchoolManagement.Web.Helpers;
using Syncfusion.Licensing;
using Swashbuckle.AspNetCore.Swagger;



var builder = WebApplication.CreateBuilder(args);


//Link Syncfusion license key from appsettings.json
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey"]);

// Database -DB Context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// Auth
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/NotAuthorized";
});

// Add MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // se usares Razor também


// Configuração do Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Management API",
        Version = "v1",
        Description = "API for accessing student classes and related data"
    });

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// Services
builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddTransient<IMailHelper, MailHelper>();
builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
builder.Services.AddScoped<IStudentClassHelper, StudentClassHelper>();
builder.Services.AddScoped<ICourseHelper, CourseHelper>();
builder.Services.AddScoped<IStudentGradeHelper, StudentGradeHelper>();

// Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();
builder.Services.AddScoped<IStudentAbsenceHelper, StudentAbsenceHelper>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "School API v1");
    c.RoutePrefix = "swagger"; // Acesso ao swagger
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Public}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<SeedDb>();
    await seeder.SeedAsync(); // <- Cria o Admin user
}

await app.RunAsync(); 
