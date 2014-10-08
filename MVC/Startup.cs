//using Owin;

//namespace MVC.Security
//{
//    public partial class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            ConfigureAuth(app);
//        }
//    }
//}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("TestingConfiguration", typeof(MVC.Security.Startup))]

namespace MVC.Security
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
