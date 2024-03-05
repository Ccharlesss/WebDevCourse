using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebcoursworkV1.Controllers;
using WebcoursworkV1.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------------------- Service added via Dependency injection ---------------------------------
// Registers the services required to work w/ controllers to enable to handle HTTP request
builder.Services.AddControllers();
// Registers the context class as a service in the app allowing other parts of the app to access it to interact w/ DB
// configure DB provider as SQLite in the connection string
builder.Services.AddDbContext<RealEstateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Connection")));
// -----------------------------------------------------------------------------------------------------------



// --------------------------------- Service added via Dependency injection ---------------------------------
// Configures Identity System to use IdentityUser as the user entity and IdentityRole as the role entity in DB
// Stores user and role data in DB and uses the contect class to interact w/ these tables
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
// Generates and validates token used for authentication and authorization purposes
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<RealEstateContext>().AddDefaultTokenProviders();
// -----------------------------------------------------------------------------------------------------------



// --------------------------------- Service added via Dependency injection ---------------------------------
// Purpose: Enable the Webapp to use email services defined previously
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();
// -----------------------------------------------------------------------------------------------------------



// --------------------------------- Service added via Dependency injection ---------------------------------
// New instance of RolesController will be created for each HTTP request
builder.Services.AddScoped<RolesController>();
// Purpose: Configures a new service in Webapp => Set aithetification scheme to JTW
builder.Services.AddAuthentication(options =>
{   // JWT Bearer authentication will be used as the default scheme for authenticating users
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // JWT Bearer authentication will be used as the default scheme for unauthorized requests
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Purpose: Add services to allow to configure the JWT token
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
// -----------------------------------------------------------------------------------------------------------

// --------------------------------------------- Loggin added ---------------------------------------------
builder.Logging.ClearProviders(); // Clears any default loggin providers that was configured to replace it by custom one
builder.Logging.AddConsole(); // Configures the app to log messages to the console
// ------------------------------------------------------------------------------------------------------

// Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
//     .WriteTo.File("LogRepository/LogSafe-.txt", rollingInterval: RollingInterval.Day).CreateLogger();

// builder.Host.UseSerilog();
// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration).CreateLogger();



var app = builder.Build();

// builder.Host.UseSerilog();

// Enable to access the service configured:
using(var scope = app.Services.CreateScope())
{   
    // Add Roles in the system when the app launches:
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // Create the various roles
    var roles = new[] {"Manager", "Admin", "User"};

    foreach (var role in roles)
    {   // Assess if the role does not exist to prevent duplicate entry
        if(!await roleManager.RoleExistsAsync(role))
        {   // If does not exist => create a new role via the roleManager
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}


using(var scope = app.Services.CreateScope())
{ 
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    // Want to have this account build in our environment once and not created everytime the app is launched;
    
    //  Add userManager attributes:
    string email = "ceorealfinance@gmail.com";
    string password = "Defaultpasseword12$";
    
    // userManager look in system to check if an account with this email already exist:
    if(await userManager.FindByEmailAsync(email)==null)
    {
        // if doesnt exist, create the user however it is not yet in database
        var user = new IdentityUser();
        user.UserName = email;
        user.Email = email;

        // UserManager creates user's account and adds it to DB
        await userManager.CreateAsync(user, password);
        // 
        await userManager.AddToRoleAsync(user, "Admin");


    }


}


// ==========================================================================================================
// =================================== Implementation of Serilog library ====================================
//optional configuration settings 1:
// builder.Host.UseSerilog((context, configuration)=> 
//     configuration.WriteTo.Console()
//     .MinimumLevel.Warning());

// optional configuration settings 2:
// Read the log configuration defined in Appsettings.Json
// builder.Host.UseSerilog((context, configuration)=> 
//     configuration.ReadFrom.Configuration(context.Configuration));

// // log HTTP request to our Core Web API:
// app.UseSerilogRequestLogging();

// This or
// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Information()
//     .WriteTo.Console()
//     .WriteTo.File("LogRepository/LogSafe-.txt", rollingInterval: RollingInterval.Day)
//     .CreateLogger();



// builder.Host.UseSerilog();

// Configure routing: Maps user's HTTP request to the ontroller's action methods
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects HTTP request to HTTPS to ensure data is transmitted securely over an encripted connection
app.UseHttpsRedirection();



app.Run();
