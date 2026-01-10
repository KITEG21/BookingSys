using System;

namespace Shared.Events;

public record ClientBlocked(
    Guid ClientId,
    string Reason,
    DateTime BlockedAt
);