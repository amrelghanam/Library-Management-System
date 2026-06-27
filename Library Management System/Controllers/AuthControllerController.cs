using Library_Management_System.Applications.Dtos;
using Library_Management_System.Applications.Interfaces;
using Library_Management_System.Infrastructure.Data;
using Library_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly LibraryDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public AuthController(
        LibraryDbContext context, IActivityLogService activityLogService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _activityLogService = activityLogService;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest("Invalid Role");

            await _userManager.AddToRoleAsync(user, dto.Role);

            await _activityLogService.LogActivity(user.Id, "New User Registered");

            await transaction.CommitAsync();

            return Ok("User registered successfully");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // LOGIN 
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.UserName,
            dto.Password,
            false,
            false);

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user !=null)
        {
            await _activityLogService.LogActivity(user.Id, "User Logged In");

        }

        return Ok("Login successful");
    }

    // LOGOUT
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user !=null)
        {
            await _activityLogService.LogActivity(user.Id, "User Logged Out");

        }

        await _signInManager.SignOutAsync();
        return Ok("Logged out successfully");
    }
}