namespace Nexus.Models;

public enum FileSizeFilter
{
    Any,
    Tiny,
    Small,
    Medium,
    Large,
    Huge
}

public enum DateFilter
{
    Any,
    Today,
    ThisWeek,
    ThisMonth,
    ThisYear,
    Older
}

public enum SortField
{
    Name,
    Size,
    DateModified,
    DateCreated,
    Extension
}

public enum SortDirection
{
    Ascending,
    Descending
}
