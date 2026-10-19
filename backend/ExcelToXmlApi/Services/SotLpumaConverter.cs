using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using ExcelDataReader;
using ExcelToXmlApi.Models;

namespace ExcelToXmlApi.Services;

public class SotLpumaConverter : ISotLpumaConverter
{
    private static readonly XNamespace LpoNs = "https://lpuma.skkmigas.go.id/schema";
    private static readonly XNamespace XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

    public (string xmlContent, int rowCount, List<string> warnings) Convert(Stream excelStream, string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var ms = new MemoryStream();
        excelStream.CopyTo(ms);
        ms.Position = 0;

        var (rows, hiddenCount) = ParseExcelRows(ms);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("File Excel tidak memiliki data atau seluruh baris disembunyikan/difilter keluar.");
        }

        var warnings = LpoEnumValidator.Validate(rows);
        if (hiddenCount > 0)
        {
            warnings.Insert(0, $"Informasi Filter/Grouping: Terdapat {hiddenCount} baris data yang dilewati karena berstatus disembunyikan / difilter (AutoFilter atau Grouping Collapse). Hanya {rows.Count} baris aktif (visible) yang dikonversi ke XML.");
        }

        var xmlString = GenerateLpumaXml(rows);

        return (xmlString, rows.Count, warnings);
    }

    private (List<SotLpumaRow> rows, int hiddenCount) ParseExcelRows(Stream stream)
    {
        try
        {
            return ParseExcelRowsClosedXml(stream);
        }
        catch
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
                var legacyRows = ParseExcelRowsExcelDataReader(stream);
                return (legacyRows, 0);
            }
            throw;
        }
    }

    private (List<SotLpumaRow> rows, int hiddenCount) ParseExcelRowsClosedXml(Stream stream)
    {
        var list = new List<SotLpumaRow>();
        int hiddenCount = 0;

        using var workbook = new XLWorkbook(stream);
        if (workbook.Worksheets.Count == 0) return (list, 0);

        IXLWorksheet worksheet = workbook.Worksheet(1);
        int headerRowNum = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Prioritas sheet: periksa setiap sheet untuk menemukan baris header LPO
        foreach (var ws in workbook.Worksheets)
        {
            int maxScan = Math.Min(15, ws.LastRowUsed()?.RowNumber() ?? 0);
            for (int r = 1; r <= maxScan; r++)
            {
                var row = ws.Row(r);
                var tempMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
                for (int c = 1; c <= lastCol; c++)
                {
                    var val = row.Cell(c).GetString().Trim();
                    if (!string.IsNullOrEmpty(val))
                    {
                        tempMap[CleanHeader(val)] = c;
                    }
                }

                // Cek apakah sheet dan baris ini adalah header valid LPO
                bool hasKkksOrWk = tempMap.ContainsKey("kkks") || tempMap.ContainsKey("wk") || tempMap.ContainsKey("wilayahkerja");
                bool hasLpoFields = tempMap.ContainsKey("kategori") || tempMap.ContainsKey("category") ||
                                    tempMap.ContainsKey("eventid") || tempMap.ContainsKey("facilitytype") ||
                                    tempMap.ContainsKey("fieldareatrainplatformall");

                if (hasKkksOrWk && hasLpoFields)
                {
                    worksheet = ws;
                    headerRowNum = r;
                    colMap = tempMap;
                    break;
                }
            }

            if (headerRowNum != -1) break;
        }

        // Fallback jika tidak terdeteksi spesifik: gunakan baris pertama sheet pertama
        if (headerRowNum == -1)
        {
            worksheet = workbook.Worksheet(1);
            headerRowNum = 1;
            int lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int c = 1; c <= lastCol; c++)
            {
                var val = worksheet.Row(1).Cell(c).GetString().Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    colMap[CleanHeader(val)] = c;
                }
            }
        }

        int totalRows = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        for (int r = headerRowNum + 1; r <= totalRows; r++)
        {
            var row = worksheet.Row(r);

            // KUNCI: Lewati baris jika difilter keluar (AutoFilter), di-grouping & collapsed, atau di-hide!
            if (row.IsHidden || row.Height <= 0)
            {
                if (row.CellsUsed().Any(c => !string.IsNullOrWhiteSpace(c.GetString())))
                {
                    hiddenCount++;
                }
                continue;
            }

            // Check if row is empty
            bool allEmpty = true;
            foreach (var cell in row.CellsUsed())
            {
                if (!string.IsNullOrWhiteSpace(cell.GetString()))
                {
                    allEmpty = false;
                    break;
                }
            }
            if (allEmpty) continue;

            var rawEventDate = GetRawColValue(row, colMap, "EVENT_DATE", "Tanggal", "eventDate", "date");
            var eventDate = ParseDateString(rawEventDate);

            var rawStartTime = GetRawColValue(row, colMap, "START_TIME", "Start Time", "startTime");
            var rawEndTime = GetRawColValue(row, colMap, "END_TIME", "End Time", "endTime");
            var rawEstFinish = GetRawColValue(row, colMap, "ESTIMATED_FINISH_DATE", "Est. Tanggal Selesai", "estFinishDate", "estFinishTime");

            var item = new SotLpumaRow
            {
                DocumentName = GetColString(row, colMap, "documentName", "bysistemkkks", "document_name"),
                DocumentDate = ParseDateString(GetRawColValue(row, colMap, "documentDate", "docdate")),
                Operator = GetColString(row, colMap, "KKKS", "operator"),
                WilayahKerja = GetColString(row, colMap, "WK", "wilayahKerja"),
                FacilityKind = GetColString(row, colMap, "FACILITY_TYPE", "Field/Area/Train/Platform/All", "facilityKind", "facility_kind"),
                FacilityName = GetColString(row, colMap, "FACILITY_NAME", "Field/Area/Train/Platform/All Name", "facilityName", "facility_name"),
                EventDate = eventDate,
                Days = GetColString(row, colMap, "DAY_NUMBER", "Hari ke-", "days", "hari"),
                StartTime = ParseDateTimeString(rawStartTime, eventDate),
                EndTime = ParseDateTimeString(rawEndTime, eventDate),
                EventId = GetColString(row, colMap, "EVENT_ID", "eventID", "shutdownID", "event_id"),
                Category = GetColString(row, colMap, "CATEGORY", "Kategori", "category"),
                SystemSourceLevel1 = GetColString(row, colMap, "SYSTEM_SOURCE_LEVEL_1", "System Source Level 1", "systemSourceLevel1"),
                SystemSourceLevel2 = GetColString(row, colMap, "SYSTEM_SOURCE_LEVEL_2", "System Source Level 2", "systemSourceLevel2"),
                SystemCauseLevel1 = GetColString(row, colMap, "SYSTEM_CAUSE_LEVEL_1", "System Cause Level1", "systemCauseLevel1"),
                SystemCauseLevel2 = GetColString(row, colMap, "SYSTEM_CAUSE_LEVEL_2", "System Cause Level2", "systemCauseLevel2"),
                EventsActivities = GetColString(row, colMap, "ACTIVITY_DESCRIPTION", "Kejadian/Kegiatan", "eventsActivities", "activities", "kegiatan"),
                UpdateProgress = GetColString(row, colMap, "UPDATE_PROGRESS", "Update Progress", "updateProgress", "progress"),
                TotalWellOff = GetColString(row, colMap, "TOTAL_WELL_OFF", "Jumlah Sumur OFF", "totalWellOFF", "totalWellOff"),
                EstFinishDate = ParseDateString(rawEstFinish),
                VolumeOil = FormatDecimalString(GetColString(row, colMap, "VOLUME_OIL", "Volume LPO Minyak (BBLS)", "volumeOil", "volumeOilBbls")),
                VolumeGas = FormatDecimalString(GetColString(row, colMap, "VOLUME_GAS", "Volume LPO Gas (MMSCF)", "volumeGas", "volumeGasMmscf"))
            };

            // Jika FacilityKind kosong tapi FacilityName ada, default ke Field
            if (string.IsNullOrEmpty(item.FacilityKind) && !string.IsNullOrEmpty(item.FacilityName))
            {
                item.FacilityKind = "Field";
            }

            // Skip baris subtotal/summary jika kolom inti kosong
            if (string.IsNullOrWhiteSpace(item.Operator) && string.IsNullOrWhiteSpace(item.FacilityName) && string.IsNullOrWhiteSpace(item.EventId))
            {
                continue;
            }

            list.Add(item);
        }

        return (list, hiddenCount);
    }

    private static object? GetRawColValue(IXLRow row, Dictionary<string, int> colMap, params string[] candidateKeys)
    {
        foreach (var key in candidateKeys)
        {
            var norm = CleanHeader(key);
            if (colMap.TryGetValue(norm, out int colIdx))
            {
                var cell = row.Cell(colIdx);
                if (!cell.IsEmpty())
                {
                    if (cell.DataType == XLDataType.DateTime)
                    {
                        return cell.GetDateTime();
                    }
                    if (cell.DataType == XLDataType.TimeSpan)
                    {
                        return cell.GetTimeSpan();
                    }
                    var formatted = cell.GetFormattedString()?.Trim();
                    if (!string.IsNullOrEmpty(formatted))
                    {
                        return formatted;
                    }
                    return cell.Value.ToString()?.Trim();
                }
            }
        }
        return null;
    }

    private static string GetColString(IXLRow row, Dictionary<string, int> colMap, params string[] candidateKeys)
    {
        var obj = GetRawColValue(row, colMap, candidateKeys);
        if (obj == null) return string.Empty;
        if (obj is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
        return obj.ToString()?.Trim() ?? string.Empty;
    }

    private static string CleanHeader(string raw)
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

    private static string ParseDateString(object? rawVal)
    {
        if (rawVal == null) return string.Empty;
        if (rawVal is DateTime dt) return dt.ToString("yyyy-MM-dd");

        var str = rawVal.ToString()?.Trim();
        if (string.IsNullOrEmpty(str)) return string.Empty;

        // Format tanggal yang umum
        string[] formats = {
            "yyyy-MM-dd", "d-MMM-yyyy", "dd-MMM-yyyy", "d-MMMM-yyyy", "dd-MMMM-yyyy",
            "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyyy",
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "d-MMM-yyyy HH:mm:ss", "dd-MMM-yyyy HH:mm:ss"
        };

        if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDt) ||
            DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDt) ||
            DateTime.TryParse(str, CultureInfo.GetCultureInfo("id-ID"), DateTimeStyles.None, out parsedDt))
        {
            return parsedDt.ToString("yyyy-MM-dd");
        }

        return str;
    }

    private static string ParseDateTimeString(object? rawVal, string fallbackDate)
    {
        if (rawVal == null) return string.Empty;
        var baseDate = !string.IsNullOrEmpty(fallbackDate) ? fallbackDate : DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (rawVal is DateTime dt)
        {
            // Jika tahun < 1920, ini adalah waktu time-only serial yang disimpan Excel
            if (dt.Year < 1920)
            {
                return $"{baseDate}T{dt:HH\\:mm\\:ss}";
            }
            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        if (rawVal is TimeSpan ts)
        {
            return $"{baseDate}T{ts:hh\\:mm\\:ss}";
        }

        var str = rawVal.ToString()?.Trim();
        if (string.IsNullOrEmpty(str)) return string.Empty;

        // Format datetime yang umum
        string[] dateTimeFormats = {
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss", "d-MMM-yyyy HH:mm:ss", "dd-MMM-yyyy HH:mm:ss",
            "d-MMM-yyyy H:mm:ss", "dd-MMM-yyyy H:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy H:mm:ss"
        };

        if (DateTime.TryParseExact(str, dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDt) ||
            DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDt))
        {
            return parsedDt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        // Jika hanya waktu, seperti "00:00:00" atau "08:30:00"
        string[] timeFormats = { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm" };
        if (TimeSpan.TryParseExact(str, timeFormats, CultureInfo.InvariantCulture, out var parsedTs) ||
            TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out parsedTs))
        {
            return $"{baseDate}T{parsedTs:hh\\:mm\\:ss}";
        }

        return str;
    }

    private List<SotLpumaRow> ParseExcelRowsExcelDataReader(Stream stream)
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

        // Cari sheet terbaik yang memuat tabel pelaporan data LPO
        DataTable table = dataSet.Tables[0];
        int headerRowIndex = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Prioritas sheet: periksa setiap sheet untuk menemukan baris header LPO
        foreach (DataTable t in dataSet.Tables)
        {
            for (int r = 0; r < Math.Min(10, t.Rows.Count); r++)
            {
                var tempMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < t.Columns.Count; c++)
                {
                    var val = t.Rows[r][c]?.ToString()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(val))
                    {
                        var normalized = CleanHeader(val);
                        tempMap[normalized] = c;
                    }
                }

                // Cek apakah sheet dan baris ini adalah header valid LPO
                bool hasKkksOrWk = tempMap.ContainsKey("kkks") || tempMap.ContainsKey("wk") || tempMap.ContainsKey("wilayahkerja");
                bool hasLpoFields = tempMap.ContainsKey("kategori") || tempMap.ContainsKey("category") ||
                                    tempMap.ContainsKey("eventid") || tempMap.ContainsKey("facilitytype") ||
                                    tempMap.ContainsKey("fieldareatrainplatformall");

                if (hasKkksOrWk && hasLpoFields)
                {
                    table = t;
                    headerRowIndex = r;
                    colMap = tempMap;
                    break;
                }
            }

            if (headerRowIndex != -1) break;
        }

        // Fallback jika tidak terdeteksi spesifik: gunakan baris pertama sheet pertama
        if (headerRowIndex == -1)
        {
            table = dataSet.Tables[0];
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

        object? GetRawColValueFromDataRow(DataRow row, params string[] candidateKeys)
        {
            foreach (var key in candidateKeys)
            {
                var norm = CleanHeader(key);
                if (colMap.TryGetValue(norm, out int colIdx))
                {
                    var obj = row[colIdx];
                    if (obj != null && obj != DBNull.Value)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        string GetColStringFromDataRow(DataRow row, params string[] candidateKeys)
        {
            var obj = GetRawColValueFromDataRow(row, candidateKeys);
            if (obj == null) return string.Empty;
            if (obj is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd");
            }
            return obj.ToString()?.Trim() ?? string.Empty;
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

            // Parse event date terlebih dahulu untuk fallback dateTime
            var rawEventDate = GetRawColValueFromDataRow(dr, "EVENT_DATE", "Tanggal", "eventDate", "date");
            var eventDate = ParseDateString(rawEventDate);

            var rawStartTime = GetRawColValueFromDataRow(dr, "START_TIME", "Start Time", "startTime");
            var rawEndTime = GetRawColValueFromDataRow(dr, "END_TIME", "End Time", "endTime");
            var rawEstFinish = GetRawColValueFromDataRow(dr, "ESTIMATED_FINISH_DATE", "Est. Tanggal Selesai", "estFinishDate", "estFinishTime");

            var item = new SotLpumaRow
            {
                DocumentName = GetColStringFromDataRow(dr, "documentName", "bysistemkkks", "document_name"),
                DocumentDate = ParseDateString(GetRawColValueFromDataRow(dr, "documentDate", "docdate")),
                Operator = GetColStringFromDataRow(dr, "KKKS", "operator"),
                WilayahKerja = GetColStringFromDataRow(dr, "WK", "wilayahKerja"),
                FacilityKind = GetColStringFromDataRow(dr, "FACILITY_TYPE", "Field/Area/Train/Platform/All", "facilityKind", "facility_kind"),
                FacilityName = GetColStringFromDataRow(dr, "FACILITY_NAME", "Field/Area/Train/Platform/All Name", "facilityName", "facility_name"),
                EventDate = eventDate,
                Days = GetColStringFromDataRow(dr, "DAY_NUMBER", "Hari ke-", "days", "hari"),
                StartTime = ParseDateTimeString(rawStartTime, eventDate),
                EndTime = ParseDateTimeString(rawEndTime, eventDate),
                EventId = GetColStringFromDataRow(dr, "EVENT_ID", "eventID", "shutdownID", "event_id"),
                Category = GetColStringFromDataRow(dr, "CATEGORY", "Kategori", "category"),
                SystemSourceLevel1 = GetColStringFromDataRow(dr, "SYSTEM_SOURCE_LEVEL_1", "System Source Level 1", "systemSourceLevel1"),
                SystemSourceLevel2 = GetColStringFromDataRow(dr, "SYSTEM_SOURCE_LEVEL_2", "System Source Level 2", "systemSourceLevel2"),
                SystemCauseLevel1 = GetColStringFromDataRow(dr, "SYSTEM_CAUSE_LEVEL_1", "System Cause Level1", "systemCauseLevel1"),
                SystemCauseLevel2 = GetColStringFromDataRow(dr, "SYSTEM_CAUSE_LEVEL_2", "System Cause Level2", "systemCauseLevel2"),
                EventsActivities = GetColStringFromDataRow(dr, "ACTIVITY_DESCRIPTION", "Kejadian/Kegiatan", "eventsActivities", "activities", "kegiatan"),
                UpdateProgress = GetColStringFromDataRow(dr, "UPDATE_PROGRESS", "Update Progress", "updateProgress", "progress"),
                TotalWellOff = GetColStringFromDataRow(dr, "TOTAL_WELL_OFF", "Jumlah Sumur OFF", "totalWellOFF", "totalWellOff"),
                EstFinishDate = ParseDateString(rawEstFinish),
                VolumeOil = FormatDecimalString(GetColStringFromDataRow(dr, "VOLUME_OIL", "Volume LPO Minyak (BBLS)", "volumeOil", "volumeOilBbls")),
                VolumeGas = FormatDecimalString(GetColStringFromDataRow(dr, "VOLUME_GAS", "Volume LPO Gas (MMSCF)", "volumeGas", "volumeGasMmscf"))
            };

            // Jika FacilityKind kosong tapi FacilityName ada, default ke Field
            if (string.IsNullOrEmpty(item.FacilityKind) && !string.IsNullOrEmpty(item.FacilityName))
            {
                item.FacilityKind = "Field";
            }

            list.Add(item);
        }

        return list;
    }

    private static string FormatDecimalString(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return "0.000000";
        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ||
            decimal.TryParse(val, NumberStyles.Any, CultureInfo.GetCultureInfo("id-ID"), out d))
        {
            return d.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        return val;
    }

    private string GenerateLpumaXml(List<SotLpumaRow> rows)
    {
        var first = rows.First();

        var docName = !string.IsNullOrWhiteSpace(first.DocumentName) 
            ? first.DocumentName 
            : "LPO";

        var docDate = !string.IsNullOrWhiteSpace(first.DocumentDate) 
            ? first.DocumentDate 
            : (!string.IsNullOrWhiteSpace(first.EventDate) ? first.EventDate : DateTime.UtcNow.ToString("yyyy-MM-dd"));

        // Buat root element dengan namespace resmi LPO SKK Migas
        var root = new XElement(LpoNs + "lossProductionOpportunity",
            new XAttribute(XNamespace.Xmlns + "xsi", XsiNs.NamespaceName),
            new XAttribute(XsiNs + "schemaLocation", "https://lpuma.skkmigas.go.id/schema LPO.xsd"),
            new XElement(LpoNs + "documentName", docName),
            new XElement(LpoNs + "documentDate", docDate),
            new XElement(LpoNs + "operator", first.Operator),
            new XElement(LpoNs + "wilayahKerja", first.WilayahKerja)
        );

        var eventsElem = new XElement(LpoNs + "events");

        // Group by EventDate
        var dateGroups = rows.GroupBy(r => string.IsNullOrEmpty(r.EventDate) ? docDate : r.EventDate);

        foreach (var group in dateGroups)
        {
            var eventDateElem = new XElement(LpoNs + "eventDate",
                new XAttribute("date", group.Key));

            foreach (var r in group)
            {
                var eventElem = new XElement(LpoNs + "event",
                    new XAttribute("eventId", string.IsNullOrWhiteSpace(r.EventId) ? Guid.NewGuid().ToString("N") : r.EventId)
                );

                var facilityElem = new XElement(LpoNs + "facility");
                if (!string.IsNullOrWhiteSpace(r.FacilityName))
                {
                    facilityElem.Add(new XAttribute("facilityName", r.FacilityName));
                }
                facilityElem.Add(new XAttribute("facilityKind", string.IsNullOrWhiteSpace(r.FacilityKind) ? "Field" : r.FacilityKind));

                facilityElem.Add(new XElement(LpoNs + "days", string.IsNullOrWhiteSpace(r.Days) ? "1" : r.Days));
                facilityElem.Add(new XElement(LpoNs + "startTime", r.StartTime));
                facilityElem.Add(new XElement(LpoNs + "endTime", r.EndTime));
                facilityElem.Add(new XElement(LpoNs + "category", r.Category));
                facilityElem.Add(new XElement(LpoNs + "systemSourceLevel1", r.SystemSourceLevel1));
                facilityElem.Add(new XElement(LpoNs + "systemSourceLevel2", r.SystemSourceLevel2));
                facilityElem.Add(new XElement(LpoNs + "systemCauseLevel1", r.SystemCauseLevel1));
                facilityElem.Add(new XElement(LpoNs + "systemCauseLevel2", r.SystemCauseLevel2));
                facilityElem.Add(new XElement(LpoNs + "eventsActivities", r.EventsActivities));

                if (!string.IsNullOrWhiteSpace(r.UpdateProgress))
                {
                    facilityElem.Add(new XElement(LpoNs + "updateProgress", r.UpdateProgress));
                }

                if (!string.IsNullOrWhiteSpace(r.TotalWellOff))
                {
                    facilityElem.Add(new XElement(LpoNs + "totalWellOFF", r.TotalWellOff));
                }

                if (!string.IsNullOrWhiteSpace(r.EstFinishDate))
                {
                    facilityElem.Add(new XElement(LpoNs + "estFinishDate", r.EstFinishDate));
                }

                facilityElem.Add(new XElement(LpoNs + "volumeOil", r.VolumeOil));
                facilityElem.Add(new XElement(LpoNs + "volumeGas", r.VolumeGas));

                eventElem.Add(facilityElem);
                eventDateElem.Add(eventElem);
            }

            eventsElem.Add(eventDateElem);
        }

        root.Add(eventsElem);

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            root
        );

        return doc.Declaration?.ToString() + Environment.NewLine + doc.ToString();
    }
}
