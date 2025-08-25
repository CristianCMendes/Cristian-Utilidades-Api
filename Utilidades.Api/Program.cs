using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using EntityEase.Models.Repo;
using EntityEase.Models.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Utilidades.Api.Context;
using Utilidades.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var envfile = Directory.GetCurrentDirectory() + "/.env";
var hasEnvFile = File.Exists(envfile);
if (hasEnvFile) {
    Env.Load(envfile);
}

EEConfig.EntityNamingMode = NamingMode.SnakeCase;
// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddDbContext<UtilDbContext>(opt => {
    opt.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_DB"));
    opt.UseSnakeCaseNamingConvention();
    opt.EnableSensitiveDataLogging();
    opt.UseLazyLoadingProxies(false);
});


// Open Api não respeita o serializador dos controllers então fiz um separado...
builder.Services.Configure<JsonOptions>(o => {
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.MaxDepth = 256;
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

});
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(o => {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.MaxDepth = 256;
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
);
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
builder.Services.AddScoped<ISecretFriendService, SecretFriendService>();

builder.Services.AddOpenApi("v1", options => {
    options.AddDocumentTransformer((document, context, ct) => {
        var servers = builder.Configuration.GetSection("OpenApi:Servers")
                          .Get<List<OpenApiServer>>() ??
                      [];
        foreach (var s in servers) {
            document.Servers.Add(s);
        }


        return Task.CompletedTask;
    });

    // Add security scheme to all endpoints.
    options.AddDocumentTransformer((document, context, ct) => {
        document.Components ??= new OpenApiComponents();

        var bearerScheme = new OpenApiSecurityScheme {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Envie o token JWT no header Authorization: Bearer {token}",
        };

        document.Components.SecuritySchemes["Bearer"] = bearerScheme;

        var globalRequirement = new OpenApiSecurityRequirement {
            [new() {
                Reference = new() {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        };

        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        // Evita duplicar em execuções repetidas
        if (!document.SecurityRequirements.Any())
            document.SecurityRequirements.Add(globalRequirement);

        return Task.CompletedTask;
    });

    // Allows anonymous access to controllers with the [AllowAnonymous] attribute.
    options.AddOperationTransformer((operation, context, ct) => {
        var isAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (isAnonymous) {
            operation.Security?.Clear();
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
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
    if (!dbContext.Database.HasPendingModelChanges()) {
        await dbContext.Database.MigrateAsync();
    }

    try {
        // Ve se tem usuarios no banco, caso não tenha o primeiro vai ter o privilegio de administrador.
        UtilDbContext.HasUsers = await dbContext.Users.AnyAsync();
    }
    catch (Exception ex) {
        Console.WriteLine(ex);
    }
}

app.Run();