# LiteCrm.OAuth

OAuth client for Lite CRM.

## Usage

```
public static class Program
{
    public static Task Main()
    {
        var configuration = Configuration.GetConfiguration();

        return configuration
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSomeAuthentication()
                    .AddLiteCrmOAuth(configuration)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
            })
            .Configure((context, builder) =>
            {
                builder
                    .UseAuthentication()
            })
            .Build()
            .RunAsync();
    }
}

```
