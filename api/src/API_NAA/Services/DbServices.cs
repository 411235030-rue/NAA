using API_NAA.Constants;
using API_NAA.Dtos.Input.Create;
using API_NAA.Dtos.Input.Query;
using API_NAA.Dtos.Output.Origin;
using API_NAA.Interfaces;
using Microsoft.Data.SqlClient;
using ResponseModel;
using System.Data;
using static ResponseModel.ResponseMapper;

namespace API_NAA.Services;

public class DbServices : IDbServices
{
    private const string SystemOperator = "NAA_API";
    private readonly string _connectionString;
    private readonly ILogger<DbServices> _logger;

    public DbServices(IConfiguration configuration, ILogger<DbServices> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }

    public async Task<ResponseModel<HistoryResponseDto>> SaveHistoryAsync(HistoryCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Account) ||
            string.IsNullOrWhiteSpace(dto.QuestionText) ||
            string.IsNullOrWhiteSpace(dto.AnswerText) ||
            string.IsNullOrWhiteSpace(dto.OriginCode))
        {
            return GenerateErrorResponse<HistoryResponseDto>(
                "account, questionText, answerText, originCode are required");
        }

        if (string.IsNullOrWhiteSpace(_connectionString))
            return GenerateErrorResponse<HistoryResponseDto>("Database connection is not configured");

        var account = dto.Account.Trim();
        var originCode = dto.OriginCode.Trim();

        if (account.Length > 50)
            return GenerateErrorResponse<HistoryResponseDto>("account cannot exceed 50 characters");

        if (originCode.Length > 50)
            return GenerateErrorResponse<HistoryResponseDto>("originCode cannot exceed 50 characters");

        var questionId = Guid.NewGuid().ToString();
        var answerId = Guid.NewGuid().ToString();
        var conversationId = Guid.NewGuid().ToString();
        var threadId = string.IsNullOrWhiteSpace(dto.ThreadId)
            ? conversationId
            : dto.ThreadId.Trim();

        if (threadId.Length > 36)
            return GenerateErrorResponse<HistoryResponseDto>("threadId cannot exceed 36 characters");

        var now = DateTime.UtcNow;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                var userId = await GetOrCreateUserAsync(connection, transaction, account, now);

                await InsertQuestionAsync(connection, transaction, questionId, dto.QuestionText, originCode, now);
                await InsertAnswerAsync(connection, transaction, answerId, questionId, dto.AnswerText, originCode, now);
                await LinkQuestionToAnswerAsync(connection, transaction, questionId, answerId, now);
                await InsertConversationAsync(
                    connection,
                    transaction,
                    conversationId,
                    threadId,
                    userId,
                    questionId,
                    answerId,
                    dto.ChatTitle,
                    originCode,
                    now);

                await transaction.CommitAsync();

                var history = new HistoryResponseDto
                {
                    UniqueId = conversationId,
                    ThreadId = threadId,
                    Account = account,
                    ChatTitle = dto.ChatTitle,
                    OriginCode = originCode,
                    QuestionText = dto.QuestionText,
                    AnswerText = dto.AnswerText,
                    InsertDt = now
                };

                return new[] { history }.ToResponse(DbConstant.InsertSuccess, DbConstant.InsertError);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to save history for account {Account}", account);
            return GenerateErrorResponse<HistoryResponseDto>("Database save failed");
        }
    }

    public async Task<ResponseModel<HistoryResponseDto>> GetHistoryByAccountAsync(HistoryQueryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Account))
            return GenerateErrorResponse<HistoryResponseDto>("account is required");

        if (string.IsNullOrWhiteSpace(_connectionString))
            return GenerateErrorResponse<HistoryResponseDto>("Database connection is not configured");

        const string sql = """
            SELECT
                c.UNIQUE_ID,
                c.THREAD_ID,
                u.USER_ACCOUNT,
                c.CHAT_TITLE,
                q.QUESTION_TEXT,
                a.ANSWER_TEXT,
                c.ORIGIN_CODE,
                c.INSERT_DT
            FROM dbo.CONVERSATION_RECORDS AS c
            INNER JOIN dbo.USER_ACCOUNT AS u ON u.UNIQUE_ID = c.USER_ID
            INNER JOIN dbo.QUESTIONS_RECORDS AS q ON q.UNIQUE_ID = c.QUESTION_ID
            INNER JOIN dbo.ANSWERS_RECORDS AS a ON a.UNIQUE_ID = c.ANSWER_ID
            WHERE u.USER_ACCOUNT = @Account
              AND (@OriginCode IS NULL OR c.ORIGIN_CODE = @OriginCode)
            ORDER BY c.INSERT_DT DESC;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = dto.Account.Trim();
            command.Parameters.Add("@OriginCode", SqlDbType.VarChar, 50).Value =
                string.IsNullOrWhiteSpace(dto.OriginCode) ? DBNull.Value : dto.OriginCode.Trim();

            var result = new List<HistoryResponseDto>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new HistoryResponseDto
                {
                    UniqueId = reader.GetString(0),
                    ThreadId = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Account = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ChatTitle = reader.IsDBNull(3) ? null : reader.GetString(3),
                    QuestionText = reader.IsDBNull(4) ? null : reader.GetString(4),
                    AnswerText = reader.IsDBNull(5) ? null : reader.GetString(5),
                    OriginCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                    InsertDt = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
                });
            }

            return result.ToResponse(DbConstant.QuerySuccess, DbConstant.QueryNoData);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to query history for account {Account}", dto.Account);
            return GenerateErrorResponse<HistoryResponseDto>("Database query failed");
        }
    }

    private static async Task<string> GetOrCreateUserAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string account,
        DateTime now)
    {
        const string findSql = """
            SELECT TOP (1) UNIQUE_ID
            FROM dbo.USER_ACCOUNT WITH (UPDLOCK, HOLDLOCK)
            WHERE USER_ACCOUNT = @Account
            ORDER BY INSERT_DT;
            """;

        await using var findCommand = new SqlCommand(findSql, connection, transaction);
        findCommand.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = account;
        var existingId = await findCommand.ExecuteScalarAsync();

        if (existingId is string userId)
            return userId;

        var newUserId = Guid.NewGuid().ToString();
        const string insertSql = """
            INSERT INTO dbo.USER_ACCOUNT
                (UNIQUE_ID, INSERT_DT, INSERT_OP, USER_ACCOUNT)
            VALUES
                (@UniqueId, @InsertDt, @InsertOp, @Account);
            """;

        await using var insertCommand = new SqlCommand(insertSql, connection, transaction);
        insertCommand.Parameters.Add("@UniqueId", SqlDbType.VarChar, 36).Value = newUserId;
        insertCommand.Parameters.Add("@InsertDt", SqlDbType.DateTime2).Value = now;
        insertCommand.Parameters.Add("@InsertOp", SqlDbType.VarChar, 10).Value = SystemOperator;
        insertCommand.Parameters.Add("@Account", SqlDbType.VarChar, 50).Value = account;
        await insertCommand.ExecuteNonQueryAsync();
        return newUserId;
    }

    private static async Task InsertQuestionAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string questionId,
        string questionText,
        string originCode,
        DateTime now)
    {
        const string sql = """
            INSERT INTO dbo.QUESTIONS_RECORDS
                (UNIQUE_ID, QUESTION_TEXT, INSERT_OP, INSERT_DT, ORIGIN_CODE)
            VALUES
                (@UniqueId, @QuestionText, @InsertOp, @InsertDt, @OriginCode);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@UniqueId", SqlDbType.VarChar, 36).Value = questionId;
        command.Parameters.Add("@QuestionText", SqlDbType.NVarChar, -1).Value = questionText;
        command.Parameters.Add("@InsertOp", SqlDbType.VarChar, 10).Value = SystemOperator;
        command.Parameters.Add("@InsertDt", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@OriginCode", SqlDbType.VarChar, 50).Value = originCode;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertAnswerAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string answerId,
        string questionId,
        string answerText,
        string originCode,
        DateTime now)
    {
        const string sql = """
            INSERT INTO dbo.ANSWERS_RECORDS
                (UNIQUE_ID, QUESTION_ID, ANSWER_TEXT, INSERT_OP, INSERT_DT, ORIGIN_CODE)
            VALUES
                (@UniqueId, @QuestionId, @AnswerText, @InsertOp, @InsertDt, @OriginCode);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@UniqueId", SqlDbType.VarChar, 36).Value = answerId;
        command.Parameters.Add("@QuestionId", SqlDbType.VarChar, 36).Value = questionId;
        command.Parameters.Add("@AnswerText", SqlDbType.NVarChar, -1).Value = answerText;
        command.Parameters.Add("@InsertOp", SqlDbType.VarChar, 10).Value = SystemOperator;
        command.Parameters.Add("@InsertDt", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@OriginCode", SqlDbType.VarChar, 50).Value = originCode;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task LinkQuestionToAnswerAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string questionId,
        string answerId,
        DateTime now)
    {
        const string sql = """
            UPDATE dbo.QUESTIONS_RECORDS
            SET ANSWER_ID = @AnswerId,
                UPDATE_OP = @UpdateOp,
                UPDATE_DT = @UpdateDt
            WHERE UNIQUE_ID = @QuestionId;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@AnswerId", SqlDbType.VarChar, 36).Value = answerId;
        command.Parameters.Add("@UpdateOp", SqlDbType.VarChar, 10).Value = SystemOperator;
        command.Parameters.Add("@UpdateDt", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@QuestionId", SqlDbType.VarChar, 36).Value = questionId;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertConversationAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string conversationId,
        string threadId,
        string userId,
        string questionId,
        string answerId,
        string? chatTitle,
        string originCode,
        DateTime now)
    {
        const string sql = """
            INSERT INTO dbo.CONVERSATION_RECORDS
                (UNIQUE_ID, THREAD_ID, USER_ID, QUESTION_ID, ANSWER_ID, CHAT_TITLE, INSERT_OP, INSERT_DT, ORIGIN_CODE)
            VALUES
                (@UniqueId, @ThreadId, @UserId, @QuestionId, @AnswerId, @ChatTitle, @InsertOp, @InsertDt, @OriginCode);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@UniqueId", SqlDbType.VarChar, 36).Value = conversationId;
        command.Parameters.Add("@ThreadId", SqlDbType.VarChar, 36).Value = threadId;
        command.Parameters.Add("@UserId", SqlDbType.VarChar, 36).Value = userId;
        command.Parameters.Add("@QuestionId", SqlDbType.VarChar, 36).Value = questionId;
        command.Parameters.Add("@AnswerId", SqlDbType.VarChar, 36).Value = answerId;
        command.Parameters.Add("@ChatTitle", SqlDbType.NVarChar, 255).Value =
            string.IsNullOrWhiteSpace(chatTitle) ? DBNull.Value : chatTitle[..Math.Min(chatTitle.Length, 255)];
        command.Parameters.Add("@InsertOp", SqlDbType.VarChar, 10).Value = SystemOperator;
        command.Parameters.Add("@InsertDt", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@OriginCode", SqlDbType.VarChar, 50).Value = originCode;
        await command.ExecuteNonQueryAsync();
    }
}
