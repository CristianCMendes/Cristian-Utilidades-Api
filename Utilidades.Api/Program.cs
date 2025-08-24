using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using EntityEase.Models.Repo;
using EntityEase.Models.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using Utilidades.Api.Context;
using Utilidades.Api.Services;

var builder = WebApplication.CreateBuilder(args);



EEConfig.EntityNamingMode = NamingMode.SnakeCase;
// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddDbContext<UtilDbContext>(opt => {
    opt.UseNpgsql(builder.Configuration.GetConnectionString("UtilDbContext"));
    opt.UseSnakeCaseNamingConvention();
    opt.EnableSensitiveDataLogging();
    opt.UseLazyLoadingProxies(false);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

// Autenticação JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey)) {
    throw new InvalidOperationException("Configuração Jwt:Key não definida.");
}

var issuer = builder.Configuration["Jwt:Issuer"] ?? "Utilidades.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "Utilidades.Api.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    }).AddCookie();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IMailService, MailService>();

builder.Services.AddOpenApi("v1");


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi().WithOpenApi();
    Env.Load(Directory.GetCurrentDirectory() + "/.env");

}

app.MapControllers();
app.UseResponseCaching();
app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

await using (var validationScope = app.Services.CreateAsyncScope()) {
    var dbContext = validationScope.ServiceProvider.GetRequiredService<UtilDbContext>();
    // Garante que o banco foi criado e tá na ultima versão.
    await dbContext.Database.MigrateAsync();

    // Ve se tem usuarios no banco, caso não tenha o primeiro vai ter o privilegio de administrador.
    UtilDbContext.HasUsers = await dbContext.Users.AnyAsync();
}

app.Run();