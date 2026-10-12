namespace ExcelToXmlApi.DTOs;

public class ConvertRequestDto
{
    public string? CustomFileName { get; set; }
    public string SchemaType { get; set; } = "SOT_PRODUCTION"; // SOT_PRODUCTION | SOT_LPUMA
}

public class ConversionResponseDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string CustomFileName { get; set; } = string.Empty;
    public string SchemaType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int RowCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class HistoryItemDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DisplayFileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string SchemaType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int RowCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
