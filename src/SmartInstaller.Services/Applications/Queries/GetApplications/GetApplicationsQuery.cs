namespace SmartInstaller.Services.Applications.Queries.GetApplications;

public sealed record GetApplicationsQuery
{
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public string? Search { get; init; }

    public string? Category { get; init; }

    public string? Platform { get; init; }

    public string? Tag { get; init; }

    public bool? Featured { get; init; }

    public string SortBy { get; init; } = "name";

    public string SortDirection { get; init; } = "asc";

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(
            value,
            1,
            MaximumPageSize);
    }
}