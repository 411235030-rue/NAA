using API_NAA.Dtos.Input.Create;
using API_NAA.Dtos.Input.Query;
using API_NAA.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_NAA.EndPoints;

public class NaaEndPoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/SaveHistory", async ([FromServices] IDbServices dbService, HistoryCreateDto dto) =>
        {
            var result = await dbService.SaveHistoryAsync(dto);
            return Results.Ok(result);
        })
        .WithName("SaveHistory");

        app.MapPost("/GetHistoryByAccount", async ([FromServices] IDbServices dbService, HistoryQueryDto dto) =>
        {
            var result = await dbService.GetHistoryByAccountAsync(dto);
            return Results.Ok(result);
        })
        .WithName("GetHistoryByAccount");
    }
}
