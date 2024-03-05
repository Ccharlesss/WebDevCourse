using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
public class EmailService
{
    private readonly EmailSettings _emailSettings;

    // Constructor initializes an instance of the EmailService
    // IOptions<EmailSettings parameter allows to retrieve email settings from the WebApp configuration.
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    // Purpose: Send the email to the address
    public void SendEmail(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Support CareApp", _emailSettings.SmtpUsername));
        message.To.Add(new MailboxAddress("Reciever Name", toEmail));
        message.Subject = subject;
        
        // Creates a TextPart object to represent the body of the email and sets its content to the provided body
        var textPart = new TextPart("plain")
        {
            Text = body
        };

        // Assigns the TextPart object to the body property of the MimeMessage
        message.Body = textPart;

        using (var client = new SmtpClient())
        {
            // Establishes a connection to the SMTP server using a SMTP Client
            client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            // Authenticate the Webapp with the SMTP server to ensure that only authorized users can send emails through it
            client.Authenticate(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            client.Send(message); // Sends the email to the SMTP server for delvery
            client.Disconnect(true); // Disconnect the connection w/ SMTP server


        }
         


    }


}