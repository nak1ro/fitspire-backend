using backend.Modules.Auth.DTOs;
using backend.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Auth.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var userDto = await _authService.RegisterAsync(dto);
        return Ok(userDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var userDto = await _authService.LoginAsync(dto);
        return Ok(userDto);
    }
    
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var success = await _authService.ConfirmEmailAsync(dto.UserId, dto.Token);
        if (success)
            return Ok("Email confirmed.");
        else
            return BadRequest("Invalid token.");
    }
    
    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto)
    {
        var userDto = await _authService.ExternalLoginAsync(dto);
        return Ok(userDto);
    }
}