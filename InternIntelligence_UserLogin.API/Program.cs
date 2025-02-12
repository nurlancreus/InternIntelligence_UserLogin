
using InternIntelligence_UserLogin.API.Endpoints;

namespace InternIntelligence_UserLogin.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.RegisterServices();

            var app = builder.Build();

            app.AddMiddlewares();

            app.RegisterAuthEndpoints()
               .RegisterUserEndpoints()
               .RegisterRoleEndpoints();

            app.Run();
        }
    }
}
