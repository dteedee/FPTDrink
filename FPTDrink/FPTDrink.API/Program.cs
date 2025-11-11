using FPTDrink.API.Extensions;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FptdrinkContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// CORS
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

// AuthN/AuthZ (JWT)
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
});
builder.Services.AddControllers(options =>
{
	options.Filters.Add(new FPTDrink.API.Filters.ValidationFilter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure registrations (DbContext, Repository, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWeb");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
