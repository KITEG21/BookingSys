using System;

namespace Reservation.Application.Interfaces;

public interface IUserContext
{
    Guid? GetUserId();
    Task<string?> GetUserEmailAsync(Guid userId);
    string? GetCurrentUserEmail();
}
