# Email Service Setup

## Overview
The email service sends notifications when certain events occur in the application, such as when a new SaveChickenRequest is created.

## Configuration

### Environment Variables
The email service is configured via environment variables for security and flexibility. Use ASP.NET Core's double underscore notation:

```bash
Email__SmtpHost=mailhog                              # SMTP server hostname
Email__SmtpPort=1025                                 # SMTP server port
Email__FromEmail=noreply@savethechicken.local        # Sender email address
Email__FromName=Save The Chicken Dev                 # Sender display name
Email__Username=                                     # SMTP authentication username (optional)
Email__Password=                                     # SMTP authentication password (optional)
Email__EnableSsl=false                               # Enable SSL/TLS (true/false)
Email__NotificationRecipients__0=admin@example.com   # First recipient
Email__NotificationRecipients__1=admin2@example.com  # Second recipient (optional)
```

These are set in [`infrastructure/development/.env`](../../infrastructure/development/.env) and automatically loaded by the webapi container.

### Development Environment
In development, we use **MailHog** - a local email testing tool that captures all sent emails and displays them in a web interface.

**Access MailHog Web UI:** http://localhost:8025

The development configuration in `appsettings.Development.json`:
```json
{
  "Email": {
    "SmtpHost": "mailhog",
    "SmtpPort": "1025",
    "FromEmail": "noreply@savethechicken.local",
    "FromName": "Save The Chicken Dev",
    "Username": "",
    "Password": "",
    "EnableSsl": false,
    "NotificationRecipients": [
      "admin@savethechicken.local"
    ]
  }
}
```

### Production Environment
Set these environment variables in your production environment:

```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__FromEmail=your-email@gmail.com
Email__FromName=Save The Chicken
Email__Username=your-email@gmail.com
Email__Password=your-app-password
Email__EnableSsl=true
Email__NotificationRecipients__0=admin1@example.com
Email__NotificationRecipients__1=admin2@example.com
```

**Note:** For multiple notification recipients, use indexed notation: `__0`, `__1`, `__2`, etc.

### Common SMTP Providers

#### Gmail
- SMTP Host: `smtp.gmail.com`
- SMTP Port: `587`
- Enable SSL: `true`
- Note: Use App Password if 2FA is enabled

#### Office 365 / Outlook
- SMTP Host: `smtp.office365.com`
- SMTP Port: `587`
- Enable SSL: `true`

#### SendGrid
- SMTP Host: `smtp.sendgrid.net`
- SMTP Port: `587`
- Username: `apikey`
- Password: Your SendGrid API Key
- Enable SSL: `true`

## Usage

### Send Email from Controller
```csharp
public class MyController : ControllerBase
{
    private readonly IEmailService _emailService;

    public MyController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSomething(MyDto dto)
    {
        // Your logic here

        // Send notification email
        await _emailService.SendEmailAsync(
            to: "recipient@example.com",
            subject: "Something was created",
            body: "<h1>Hello</h1><p>Something happened!</p>",
            isHtml: true
        );

        return Ok();
    }
}
```

### Send to Multiple Recipients
```csharp
var recipients = new List<string> { "admin1@example.com", "admin2@example.com" };
await _emailService.SendEmailAsync(recipients, "Subject", "Body");
```

## Current Implementations

### SaveChickenRequest Creation
When a new SaveChickenRequest is created, an email notification is sent to all configured recipients with details including:
- Request ID
- Contact information (name, email, phone)
- Address
- Number of chickens
- Assigned SaveChickenAction (if any)

Location: [`SaveChickenRequestController.cs`](../Controllers/ControllersImpl/SaveChickenRequestController.cs)

## Testing

1. Start the development environment:
   ```bash
   cd infrastructure/development
   docker compose up --build
   ```

2. Create a new SaveChickenRequest through the API or UI

3. Open MailHog web interface: http://localhost:8025

4. You should see the notification email in the MailHog inbox

## Error Handling
- Email failures are logged but don't cause the API request to fail
- Check application logs if emails aren't being sent
- Verify SMTP configuration in appsettings files

## Adding More Email Notifications

To add email notifications for other events:

1. Inject `IEmailService` into your controller
2. Call `_emailService.SendEmailAsync()` after the event occurs
3. Wrap in try-catch to prevent email failures from breaking your API
4. Log any exceptions

Example:
```csharp
try
{
    await _emailService.SendEmailAsync(recipients, subject, body);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to send notification email");
}
```
