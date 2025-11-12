using FPTDrink.API.Extensions;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FPTDrink.API.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FptdrinkContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowWeb", policy =>
	{
		var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "https://localhost:5002", "https://localhost:44305", "http://localhost:4200" };
		policy.WithOrigins(origins)
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-key-change-in-production-please";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FPTDrink";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FPTDrinkClients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ValidIssuer = jwtIssuer,
			ValidAudience = jwtAudience,
			IssuerSigningKey = signingKey,
			ClockSkew = TimeSpan.FromSeconds(30)
		};
	});
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("Admin", p => p.RequireRole("Admin"));
	options.AddPolicy("Employee", p => p.RequireRole("Employee", "Admin"));
	options.AddPolicy("ThuNgan", p => p.RequireRole("Thu ngân", "Admin"));
	options.AddPolicy("KeToan", p => p.RequireRole("Kế toán", "Admin"));
});
builder.Services.AddControllers(options =>
{
	options.Filters.Add(new FPTDrink.API.Filters.ValidationFilter());
})
.AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	options.JsonSerializerOptions.WriteIndented = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionAuthorizationMiddlewareResultHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<FptdrinkContext>();
	await FPTDrink.Infrastructure.Data.Seed.PermissionSeeder.SeedAsync(db);
}

app.MapControllers();

app.Run();
