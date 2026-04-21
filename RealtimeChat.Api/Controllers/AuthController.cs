using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace RealtimeChat.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CustomRegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            ShortId = request.ShortId
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded) return Ok();

        var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
        return ValidationProblem(new ValidationProblemDetails(errors));
    }

    [Authorize(AuthenticationSchemes = "Identity.Bearer")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        return Ok(new { user.DisplayName, user.ShortId });
    }
}

public class CustomRegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ShortId { get; set; } = string.Empty;
}