using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using Syncfusion.Licensing;
using System.Text;

namespace SchoolManagement.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                // Localiza��o personalizada das views
                options.ViewLocationFormats.Add("/Views/AdminDashboard/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/EmployeeDashboard/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/StudentDashboard/{1}/{0}.cshtml");

                // Para as p�ginas de erro, como a 403.cshtml
                options.ViewLocationFormats.Add("/Views/Errors/{0}.cshtml");
            });

            //Link Syncfusion license key 
            SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey"]);

            // Database -DB Context
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0; 
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<DataContext>()  // Faz a separa��o entre o Identity e a base de dados
            .AddDefaultTokenProviders();


            // JWT Authentication
            builder.Services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                message = "Token inv�lido ou n�o fornecido."
                            });
                            return context.Response.WriteAsync(result);
                        }
                    };
                });


            // Auth
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/NotAuthorized";
            });

            // Add MVC
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();


            // Configura��o do Swagger 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "School Management API",
                    Version = "v1",
                    Description = "API for accessing student classes and related data"
                });

                // Localiza��o do XML gerado
                var xmlFilename = "SchoolManagement.Web.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    return apiDesc.ActionDescriptor.RouteValues["controller"]?.Contains("API") ?? false;
                });

                // Adiciona suporte ao JWT no Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the Token Here"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Services e Helpers
            builder.Services.AddTransient<SeedDb>();

            builder.Services.AddScoped<IBlobHelper, BlobHelper>();
            builder.Services.AddScoped<IUserHelper, UserHelper>();
            builder.Services.AddTransient<IMailHelper, MailHelper>();
            builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
            builder.Services.AddScoped<IStudentClassHelper, StudentClassHelper>();
            builder.Services.AddScoped<ICourseHelper, CourseHelper>();
            builder.Services.AddScoped<IStudentGradeHelper, StudentGradeHelper>();
            builder.Services.AddScoped<IStudentAbsenceHelper, StudentAbsenceHelper>();
            // Repositories
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();
            builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
            builder.Services.AddScoped<IGradesRepository, GradesRepository>();
            builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<ISubjectEnrollmentRequestRepository, SubjectEnrollmentRequestRepository>();
           

            var app = builder.Build();

            // Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Errors/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePagesWithReExecute("/Errors/{0}");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "School API v1");
                c.RoutePrefix = "swagger"; // Acesso ao swagger
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseAuthentication(); //tem ser colocado antes do UseAuthorization
            app.UseAuthorization();

            // Routes
            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Public}/{id?}");


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var seeder = services.GetRequiredService<SeedDb>();
                await seeder.SeedAsync(); // Cria o Admin user
            }

            await app.RunAsync();
        }
    }
}