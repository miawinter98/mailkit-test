using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World");
app.MapGet(
    "/send",
    async (CancellationToken cancellationToken) =>
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Example Server", "server@example.invalid"));
		message.To.Add(new MailboxAddress("some user", "user@example.invalid"));
		message.Subject = "Test Subject";
		message.Body = new BodyBuilder
        {
            HtmlBody = "<p><em>Test Message</em></p>", 
            TextBody = "Test Message"
        }.ToMessageBody();

        message.Headers.Add(
            HeaderId.ListUnsubscribe,
            "<https://example.invalid/unsubscribe?email=user%40example.invalid&token=sometoken>"
        );
        message.Headers.Add(HeaderId.ListUnsubscribePost, "List-Unsubscribe=One-Click");

		using var client = new SmtpClient(new ProtocolLogger("smtp.log"));
		await client.ConnectAsync("127.0.0.1", 2525, false, cancellationToken);

		await client.SendAsync(message, cancellationToken);
		await client.DisconnectAsync(true, cancellationToken);

        return "OK";
    }
);

app.Run();
