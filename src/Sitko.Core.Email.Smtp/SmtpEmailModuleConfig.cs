using FluentValidation;

namespace Sitko.Core.Email.Smtp;

public class SmtpEmailModuleOptions : FluentEmailModuleOptions
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}

public class SmtpEmailModuleOptionsValidator : FluentEmailModuleOptionsValidator<SmtpEmailModuleOptions>
{
    public SmtpEmailModuleOptionsValidator()
    {
        RuleFor(o => o.Server).NotEmpty().WithMessage("Provide smtp server");
        RuleFor(o => o.Port).GreaterThan(0).WithMessage("Provide smtp port");
    }
}

