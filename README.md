# LiteCrm.OAuth

OAuth client for Lite CRM.

## Usage
1. Add nuget source: `nuget sources add -name GPR -Source https://nuget.pkg.github.com/ajupov`
2. Install package: `nuget install LiteCrm.OAuth`

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

## Development
1. Clone this repository
2. Switch to a `new branch`
3. Make changes into `new branch`
4. Upgrade `PackageVersion` property value in `.csproj` file
5. Create pull request from `new branch` to `master` branch
6. Require code review
7. Merge pull request after approving
8. You can see package in [Github Packages](https://github.com/ajupov/LiteCrm.OAuth/packages)
