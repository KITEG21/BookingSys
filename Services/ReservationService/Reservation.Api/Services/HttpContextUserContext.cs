using System;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Reservation.Application.Interfaces;

namespace Reservation.Api.Services;

public class HttpContextUserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;
    private readonly HttpClient _httpClient;

    public HttpContextUserContext(IHttpContextAccessor accessor, HttpClient httpClient)
    {
        _accessor = accessor;
        _httpClient = httpClient;
    }

    public string? GetCurrentUserEmail()
    {
        var user = _accessor.HttpContext?.User;
        // Check both standard claim types - JWT uses "email", ClaimTypes uses the full URI
        return user?.FindFirst(ClaimTypes.Email)?.Value 
            ?? user?.FindFirst("email")?.Value;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/auth/users/{userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return user?.Email;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public Guid? GetUserId()
    {
        var user = _accessor.HttpContext?.User;
        var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(id, out var gid) ? gid : (Guid?)null;
    }

    private record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role, DateTime CreatedAt, DateTime? LastLoginAt);

}