using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
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

        var rows = ParseExcelRows(excelStream);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("File Excel tidak memiliki data atau format tidak dikenali.");
        }

        var warnings = LpoEnumValidator.Validate(rows);
        var xmlString = GenerateLpumaXml(rows);

        return (xmlString, rows.Count, warnings);
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

        object? GetRawColValue(DataRow row, params string[] candidateKeys)
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

        string GetColString(DataRow row, params string[] candidateKeys)
        {
            var obj = GetRawColValue(row, candidateKeys);
            if (obj == null) return string.Empty;
            if (obj is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd");
            }
            return obj.ToString()?.Trim() ?? string.Empty;
        }

        string ParseDateString(object? rawVal)
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

        string ParseDateTimeString(object? rawVal, string fallbackDate)
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
            var rawEventDate = GetRawColValue(dr, "EVENT_DATE", "Tanggal", "eventDate", "date");
            var eventDate = ParseDateString(rawEventDate);

            var rawStartTime = GetRawColValue(dr, "START_TIME", "Start Time", "startTime");
            var rawEndTime = GetRawColValue(dr, "END_TIME", "End Time", "endTime");
            var rawEstFinish = GetRawColValue(dr, "ESTIMATED_FINISH_DATE", "Est. Tanggal Selesai", "estFinishDate", "estFinishTime");

            var item = new SotLpumaRow
            {
                DocumentName = GetColString(dr, "documentName", "bysistemkkks", "document_name"),
                DocumentDate = ParseDateString(GetRawColValue(dr, "documentDate", "docdate")),
                Operator = GetColString(dr, "KKKS", "operator"),
                WilayahKerja = GetColString(dr, "WK", "wilayahKerja"),
                FacilityKind = GetColString(dr, "FACILITY_TYPE", "Field/Area/Train/Platform/All", "facilityKind", "facility_kind"),
                FacilityName = GetColString(dr, "FACILITY_NAME", "Field/Area/Train/Platform/All Name", "facilityName", "facility_name"),
                EventDate = eventDate,
                Days = GetColString(dr, "DAY_NUMBER", "Hari ke-", "days", "hari"),
                StartTime = ParseDateTimeString(rawStartTime, eventDate),
                EndTime = ParseDateTimeString(rawEndTime, eventDate),
                EventId = GetColString(dr, "EVENT_ID", "eventID", "shutdownID", "event_id"),
                Category = GetColString(dr, "CATEGORY", "Kategori", "category"),
                SystemSourceLevel1 = GetColString(dr, "SYSTEM_SOURCE_LEVEL_1", "System Source Level 1", "systemSourceLevel1"),
                SystemSourceLevel2 = GetColString(dr, "SYSTEM_SOURCE_LEVEL_2", "System Source Level 2", "systemSourceLevel2"),
                SystemCauseLevel1 = GetColString(dr, "SYSTEM_CAUSE_LEVEL_1", "System Cause Level1", "systemCauseLevel1"),
                SystemCauseLevel2 = GetColString(dr, "SYSTEM_CAUSE_LEVEL_2", "System Cause Level2", "systemCauseLevel2"),
                EventsActivities = GetColString(dr, "ACTIVITY_DESCRIPTION", "Kejadian/Kegiatan", "eventsActivities", "activities", "kegiatan"),
                UpdateProgress = GetColString(dr, "UPDATE_PROGRESS", "Update Progress", "updateProgress", "progress"),
                TotalWellOff = GetColString(dr, "TOTAL_WELL_OFF", "Jumlah Sumur OFF", "totalWellOFF", "totalWellOff"),
                EstFinishDate = ParseDateString(rawEstFinish),
                VolumeOil = FormatDecimalString(GetColString(dr, "VOLUME_OIL", "Volume LPO Minyak (BBLS)", "volumeOil", "volumeOilBbls")),
                VolumeGas = FormatDecimalString(GetColString(dr, "VOLUME_GAS", "Volume LPO Gas (MMSCF)", "volumeGas", "volumeGasMmscf"))
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
