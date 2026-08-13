using API_NAA.Dtos.Input.Create;
using API_NAA.Dtos.Input.Query;
using API_NAA.Dtos.Input.Update;
using API_NAA.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_NAA.EndPoints;

public class NaaEndPoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/AuthenticateUser", async ([FromServices] IDbServices dbService, LoginRequestDto dto) =>
        {
            var result = await dbService.AuthenticateUserAsync(dto);
            return Results.Ok(result);
        })
        .WithName("AuthenticateUser");

        app.MapPost("/SaveHistory", async ([FromServices] IDbServices dbService, HistoryCreateDto dto) =>
        {
            var result = await dbService.SaveHistoryAsync(dto);
            return Results.Ok(result);
        })
        .WithName("SaveHistory");

        app.MapPost("/GetConversationSummaries", async ([FromServices] IDbServices dbService, HistoryQueryDto dto) =>
        {
            var result = await dbService.GetConversationSummariesAsync(dto);
            return Results.Ok(result);
        })
        .WithName("GetConversationSummaries");

        app.MapPost("/GetConversationById", async ([FromServices] IDbServices dbService, HistoryQueryDto dto) =>
        {
            var result = await dbService.GetConversationByIdAsync(dto);
            return Results.Ok(result);
        })
        .WithName("GetConversationById");

        app.MapPost("/GetAgentContext", async ([FromServices] IDbServices dbService, HistoryQueryDto dto) =>
        {
            var result = await dbService.GetAgentContextAsync(dto);
            return Results.Ok(result);
        })
        .WithName("GetAgentContext");

        app.MapPost("/SoftDeleteConversation", async ([FromServices] IDbServices dbService, ConversationMutationDto dto) =>
        {
            var result = await dbService.SoftDeleteConversationAsync(dto);
            return Results.Ok(result);
        })
        .WithName("SoftDeleteConversation");

        app.MapPost("/RestoreConversation", async ([FromServices] IDbServices dbService, ConversationMutationDto dto) =>
        {
            var result = await dbService.RestoreConversationAsync(dto);
            return Results.Ok(result);
        })
        .WithName("RestoreConversation");
    }
}
