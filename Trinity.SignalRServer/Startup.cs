using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartupAttribute(typeof(Startup))]
class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseCors(CorsOptions.AllowAll);
        app.MapSignalR();
    }
}