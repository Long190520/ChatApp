using ChatApp.Api.Data;
using ChatApp.Api.Hubs;
using ChatApp.Api.Startup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors();

builder.Services.AddSignalR();

builder.Services.AddDbContext<ChatAppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatApp"));
});

var tokenOptions = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:ValidAudience"],
    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? "")),
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,
};

builder.Services.AddSingleton(tokenOptions);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = tokenOptions;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.RegisterAppService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
    await context.UserConnections.ExecuteDeleteAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/hubs");

app.MapControllers();

app.Run();
