using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		builder =>
		{
			builder.AllowAnyOrigin()
				   .AllowAnyMethod()
				   .AllowAnyHeader();
		});
});

// Configure EF Core with error handling
try
{
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	Console.WriteLine($"Connection String: {connectionString}");

	if (string.IsNullOrEmpty(connectionString))
	{
		throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
	}

	builder.Services.AddDbContext<SchoolManagement.Web.Data.DataContext>(options =>
		options.UseSqlServer(connectionString));

	Console.WriteLine("Entity Framework configured successfully with SQL Server");
}
catch (Exception ex)
{
	Console.WriteLine($"Error configuring Entity Framework: {ex.Message}");
	Console.WriteLine("Falling back to InMemory database...");

	// Fallback to in-memory database for testing
	builder.Services.AddDbContext<SchoolManagement.Web.Data.DataContext>(options =>
		options.UseInMemoryDatabase("TestDatabase"));

	Console.WriteLine("Entity Framework configured with InMemory database");
}

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequiredLength = 6;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<SchoolManagement.Web.Data.DataContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
{
	Console.WriteLine("Warning: JWT configuration is missing. Using default values.");
	jwtKey = "uma-chave-secreta-bem-forte-1234567890";
	jwtIssuer = "SchoolManagementAPI";
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false; // Development
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtIssuer,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});

// Register services from Web project
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<SchoolManagement.Web.Data.Repositories.IGradesRepository, SchoolManagement.Web.Data.Repositories.GradesRepository>();
// Additional repositories for public catalog endpoints
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<ISubjectEnrollmentRequestRepository, SubjectEnrollmentRequestRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	// Swagger removed to prevent 500 error
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "School Management API is running! Test with Postman at /api/account");

app.Run();
