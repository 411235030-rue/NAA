using API_NAA_DDM.Dtos;
using API_NAA_DDM.Interfaces;
using API_NAA_DDM.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_NAA_DDM.EndPoints;

public class NaaEndPoints : INaaEndPoint
{
    public void NaaEndPoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/Login", async (INaaHttpServices svc, LoginRequestDto dto) =>
            Results.Ok(await svc.AuthenticateUserAsync(dto)));

        app.MapPost("/SaveHistory", async (INaaHttpServices svc, HistoryCreateDto dto) =>
            Results.Ok(await svc.SaveHistoryAsync(dto)));

        app.MapPost("/GetConversationSummaries", async (INaaHttpServices svc, HistoryQueryDto dto) =>
            Results.Ok(await svc.GetConversationSummariesAsync(dto)));

        app.MapPost("/GetConversationById", async (INaaHttpServices svc, HistoryQueryDto dto) =>
            Results.Ok(await svc.GetConversationByIdAsync(dto)));

        app.MapPost("/GetUserByAccount", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.GetUserByAccountAsync(dto)));

        app.MapPut("/UpdateUser", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.UpdateUserAsync(dto)));

        app.MapPost("/CreateUser", async (INaaHttpServices svc, UserQueryDto dto) =>
            Results.Ok(await svc.CreateUserAsync(dto)));

        app.MapPost("/SoftDeleteConversation", async ([FromServices] INaaHttpServices svc, ConversationMutationDto dto) =>
            Results.Ok(await svc.SoftDeleteConversationAsync(dto)));

        app.MapPost("/RestoreConversation", async ([FromServices] INaaHttpServices svc, ConversationMutationDto dto) =>
            Results.Ok(await svc.RestoreConversationAsync(dto)));

        app.MapPost("/ReviseText", async (ReviseRequestDto dto, INaaHttpServices svc) =>
        {
            try
            {
                var result = await svc.GenerateRevisedTextAsync(dto);
                return string.IsNullOrWhiteSpace(result)
                    ? Results.BadRequest("conversationId, account and inputText are required")
                    : Results.Ok(new { revisedText = result });
            }
            catch (AgentServiceException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithName("ReviseText");
    }
}
