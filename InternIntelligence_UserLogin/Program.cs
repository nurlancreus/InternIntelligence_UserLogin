
using InternIntelligence_UserLogin.Endpoints;

namespace InternIntelligence_UserLogin
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
               .RegisterUserEndpoints();

            app.Run();
        }
    }
}
