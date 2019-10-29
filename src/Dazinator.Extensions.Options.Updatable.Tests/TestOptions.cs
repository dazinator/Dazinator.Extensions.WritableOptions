namespace Dazinator.Extensions.Options.Updatable.Tests
{
    public class TestOptions
    {
        public bool Enabled { get; set; } = false;
        public int? SomeInt { get; set; }
        public decimal? SomeDecimal { get; set; }
    }

    public enum PlatformSetupStatus
    {
        SetupDatabase,
        ConfigureSmtp,
        ConfigureTenant,
        AwaitingTenantAdminConfirmation,
        SetupComplete
    }

    public class DatabaseConnectionOptionsDto
    {
        public string ConnectionString { get; set; }

        public string Provider { get; set; }
    }

    public class SmtpOptionsDto
    {

        public SmtpOptionsDto()
        {

        }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string FromName { get; set; }
        public string FromEmailAddress { get; set; } // sender email address for the system
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RequiresAuthentication { get; set; }

    }

    public class DefaultTenantSetupOptionsDto
    {
        public string Guid { get; set; }

        public string Code { get; set; }
        public int Id { get; set; }

        public string TenantName { get; set; }

        public string Email { get; set; }

        public bool IsCurrent { get; set; }

    }

    public class PlatformSetupOptionsDto
    {
        public PlatformSetupOptionsDto()
        {
            Database = new DatabaseConnectionOptionsDto();
            Smtp = new SmtpOptionsDto();
            Tenant = new DefaultTenantSetupOptionsDto();
            SetupStatus = PlatformSetupStatus.SetupDatabase;
        }
        public bool SetupComplete { get; set; }

        public PlatformSetupStatus SetupStatus { get; set; }

        public DatabaseConnectionOptionsDto Database { get; set; }

        public SmtpOptionsDto Smtp { get; set; }

        public DefaultTenantSetupOptionsDto Tenant { get; set; }
    }
}
