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
	
	if (string.IsNullOrEmpty(connectionString))
	{
		throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
	}

	builder.Services.AddDbContext<DataContext>(options =>
		options.UseSqlServer(connectionString));
}
catch (Exception ex)
{
	// Fallback to in-memory database for testing
	builder.Services.AddDbContext<DataContext>(options =>
		options.UseInMemoryDatabase("TestDatabase"));
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
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["JWT:Key"] ?? "uma-chave-secreta-bem-forte-1234567890";
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "SchoolManagementAPI";

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
builder.Services.AddScoped<IGradesRepository, GradesRepository>();
// Additional repositories for public catalog endpoints
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<ISubjectEnrollmentRequestRepository, SubjectEnrollmentRequestRepository>();
builder.Services.AddScoped<API.SchoolManagement.Data.Repositories.IAlertRepository, API.SchoolManagement.Data.Repositories.AlertRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "School Management API is running! Test with Postman at /api/account");

app.Run();
