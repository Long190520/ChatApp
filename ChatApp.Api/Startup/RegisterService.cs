using ChatApp.Api.Services;

namespace ChatApp.Api.Startup
{
    public static partial class ServiceCollectionExtensions
    {
        public static void RegisterAppService(this IServiceCollection services)
        {
            //others Svc
            services.AddSingleton<ITokenService, TokenService>();
        }
    }
}
