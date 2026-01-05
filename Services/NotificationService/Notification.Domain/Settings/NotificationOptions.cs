namespace Notification.Domain.Settings;

public class NotificationOptions
{
    public SmtpOptions Smtp { get; set; } = new();
    public TwilioOptions Twilio { get; set; } = new();
}

public class SmtpOptions 
{ 
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}

public class TwilioOptions 
{ 
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromWhatsappNumber { get; set; } = string.Empty;
}
