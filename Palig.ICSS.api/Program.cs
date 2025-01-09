using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Palig.ICSS.support.Context;
using Palig.ICSS.support.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;


var client_id = builder.Configuration.GetSection("OIDC:ClientId").Value;
var Authority = builder.Configuration.GetSection("OIDC:Authority").Value;
var MetadataAddress = builder.Configuration.GetSection("OIDC:MetadataAddress").Value ?? string.Empty;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(options =>
{
    options.Authority = Authority;
    options.Audience = client_id;
    options.RequireHttpsMetadata = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Authority,
        ValidateAudience = true,
        ValidAudience = client_id,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
    options.MetadataAddress = MetadataAddress;
});

builder.Services.AddAuthorization();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDIExtension();

builder.Services.AddOptionServiceCustom(builder.Configuration);

builder.Services.AddDbContext<SQLDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.Use(async (context, next) =>
{
    int longitud = 20;
    Guid miGuid = Guid.NewGuid();
    string newNonce = miGuid.ToString().Replace("-", string.Empty).Substring(0, longitud);

    StringBuilder sb = new StringBuilder();

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Add("Server", " ");


        context.Response.Headers.Remove("X-Frame-Options");
        context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");


        context.Response.Headers.Remove("X-Content-Type-Options");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");


        context.Response.Headers.Remove("X-Xss-Protection");
        context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");


        context.Response.Headers.Remove("Referrer-Policy");
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");

        context.Response.Headers.Remove("Referrer-Policy");
        context.Response.Headers.Add("Feature-Policy", "geolocation 'self'");

        context.Response.Headers.Remove("X-Permitted-Cross-Domain-Policies");
        context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");


        context.Response.Headers.Remove("Content-Security-Policy");
        context.Response.Headers.Add("Content-Security-Policy", sb.ToString());

        return Task.FromResult(0);
    });


    await next();
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost4200");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
