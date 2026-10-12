namespace ExcelToXmlApi.Models;

public class ConversionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string OriginalFileName { get; set; } = string.Empty;
    public string CustomFileName { get; set; } = string.Empty;
    public string SchemaType { get; set; } = "SOT_PRODUCTION"; // SOT_PRODUCTION | SOT_LPUMA
    public string Status { get; set; } = "Berhasil"; // Berhasil | Gagal
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public string XmlContent { get; set; } = string.Empty;
    public int RowCount { get; set; }
}
