using CBCMissionaryWallApi.Data;
using CBCMissionaryWallApi.Extensions;
using CBCMissionaryWallApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCustomSwagger();

//get a connection to database 
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

//configure for postgres 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

//Add identity endpoints 
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
}).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Admin Policy
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

//Email Sender Service
builder.Services.AddTransient<IEmailSender, ConsoleEmailService>();

//enable validation for minimal Apis
builder.Services.AddValidation();   

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

var authRouteGroup = app.MapGroup("/api/auth")
    .WithTags("Admin");

authRouteGroup.MapIdentityApi<ApplicationUser>();

app.MapGet("/", () => "Hello World!")
    .WithName("Welcome Message");

app.Run();


