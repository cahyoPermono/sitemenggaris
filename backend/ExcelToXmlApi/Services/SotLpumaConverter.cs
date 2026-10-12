using System.Data;
using System.Text;
using System.Xml.Linq;
using ExcelDataReader;
using ExcelToXmlApi.Models;

namespace ExcelToXmlApi.Services;

public class SotLpumaConverter : ISotLpumaConverter
{
    public (string xmlContent, int rowCount) Convert(Stream excelStream, string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var rows = ParseExcelRows(excelStream);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("File Excel tidak memiliki data atau format tidak dikenali.");
        }

        var xmlString = GenerateLpumaXml(rows);
        return (xmlString, rows.Count);
    }

    private List<SotLpumaRow> ParseExcelRows(Stream stream)
    {
        var list = new List<SotLpumaRow>();

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = false
            }
        });

        if (dataSet.Tables.Count == 0) return list;

        var table = dataSet.Tables[0];
        if (table.Rows.Count < 2) return list;

        int headerRowIndex = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Scan first 5 rows to identify header
        for (int r = 0; r < Math.Min(5, table.Rows.Count); r++)
        {
            var tempMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var val = table.Rows[r][c]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    var normalized = CleanHeader(val);
                    tempMap[normalized] = c;
                }
            }

            if (tempMap.ContainsKey("kkks") || tempMap.ContainsKey("wilayahkerja") || tempMap.ContainsKey("wk") || tempMap.ContainsKey("eventid") || tempMap.ContainsKey("kategori"))
            {
                headerRowIndex = r;
                colMap = tempMap;
                break;
            }
        }

        if (headerRowIndex == -1)
        {
            headerRowIndex = 0;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var val = table.Rows[0][c]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    colMap[CleanHeader(val)] = c;
                }
            }
        }

        string CleanHeader(string raw)
        {
            return raw.Replace(" ", "")
                      .Replace("_", "")
                      .Replace("[", "")
                      .Replace("]", "")
                      .Replace("/", "")
                      .Replace("-", "")
                      .Replace(".", "")
                      .ToLowerInvariant();
        }

        string GetColValue(DataRow row, params string[] candidateKeys)
        {
            foreach (var key in candidateKeys)
            {
                var norm = CleanHeader(key);
                if (colMap.TryGetValue(norm, out int colIdx))
                {
                    var obj = row[colIdx];
                    if (obj != null && obj != DBNull.Value)
                    {
                        if (obj is DateTime dt)
                        {
                            return dt.ToString("yyyy-MM-dd");
                        }
                        return obj.ToString()?.Trim() ?? string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        for (int r = headerRowIndex + 1; r < table.Rows.Count; r++)
        {
            var dr = table.Rows[r];

            bool allEmpty = true;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                if (!string.IsNullOrWhiteSpace(dr[c]?.ToString()))
                {
                    allEmpty = false;
                    break;
                }
            }
            if (allEmpty) continue;

            var item = new SotLpumaRow
            {
                DocumentName = GetColValue(dr, "documentName", "bysistemkkks", "document_name"),
                DocumentDate = GetColValue(dr, "documentDate", "docdate"),
                Operator = GetColValue(dr, "KKKS", "operator"),
                WilayahKerja = GetColValue(dr, "WK", "wilayahKerja"),
                FacilityKind = GetColValue(dr, "Field/Area/Train/Platform/All", "facilityKind"),
                FacilityName = GetColValue(dr, "Field/Area/Train/Platform/All Name", "facilityName"),
                EventDate = GetColValue(dr, "Tanggal", "eventDate", "date"),
                Days = GetColValue(dr, "Hari ke-", "days", "hari"),
                StartTime = GetColValue(dr, "Start Time", "startTime"),
                EndTime = GetColValue(dr, "End Time", "endTime"),
                ShutdownId = GetColValue(dr, "eventID", "shutdownID"),
                Category = GetColValue(dr, "Kategori", "category"),
                SystemSourceLevel1 = GetColValue(dr, "System Source Level 1", "systemSourceLevel1"),
                SystemSourceLevel2 = GetColValue(dr, "System Source Level 2", "systemSourceLevel2"),
                SystemCauseLevel1 = GetColValue(dr, "System Cause Level1", "systemCauseLevel1"),
                SystemCauseLevel2 = GetColValue(dr, "System Cause Level2", "systemCauseLevel2"),
                EventsActivities = GetColValue(dr, "Kejadian/Kegiatan", "eventsActivities", "activities"),
                UpdateProgress = GetColValue(dr, "Update Progress", "updateProgress"),
                TotalWellOff = GetColValue(dr, "Jumlah Sumur OFF", "totalWellOFF", "totalWellOff"),
                EstFinishTime = GetColValue(dr, "Est. Tanggal Selesai", "estFinishTime"),
                VolumeOil = GetColValue(dr, "Volume LPO Minyak (BBLS)", "volumeOil", "volumeOilBbls"),
                VolumeGas = GetColValue(dr, "Volume LPO Gas (MMSCF)", "volumeGas", "volumeGasMmscf")
            };

            list.Add(item);
        }

        return list;
    }

    private string GenerateLpumaXml(List<SotLpumaRow> rows)
    {
        var first = rows.First();

        var root = new XElement("lossProductionOpportunity",
            new XElement("documentName", string.IsNullOrEmpty(first.DocumentName) ? "LPO_" + DateTime.UtcNow.ToString("yyyyMMdd") : first.DocumentName),
            new XElement("documentDate", string.IsNullOrEmpty(first.DocumentDate) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : first.DocumentDate),
            new XElement("operator", first.Operator),
            new XElement("wilayahKerja", first.WilayahKerja)
        );

        var eventsElem = new XElement("events");

        // Group by EventDate
        var dateGroups = rows.GroupBy(r => string.IsNullOrEmpty(r.EventDate) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : r.EventDate);

        foreach (var group in dateGroups)
        {
            var eventDateElem = new XElement("eventDate",
                new XAttribute("date", group.Key));

            foreach (var r in group)
            {
                var eventElem = new XElement("event",
                    new XElement("facility",
                        new XElement("facilityKind", r.FacilityKind),
                        new XElement("facilityName", r.FacilityName),
                        new XElement("days", r.Days),
                        new XElement("startTime", r.StartTime),
                        new XElement("endTime", r.EndTime),
                        new XElement("shutdownID", r.ShutdownId),
                        new XElement("category", r.Category),
                        new XElement("systemSourceLevel1", r.SystemSourceLevel1),
                        new XElement("systemSourceLevel2", r.SystemSourceLevel2),
                        new XElement("systemCauseLevel1", r.SystemCauseLevel1),
                        new XElement("systemCauseLevel2", r.SystemCauseLevel2),
                        new XElement("eventsActivities", r.EventsActivities),
                        new XElement("updateProgress", r.UpdateProgress),
                        new XElement("totalWellOFF", r.TotalWellOff),
                        new XElement("estFinishTime", r.EstFinishTime),
                        new XElement("volumeOil", r.VolumeOil),
                        new XElement("volumeGas", r.VolumeGas)
                    )
                );

                eventDateElem.Add(eventElem);
            }

            eventsElem.Add(eventDateElem);
        }

        root.Add(eventsElem);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            root
        );

        return doc.Declaration?.ToString() + Environment.NewLine + doc.ToString();
    }
}
