using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;  // Correct namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddOpenIdConnect(options =>
{
    options.SignInScheme = IdentityConstants.ExternalScheme;
    options.Authority = "https://auth.pingone.com/29150bc2-4475-48dd-8b02-4256561167ef/as";
    options.ClientId = "c88fb314-c75e-401d-ae10-91fd860ccfc2";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.ClientSecret = "Kfrjfuve96yumDowC2wdoP_PJz1w.R6DjQjzFKKfwWXEgSDkYi.kR~_qq15G2bqE";
    options.Scope.Add("c88fb314-c75e-401d-ae10-91fd860ccfc2");
    options.CallbackPath = new PathString("/callback");
    options.SignedOutCallbackPath = new PathString("/signout-ping-one");
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

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
