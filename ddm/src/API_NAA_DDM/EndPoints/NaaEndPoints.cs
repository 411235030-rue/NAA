using API_NAA_DDM.Dtos;
using API_NAA_DDM.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_NAA_DDM.EndPoints;

public class NaaEndPoints : INaaEndPoint
{
    public void NaaEndPoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/SaveHistory", async (INaaHttpServices svc, HistoryCreateDto dto) =>
            Results.Ok(await svc.SaveHistoryAsync(dto)));

        app.MapPost("/GetHistoryByAccount", async (INaaHttpServices svc, HistoryQueryDto dto) =>
            Results.Ok(await svc.GetHistoryByAccountAsync(dto)));

        app.MapPost("/GetUserByAccount", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.GetUserByAccountAsync(dto)));

        app.MapPut("/UpdateUser", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.UpdateUserAsync(dto)));

        app.MapPost("/CreateUser", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.CreateUserAsync(dto)));

        app.MapDelete("/DeleteHistory/{uniqueId}", async ([FromServices] INaaHttpServices svc, string uniqueId) =>
            Results.Ok(await svc.DeleteHistoryAsync(uniqueId)));

        app.MapPost("/ArchiveHistory/{uniqueId}", async ([FromServices] INaaHttpServices svc, string uniqueId) =>
            Results.Ok(await svc.ArchiveHistoryAsync(uniqueId)));

        app.MapPost("/ReviseText", async (ReviseRequestDto dto, INaaHttpServices svc) =>
        {
            var result = await svc.GenerateRevisedTextAsync(dto);
            return string.IsNullOrWhiteSpace(result)
                ? Results.BadRequest("Local text revision failed")
                : Results.Ok(new { revisedText = result });
        })
        .WithName("ReviseText");
    }
}
