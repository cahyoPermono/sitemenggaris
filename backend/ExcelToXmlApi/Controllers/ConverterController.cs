using System.Text;
using ExcelToXmlApi.Data;
using ExcelToXmlApi.DTOs;
using ExcelToXmlApi.Models;
using ExcelToXmlApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExcelToXmlApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConverterController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ISotProductionConverter _sotProductionConverter;
    private readonly ISotLpumaConverter _sotLpumaConverter;
    private readonly TemplateGeneratorService _templateService;
    private readonly ILogger<ConverterController> _logger;

    public ConverterController(
        AppDbContext dbContext,
        ISotProductionConverter sotProductionConverter,
        ISotLpumaConverter sotLpumaConverter,
        TemplateGeneratorService templateService,
        ILogger<ConverterController> logger)
    {
        _dbContext = dbContext;
        _sotProductionConverter = sotProductionConverter;
        _sotLpumaConverter = sotLpumaConverter;
        _templateService = templateService;
        _logger = logger;
    }

    [HttpPost("convert")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> ConvertExcel(
        [FromForm] IFormFile file,
        [FromForm] string? schemaType = "SOT_PRODUCTION",
        [FromForm] string? customFileName = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File tidak boleh kosong." });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
        {
            return BadRequest(new { message = "Format file tidak didukung. Harap unggah file .xlsx atau .xls." });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "Ukuran file melebihi batas maksimal 10 MB." });
        }

        var isLpuma = string.Equals(schemaType, "SOT_LPUMA", StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(schemaType, "LPUMA", StringComparison.OrdinalIgnoreCase);

        var record = new ConversionRecord
        {
            OriginalFileName = file.FileName,
            CustomFileName = string.IsNullOrWhiteSpace(customFileName) ? Path.GetFileNameWithoutExtension(file.FileName) : customFileName.Trim(),
            SchemaType = isLpuma ? "SOT_LPUMA" : "SOT_PRODUCTION",
            FileSizeBytes = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        var warnings = new List<string>();

        try
        {
            using var stream = file.OpenReadStream();
            string xml;
            int count;

            if (isLpuma)
            {
                (xml, count, warnings) = _sotLpumaConverter.Convert(stream, file.FileName);
            }
            else
            {
                (xml, count, warnings) = _sotProductionConverter.Convert(stream, file.FileName);
            }

            record.Status = "Berhasil";
            record.XmlContent = xml;
            record.RowCount = count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion error for file {FileName}", file.FileName);
            record.Status = "Gagal";
            record.ErrorMessage = ex.Message;
            record.XmlContent = string.Empty;
            record.RowCount = 0;
        }

        _dbContext.ConversionRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var response = new ConversionResponseDto
        {
            Id = record.Id,
            OriginalFileName = record.OriginalFileName,
            CustomFileName = record.CustomFileName,
            SchemaType = record.SchemaType,
            Status = record.Status,
            FileSizeBytes = record.FileSizeBytes,
            RowCount = record.RowCount,
            CreatedAt = record.CreatedAt,
            XmlContent = record.XmlContent,
            ErrorMessage = record.ErrorMessage,
            Warnings = warnings,
            DownloadUrl = $"/api/converter/download/{record.Id}"
        };

        if (record.Status == "Gagal")
        {
            return UnprocessableEntity(response);
        }

        return Ok(response);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _dbContext.ConversionRecords.AsQueryable();

        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsedDate))
        {
            var startUtc = parsedDate.Date;
            var endUtc = startUtc.AddDays(1);
            query = query.Where(r => r.CreatedAt >= startUtc && r.CreatedAt < endUtc);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new HistoryItemDto
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                DisplayFileName = string.IsNullOrWhiteSpace(r.CustomFileName) ? r.OriginalFileName : $"{r.CustomFileName}.xml",
                OriginalFileName = r.OriginalFileName,
                SchemaType = r.SchemaType,
                Status = r.Status,
                FileSizeBytes = r.FileSizeBytes,
                RowCount = r.RowCount,
                ErrorMessage = r.ErrorMessage,
                DownloadUrl = $"/api/converter/download/{r.Id}"
            })
            .ToListAsync();

        return Ok(new PaginatedList<HistoryItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("history/{id}")]
    public async Task<IActionResult> GetHistoryDetail(Guid id)
    {
        var record = await _dbContext.ConversionRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound(new { message = "Data riwayat tidak ditemukan." });
        }

        return Ok(new ConversionResponseDto
        {
            Id = record.Id,
            OriginalFileName = record.OriginalFileName,
            CustomFileName = record.CustomFileName,
            SchemaType = record.SchemaType,
            Status = record.Status,
            FileSizeBytes = record.FileSizeBytes,
            RowCount = record.RowCount,
            CreatedAt = record.CreatedAt,
            XmlContent = record.XmlContent,
            ErrorMessage = record.ErrorMessage,
            DownloadUrl = $"/api/converter/download/{record.Id}"
        });
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadXml(Guid id)
    {
        var record = await _dbContext.ConversionRecords.FindAsync(id);
        if (record == null || string.IsNullOrEmpty(record.XmlContent))
        {
            return NotFound(new { message = "File XML tidak ditemukan." });
        }

        var fileName = string.IsNullOrWhiteSpace(record.CustomFileName)
            ? $"{Path.GetFileNameWithoutExtension(record.OriginalFileName)}.xml"
            : (record.CustomFileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                ? record.CustomFileName
                : $"{record.CustomFileName}.xml");

        var bytes = Encoding.UTF8.GetBytes(record.XmlContent);
        return File(bytes, "application/xml", fileName);
    }

    [HttpGet("template/{type}")]
    public IActionResult DownloadTemplate(string type)
    {
        if (string.Equals(type, "sot-lpuma", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(type, "lpuma", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _templateService.GenerateSotLpumaTemplate();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_SOT_LPUMA.xlsx");
        }

        var prodBytes = _templateService.GenerateSotProductionTemplate();
        return File(prodBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_SOT_Production.xlsx");
    }
}
