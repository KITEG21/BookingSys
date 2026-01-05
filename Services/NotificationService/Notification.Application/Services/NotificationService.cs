using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class NotificationService
{
    private readonly INotificationSender _sender;
    private readonly INotificationLogRepository _logRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationSender sender,
        INotificationLogRepository logRepository,
        ILogger<NotificationService> logger)
    {
        _sender = sender;
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task NotifyReservationConfirmedAsync(Guid reservationId, string clientEmail)
    {
        var subject = "Reservation Confirmed";
        var body = $"Your reservation {reservationId} has been confirmed.";

        await SendAndLogAsync("Email", clientEmail, subject, body, "ReservationConfirmed", reservationId);
    }

    public async Task NotifyReservationCancelledAsync(Guid reservationId, string clientEmail)
    {
        var subject = "Reservation Cancelled";
        var body = $"Your reservation {reservationId} has been cancelled.";

        await SendAndLogAsync("Email", clientEmail, subject, body, "ReservationCancelled", reservationId);
    }

    public async Task NotifyPaymentSettledAsync(Guid reservationId, Guid paymentId, string clientEmail)
    {
        var subject = "Payment Received";
        var body = $"Payment {paymentId} for reservation {reservationId} has been processed successfully.";

        await SendAndLogAsync("Email", clientEmail, subject, body, "PaymentSettled", reservationId);
    }

    public async Task NotifyReservationCompletedAsync(Guid reservationId, string clientEmail)
    {
        var subject = "Reservation Completed";
        var body = $"Thank you! Your reservation {reservationId} has been completed. We hope to see you again!";

        await SendAndLogAsync("Email", clientEmail, subject, body, "ReservationCompleted", reservationId);
    }

    public async Task NotifyClientBlockedAsync(Guid clientId, string clientEmail, string reason)
    {
        var subject = "Account Suspended";
        var body = $"Your account has been suspended. Reason: {reason}";

        await SendAndLogAsync("Email", clientEmail, subject, body, "ClientBlocked", clientId);
    }

    private async Task SendAndLogAsync(string channel, string recipient, string subject, string body, string eventType, Guid? relatedEntityId)
    {
        var log = new NotificationLog(channel, recipient, subject, body, eventType, relatedEntityId);

        try
        {
            switch (channel)
            {
                case "Email":
                    await _sender.SendEmailAsync(recipient, subject, body);
                    break;
                case "SMS":
                    await _sender.SendSmsAsync(recipient, body);
                    break;
                case "Push":
                    await _sender.SendPushAsync(recipient, subject, body);
                    break;
            }

            _logger.LogInformation("Notification sent via {Channel} for {EventType}: {Subject}", channel, eventType, subject);
        }
        catch (Exception ex)
        {
            log.MarkFailed();
            _logger.LogError(ex, "Failed to send notification via {Channel} for {EventType}", channel, eventType);
        }

        await _logRepository.AddAsync(log);
    }
}
