using API_NAA_DDM.Interfaces;

namespace API_NAA_DDM.Extensions;

public static class EndpointExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.ServiceProvider.GetServices<INaaEndPoint>();
        foreach (var endpoint in endpoints)
        {
            endpoint.NaaEndPoint(app);
        }
    }
}
