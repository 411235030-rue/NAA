namespace ResponseModel;

public class ResponseModel<T>
{
    public int Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<T> Results { get; set; } = new();
}

public static class ResponseMapper
{
    public static ResponseModel<T> ToResponse<T>(
        this IEnumerable<T> source,
        string successDescription,
        string emptyDescription)
    {
        var results = source.ToList();

        return new ResponseModel<T>
        {
            Status = results.Count > 0 ? 1 : 0,
            Description = results.Count > 0 ? successDescription : emptyDescription,
            Message = results.Count > 0 ? successDescription : emptyDescription,
            Results = results
        };
    }

    public static ResponseModel<T> GenerateErrorResponse<T>(string description)
    {
        return new ResponseModel<T>
        {
            Status = 0,
            Description = description,
            Message = description,
            Results = new List<T>()
        };
    }
}
