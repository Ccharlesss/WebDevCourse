
public class EmailSettings
{   // This class holds configuration settings related to Emails

    public string SmtpServer { get; set; } // Address of SMTP server

    public int SmtpPort { get; set; } // Port nbr of SMTP server for connections

    public string SmtpUsername { get; set; } // Username or Email address for authenticating w/ SMTP server

    public string SmtpPassword { get; set; } // Password associated w/ Email address for authenticating w/ SMTP server
}