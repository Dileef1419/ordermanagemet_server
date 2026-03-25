namespace SharedKernel;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record CursorResult<T>(
    IReadOnlyList<T> Items,
    DateTimeOffset? NextCursor,
    bool HasMore);
