using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebProducer.Startup))]

namespace WebProducer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}