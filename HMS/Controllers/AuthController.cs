﻿using Microsoft.AspNetCore.Mvc;
using HMS.Dto;
using HMS.Models;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Org.BouncyCastle.Crypto.Generators;

namespace HMS.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Check for existing username and email
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            // Validate role-specific ID and email match
            if (dto.Role == "Doctor" && dto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId && d.DoctorEmail == dto.Email);

                if (doctor == null)
                    return BadRequest("Doctor ID and Email do not match");
            }
            else if (dto.Role == "Staff" && dto.StaffId.HasValue)
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.StaffId == dto.StaffId && s.StaffEmail == dto.Email);

                if (staff == null)
                    return BadRequest("Staff ID and Email do not match");
            }
            else
            {
                return BadRequest("Invalid role or missing ID");
            }

            // Create new user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                DoctorId = dto.DoctorId,
                StaffId = dto.StaffId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");
            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            // Log login event
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var loginLog = new UserLoginLog
            {
                UserId = user.UserId,
                Username = user.Username,
                LoginTime = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent
            };
            _context.UserLoginLogs.Add(loginLog);
            await _context.SaveChangesAsync();
            // Save refresh token, etc.
            // ...
            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Role = user.Role,
                DoctorId = user.DoctorId,
                StaffId = user.StaffId
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.Expires <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newAccessToken = _jwtService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old refresh token
            refreshToken.IsRevoked = true;

            // Create new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = refreshToken.UserId
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = refreshToken.User.Username,
                Role = refreshToken.User.Role,
                DoctorId = refreshToken.User.DoctorId,
                StaffId = refreshToken.User.StaffId
            });
        }

        [HttpGet("login-logs")]
        [Authorize(Policy = "StaffOnly")] // Or [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<UserLoginLog>>> GetLoginLogs()
        {
            var logs = await _context.UserLoginLogs
                .OrderByDescending(log => log.LoginTime)
                .Take(100)
                .ToListAsync();

            return Ok(logs);
        }




        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
