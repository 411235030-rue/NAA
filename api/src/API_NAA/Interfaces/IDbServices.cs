using API_NAA.Dtos.Input.Create;
using API_NAA.Dtos.Input.Query;
using API_NAA.Dtos.Output.Origin;
using ResponseModel;

namespace API_NAA.Interfaces;

public interface IDbServices
{
    Task<ResponseModel<HistoryResponseDto>> SaveHistoryAsync(HistoryCreateDto dto);
    Task<ResponseModel<HistoryResponseDto>> GetHistoryByAccountAsync(HistoryQueryDto dto);
}
