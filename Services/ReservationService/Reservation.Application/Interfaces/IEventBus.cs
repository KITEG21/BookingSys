using System;

namespace Reservation.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class;
}
