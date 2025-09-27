using AspNetDemoPortalAPI.Data;
using AspNetDemoPortalAPI.MySockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemory");

/*
 * 
 * set useInMemory to false prior to running migrations
 * 
 * *********************************************************/
//useInMemory = false;
if (useInMemory)
{
    builder.Services.AddDbContext<DemoPortalContext>(options =>
        options.UseInMemoryDatabase("DemoPortalDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
    builder.Services.AddDbContext<DemoPortalContext>(options =>
        options.UseNpgsql(connectionString));
}

if (useInMemory)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.BackgroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine("✅ Using In-Memory DB");
}
else
{
    Console.ForegroundColor = ConsoleColor.Black;
    Console.BackgroundColor = ConsoleColor.Red;
    Console.WriteLine("🐘 Using PostgreSQL DB");
}

Console.ForegroundColor = ConsoleColor.Yellow;
Console.BackgroundColor = ConsoleColor.Black;
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.ResetColor();
System.Diagnostics.Debug.WriteLine("🌿 Environment: Development");

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"✅ Token validated for: {context.Principal.Identity.Name}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"📥 Token received: {context.Token}");
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });
builder.Services.AddSingleton<PilotTowerWebSocketHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseHsts(); // Optional, only in production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo Portal API V1");
    c.RoutePrefix = "swagger";
});
app.UseWebSockets();
app.Map("/pilot-tower/pilot-tower-messages", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<PilotTowerWebSocketHandler>();
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        await handler.HandleAsync(socket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();