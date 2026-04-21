using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeChat.Api.Data;
using RealtimeChat.Api.Hubs;
using RealtimeChat.Api.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddControllers(); 

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ChatDbContext>();

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options => 
{
    options.Events = new BearerTokenEvents 
    {
        OnMessageReceived = context => 
        {
            if (context.Request.Query.TryGetValue("access_token", out var token)) 
            {
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/chathub")) 
                {
                    context.Token = token;
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddDbContext<ChatDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();