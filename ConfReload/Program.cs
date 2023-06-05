namespace ConfReload;

using System.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Hosting;
using Owin;
using Winton.Extensions.Configuration.Consul;

public class Profile
{
    public string? Name { get; set; }
}

public class HomeController : ApiController
{
    private readonly Profile _profile;

    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options#use-ioptionssnapshot-to-read-updated-data
    public HomeController(IOptionsSnapshot<Profile> options) =>
        _profile = options.Value;

    public string Get() =>
        _profile.Name ?? "Unknown name";
}

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var services = new ServiceCollection();

        var conf = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            // https://github.com/wintoncode/Winton.Extensions.Configuration.Consul
            .AddConsul("shared", ConfigureConsul)
            .AddConsul("my-app", ConfigureConsul)
            .Build();

        services.AddSingleton<IConfiguration>(conf);
        services.Configure<Profile>(conf.GetSection("Profile").Bind);

        services.AddTransient<HomeController>();

        var config = new HttpConfiguration
        {
            DependencyResolver = new DefaultDependencyResolver(services.BuildServiceProvider())
        };

        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "{controller}/{id}",
            defaults: new { controller = "Home", id = RouteParameter.Optional }
        );

        app.UseWebApi(config);
    }

    private static void ConfigureConsul(IConsulConfigurationSource o)
    {
        o.Optional = true;
        o.ReloadOnChange = true;
        o.OnLoadException = c =>
        {
            c.Ignore = true;
            Console.Error.WriteLine(c.Exception.ToString());
        };
    }
}

public static class Program
{
    static void Main()
    {
        using (WebApp.Start<Startup>(url: "http://localhost:5000/"))
        {
            Console.ReadLine();
        }
    }
}
