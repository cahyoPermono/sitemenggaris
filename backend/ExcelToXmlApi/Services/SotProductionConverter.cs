using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using ExcelDataReader;
using ExcelToXmlApi.Models;

namespace ExcelToXmlApi.Services;

public class SotProductionConverter : ISotProductionConverter
{
    private static readonly XNamespace ProdmlNs = "http://www.prodml.org/schemas/1series";

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

        var warnings = new List<string>();
        if (hiddenCount > 0)
        {
            warnings.Add($"Informasi Filter/Grouping: Terdapat {hiddenCount} baris data yang dilewati karena berstatus disembunyikan / difilter (AutoFilter atau Grouping Collapse). Hanya {rows.Count} baris aktif (visible) yang dikonversi ke XML.");
        }

        // Check if mandatory fields are missing
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int line = i + 2;
            if (string.IsNullOrWhiteSpace(r.FacilityName))
            {
                warnings.Add($"Baris {line}: facilityName kosong.");
            }
            if (string.IsNullOrWhiteSpace(r.FlowName))
            {
                warnings.Add($"Baris {line}: flowName kosong.");
            }
            if (string.IsNullOrWhiteSpace(r.ProductName))
            {
                warnings.Add($"Baris {line}: productName kosong.");
            }
        }

        var xmlString = GenerateProdMlXml(rows, fileName);
        return (xmlString, rows.Count, warnings);
    }

    private (List<SotProductionRow> rows, int hiddenCount) ParseExcelRows(Stream stream)
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

    private (List<SotProductionRow> rows, int hiddenCount) ParseExcelRowsClosedXml(Stream stream)
    {
        var list = new List<SotProductionRow>();
        int hiddenCount = 0;

        using var workbook = new XLWorkbook(stream);
        if (workbook.Worksheets.Count == 0) return (list, 0);

        IXLWorksheet worksheet = workbook.Worksheet(1);
        int headerRowNum = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Cari sheet terbaik yang memuat kolom SOT Production
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

                if (tempMap.ContainsKey("prodvolumename") || tempMap.ContainsKey("facilityname"))
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

            var rawPeriodDate = GetRawColValue(row, colMap, "periodDate", "period_date", "date", "tanggal");

            var item = new SotProductionRow
            {
                ProdVolumeName = GetColString(row, colMap, "prodVolumeName", "volumeName", "prod_volume_name"),
                ProdVolumeOperatorName = GetColString(row, colMap, "prodVolumeOperatorName", "operatorName", "operator", "prod_volume_operator_name"),
                FacilityName = GetColString(row, colMap, "facilityName", "facility_name"),
                FacilityKind = GetColString(row, colMap, "facilityKind", "facility_kind"),
                FacilityNamingSystem = GetColString(row, colMap, "facilityNamingSystem", "namingSystem", "facility_naming_system"),
                ContextFacility = GetColString(row, colMap, "contextFacility", "context_facility"),
                FacilityParent1 = GetColString(row, colMap, "facilityParent1", "facility_parent1", "parent1"),
                FacilityParent1Kind = GetColString(row, colMap, "facilityParent1Kind", "facility_parent1_kind", "parent1Kind"),
                FacilityParent2 = GetColString(row, colMap, "facilityParent2", "facility_parent2", "parent2"),
                FacilityParent2Kind = GetColString(row, colMap, "facilityParent2Kind", "facility_parent2_kind", "parent2Kind"),
                FlowName = GetColString(row, colMap, "flowName", "flow_name"),
                FlowKind = GetColString(row, colMap, "flowKind", "flow_kind"),
                FlowDirection = GetColString(row, colMap, "flowDirection", "flow_direction"),
                FlowQualifier = GetColString(row, colMap, "flowQualifier", "flow_qualifier"),
                ProductName = GetColString(row, colMap, "productName", "product_name"),
                ProductKind = GetColString(row, colMap, "productKind", "product_kind"),
                PeriodKind = GetColString(row, colMap, "periodKind", "period_kind"),
                PeriodDate = ParseDateString(rawPeriodDate),
                PeriodVolumeValue = GetColString(row, colMap, "periodVolumeValue", "volumeValue", "volume", "period_volume_value"),
                PeriodVolumeValueUom = GetColString(row, colMap, "periodVolumeValueUom", "volumeUom", "uom", "period_volume_value_uom"),
                PeriodBlnceSetKind = GetColString(row, colMap, "periodBlnceSetKind", "balanceSetKind", "period_blnce_set_kind"),
                BlnceSetCargoNumber = GetColString(row, colMap, "BlnceSetCargoNumber", "cargoNumber", "blnce_set_cargo_number"),
                BlnceSetCargoBatchNumber = GetColString(row, colMap, "BlnceSetCargoBatchNumber", "cargoBatchNumber", "blnce_set_cargo_batch_number"),
                BlnceSetShipper = GetColString(row, colMap, "BlnceSetShipper", "shipper", "blnce_set_shipper"),
                BlnceSetBuyer = GetColString(row, colMap, "BlnceSetBuyer", "buyer", "blnce_set_buyer"),
                BlnceSetDestinationName = GetColString(row, colMap, "BlnceSetDestinationName", "destinationName", "destination", "blnce_set_destination_name"),
                BlnceSetOwner = GetColString(row, colMap, "BlnceSetOwner", "owner", "blnce_set_owner"),
                BlnceSetEventKind = GetColString(row, colMap, "BlnceSetEventKind", "eventKind", "blnce_set_event_kind"),
                PeriodBlnceSetVolValue = GetColString(row, colMap, "periodBlnceSetVolValue", "balanceVolumeValue", "period_blnce_set_vol_value"),
                PeriodBlnceVolValueUom = GetColString(row, colMap, "periodBlnceVolValueUom", "balanceVolumeUom", "period_blnce_vol_value_uom"),
                PeriodCompContWork = GetColString(row, colMap, "periodCompContWork", "componentContentWork", "period_comp_cont_work"),
                PeriodCompContWorkUom = GetColString(row, colMap, "periodCompContWorkUom", "componentContentWorkUom", "period_comp_cont_work_uom")
            };

            // Skip subtotal/summary row without minimal identifying columns
            if (string.IsNullOrWhiteSpace(item.FacilityName) && string.IsNullOrWhiteSpace(item.FlowName) && string.IsNullOrWhiteSpace(item.ProductName))
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

    private static string ParseDateString(object? rawVal)
    {
        if (rawVal == null) return string.Empty;
        if (rawVal is DateTime dt) return dt.ToString("yyyy-MM-dd");

        var str = rawVal.ToString()?.Trim();
        if (string.IsNullOrEmpty(str)) return string.Empty;

        string[] formats = {
            "yyyy-MM-dd", "d-MMM-yyyy", "dd-MMM-yyyy", "d-MMMM-yyyy", "dd-MMMM-yyyy",
            "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyyy"
        };

        if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDt) ||
            DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDt) ||
            DateTime.TryParse(str, CultureInfo.GetCultureInfo("id-ID"), DateTimeStyles.None, out parsedDt))
        {
            return parsedDt.ToString("yyyy-MM-dd");
        }

        return str;
    }

    private List<SotProductionRow> ParseExcelRowsExcelDataReader(Stream stream)
    {
        var list = new List<SotProductionRow>();

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = false
            }
        });

        if (dataSet.Tables.Count == 0) return list;

        DataTable table = dataSet.Tables[0];
        int headerRowIndex = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Cari sheet terbaik yang memuat kolom SOT Production
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

                if (tempMap.ContainsKey("prodvolumename") || tempMap.ContainsKey("facilityname"))
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

            // Check if row is empty
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

            var rawPeriodDate = GetRawColValueFromDataRow(dr, "periodDate", "period_date", "date", "tanggal");

            var item = new SotProductionRow
            {
                ProdVolumeName = GetColStringFromDataRow(dr, "prodVolumeName", "volumeName", "prod_volume_name"),
                ProdVolumeOperatorName = GetColStringFromDataRow(dr, "prodVolumeOperatorName", "operatorName", "operator", "prod_volume_operator_name"),
                FacilityName = GetColStringFromDataRow(dr, "facilityName", "facility_name"),
                FacilityKind = GetColStringFromDataRow(dr, "facilityKind", "facility_kind"),
                FacilityNamingSystem = GetColStringFromDataRow(dr, "facilityNamingSystem", "namingSystem", "facility_naming_system"),
                ContextFacility = GetColStringFromDataRow(dr, "contextFacility", "context_facility"),
                FacilityParent1 = GetColStringFromDataRow(dr, "facilityParent1", "facility_parent1", "parent1"),
                FacilityParent1Kind = GetColStringFromDataRow(dr, "facilityParent1Kind", "facility_parent1_kind", "parent1Kind"),
                FacilityParent2 = GetColStringFromDataRow(dr, "facilityParent2", "facility_parent2", "parent2"),
                FacilityParent2Kind = GetColStringFromDataRow(dr, "facilityParent2Kind", "facility_parent2_kind", "parent2Kind"),
                FlowName = GetColStringFromDataRow(dr, "flowName", "flow_name"),
                FlowKind = GetColStringFromDataRow(dr, "flowKind", "flow_kind"),
                FlowDirection = GetColStringFromDataRow(dr, "flowDirection", "flow_direction"),
                FlowQualifier = GetColStringFromDataRow(dr, "flowQualifier", "flow_qualifier"),
                ProductName = GetColStringFromDataRow(dr, "productName", "product_name"),
                ProductKind = GetColStringFromDataRow(dr, "productKind", "product_kind"),
                PeriodKind = GetColStringFromDataRow(dr, "periodKind", "period_kind"),
                PeriodDate = ParseDateString(rawPeriodDate),
                PeriodVolumeValue = GetColStringFromDataRow(dr, "periodVolumeValue", "volumeValue", "volume", "period_volume_value"),
                PeriodVolumeValueUom = GetColStringFromDataRow(dr, "periodVolumeValueUom", "volumeUom", "uom", "period_volume_value_uom"),
                PeriodBlnceSetKind = GetColStringFromDataRow(dr, "periodBlnceSetKind", "balanceSetKind", "period_blnce_set_kind"),
                BlnceSetCargoNumber = GetColStringFromDataRow(dr, "BlnceSetCargoNumber", "cargoNumber", "blnce_set_cargo_number"),
                BlnceSetCargoBatchNumber = GetColStringFromDataRow(dr, "BlnceSetCargoBatchNumber", "cargoBatchNumber", "blnce_set_cargo_batch_number"),
                BlnceSetShipper = GetColStringFromDataRow(dr, "BlnceSetShipper", "shipper", "blnce_set_shipper"),
                BlnceSetBuyer = GetColStringFromDataRow(dr, "BlnceSetBuyer", "buyer", "blnce_set_buyer"),
                BlnceSetDestinationName = GetColStringFromDataRow(dr, "BlnceSetDestinationName", "destinationName", "destination", "blnce_set_destination_name"),
                BlnceSetOwner = GetColStringFromDataRow(dr, "BlnceSetOwner", "owner", "blnce_set_owner"),
                BlnceSetEventKind = GetColStringFromDataRow(dr, "BlnceSetEventKind", "eventKind", "blnce_set_event_kind"),
                PeriodBlnceSetVolValue = GetColStringFromDataRow(dr, "periodBlnceSetVolValue", "balanceVolumeValue", "period_blnce_set_vol_value"),
                PeriodBlnceVolValueUom = GetColStringFromDataRow(dr, "periodBlnceVolValueUom", "balanceVolumeUom", "period_blnce_vol_value_uom"),
                PeriodCompContWork = GetColStringFromDataRow(dr, "periodCompContWork", "componentContentWork", "period_comp_cont_work"),
                PeriodCompContWorkUom = GetColStringFromDataRow(dr, "periodCompContWorkUom", "componentContentWorkUom", "period_comp_cont_work_uom")
            };

            list.Add(item);
        }

        return list;
    }

    private static string CleanHeader(string raw)
    {
        return raw.Replace(" ", "")
                  .Replace("_", "")
                  .Replace("-", "")
                  .Replace(".", "")
                  .Replace("[", "")
                  .Replace("]", "")
                  .Replace("/", "")
                  .ToLowerInvariant();
    }

    private string GenerateProdMlXml(List<SotProductionRow> rows, string fileName)
    {
        // Root element <prodml:productVolumes>
        var root = new XElement(ProdmlNs + "productVolumes",
            new XAttribute(XNamespace.Xmlns + "prodml", ProdmlNs.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "version", "1.2.2")
        );

        var firstRow = rows.First();
        var docName = !string.IsNullOrEmpty(firstRow.ProdVolumeName)
            ? $"{firstRow.ProdVolumeName} - DAILY PRODUCTION REPORT"
            : $"{Path.GetFileNameWithoutExtension(fileName)} - DAILY PRODUCTION REPORT";

        // documentInfo block sesuai standar ProdML TDM
        var docInfo = new XElement(ProdmlNs + "documentInfo",
            new XElement(ProdmlNs + "documentName", docName),
            new XElement(ProdmlNs + "documentAlias"),
            new XElement(ProdmlNs + "documentDate"),
            new XElement(ProdmlNs + "documentClass"),
            new XElement(ProdmlNs + "fileCreationInformation",
                new XElement(ProdmlNs + "fileCreationDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(ProdmlNs + "softwareName", "ExcelToXmlConverter"),
                new XElement(ProdmlNs + "fileCreator"),
                new XElement(ProdmlNs + "comment")
            ),
            new XElement(ProdmlNs + "securityInformation",
                new XElement(ProdmlNs + "class"),
                new XElement(ProdmlNs + "securitySystem"),
                new XElement(ProdmlNs + "endDate"),
                new XElement(ProdmlNs + "comment")
            ),
            new XElement(ProdmlNs + "disclaimer"),
            new XElement(ProdmlNs + "auditTrail",
                new XElement(ProdmlNs + "event",
                    new XElement(ProdmlNs + "eventDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")),
                    new XElement(ProdmlNs + "eventType", "generated"),
                    new XElement(ProdmlNs + "responsibleParty"),
                    new XElement(ProdmlNs + "comment")
                )
            ),
            new XElement(ProdmlNs + "owner"),
            new XElement(ProdmlNs + "queryParam",
                new XElement(ProdmlNs + "dTimMin"),
                new XElement(ProdmlNs + "dTimMax")
            ),
            new XElement(ProdmlNs + "comment")
        );
        root.Add(docInfo);

        // Group by prodVolumeName (or operator if empty)
        var volumeGroups = rows.GroupBy(r => string.IsNullOrEmpty(r.ProdVolumeName) ? "DAILY PRODUCTION REPORT" : r.ProdVolumeName);

        foreach (var volGroup in volumeGroups)
        {
            var volFirst = volGroup.First();
            var volumeElem = new XElement(ProdmlNs + "productVolume",
                new XElement(ProdmlNs + "name", volGroup.Key),
                new XElement(ProdmlNs + "installation"),
                new XElement(ProdmlNs + "contextFacility", volFirst.ContextFacility),
                new XElement(ProdmlNs + "kind"),
                new XElement(ProdmlNs + "periodKind"),
                new XElement(ProdmlNs + "dTimStart"),
                new XElement(ProdmlNs + "dTimEnd"),
                new XElement(ProdmlNs + "dateStart"),
                new XElement(ProdmlNs + "dateEnd"),
                new XElement(ProdmlNs + "month"),
                new XElement(ProdmlNs + "dTimMin"),
                new XElement(ProdmlNs + "dTimMax"),
                new XElement(ProdmlNs + "dTimCurrent"),
                new XElement(ProdmlNs + "calculationMethod"),
                new XElement(ProdmlNs + "operator",
                    new XElement(ProdmlNs + "name", volFirst.ProdVolumeOperatorName)
                ),
                new XElement(ProdmlNs + "title"),
                new XElement(ProdmlNs + "geographicContext",
                    new XElement(ProdmlNs + "country"),
                    new XElement(ProdmlNs + "state"),
                    new XElement(ProdmlNs + "field")
                ),
                new XElement(ProdmlNs + "issueDate"),
                new XElement(ProdmlNs + "issuedBy",
                    new XElement(ProdmlNs + "name"),
                    new XElement(ProdmlNs + "role")
                )
            );

            // Group by facilityName
            var facilityGroups = volGroup.GroupBy(r => r.FacilityName);
            foreach (var facGroup in facilityGroups)
            {
                var facFirst = facGroup.First();
                var facilityElem = new XElement(ProdmlNs + "facility");

                var facNameElem = new XElement(ProdmlNs + "name", facGroup.Key);
                if (!string.IsNullOrEmpty(facFirst.FacilityKind))
                {
                    facNameElem.Add(new XAttribute("kind", facFirst.FacilityKind));
                }
                facilityElem.Add(facNameElem);

                var p1Elem = new XElement(ProdmlNs + "facilityParent1", facFirst.FacilityParent1);
                p1Elem.Add(new XAttribute("kind", facFirst.FacilityParent1Kind));
                facilityElem.Add(p1Elem);

                var p2Elem = new XElement(ProdmlNs + "facilityParent2", facFirst.FacilityParent2);
                p2Elem.Add(new XAttribute("kind", facFirst.FacilityParent2Kind));
                facilityElem.Add(p2Elem);

                facilityElem.Add(new XElement(ProdmlNs + "facilityAlias"));
                facilityElem.Add(new XElement(ProdmlNs + "unit"));
                facilityElem.Add(new XElement(ProdmlNs + "network"));
                facilityElem.Add(new XElement(ProdmlNs + "contextFacility", facFirst.ContextFacility));

                // Group by flowName
                var flowGroups = facGroup.GroupBy(r => r.FlowName);
                foreach (var flowGroup in flowGroups)
                {
                    var flowFirst = flowGroup.First();
                    var flowElem = new XElement(ProdmlNs + "flow",
                        new XElement(ProdmlNs + "name", flowGroup.Key),
                        new XElement(ProdmlNs + "kind", flowFirst.FlowKind),
                        new XElement(ProdmlNs + "port"),
                        new XElement(ProdmlNs + "direction", flowFirst.FlowDirection),
                        new XElement(ProdmlNs + "facility"),
                        new XElement(ProdmlNs + "facilityAlias"),
                        new XElement(ProdmlNs + "qualifier", flowFirst.FlowQualifier)
                    );

                    // Group by productName
                    var productGroups = flowGroup.GroupBy(r => r.ProductName);
                    foreach (var prodGroup in productGroups)
                    {
                        var prodFirst = prodGroup.First();
                        var prodElem = new XElement(ProdmlNs + "product",
                            new XElement(ProdmlNs + "kind", prodFirst.ProductKind),
                            new XElement(ProdmlNs + "name", prodGroup.Key),
                            new XElement(ProdmlNs + "splitFactor")
                        );

                        // Group by period (date & kind)
                        var periodGroups = prodGroup.GroupBy(r => new { r.PeriodDate, r.PeriodKind });
                        foreach (var perGroup in periodGroups)
                        {
                            var perFirst = perGroup.First();
                            var periodElem = new XElement(ProdmlNs + "period",
                                new XElement(ProdmlNs + "kind", perGroup.Key.PeriodKind),
                                new XElement(ProdmlNs + "dTim"),
                                new XElement(ProdmlNs + "date", perGroup.Key.PeriodDate)
                            );

                            var volElem = new XElement(ProdmlNs + "volume", perFirst.PeriodVolumeValue);
                            if (!string.IsNullOrEmpty(perFirst.PeriodVolumeValueUom))
                            {
                                volElem.Add(new XAttribute("uom", perFirst.PeriodVolumeValueUom));
                            }
                            periodElem.Add(volElem);

                            // Balance sets
                            foreach (var bRow in perGroup)
                            {
                                if (string.IsNullOrEmpty(bRow.PeriodBlnceSetKind) &&
                                    string.IsNullOrEmpty(bRow.BlnceSetCargoNumber) &&
                                    string.IsNullOrEmpty(bRow.BlnceSetOwner) &&
                                    string.IsNullOrEmpty(bRow.PeriodBlnceSetVolValue))
                                {
                                    continue;
                                }

                                var balanceSetElem = new XElement(ProdmlNs + "balanceSet",
                                    new XElement(ProdmlNs + "kind", bRow.PeriodBlnceSetKind),
                                    new XElement(ProdmlNs + "cargoNumber", bRow.BlnceSetCargoNumber),
                                    new XElement(ProdmlNs + "cargoBatchNumber", bRow.BlnceSetCargoBatchNumber),
                                    new XElement(ProdmlNs + "shipper", bRow.BlnceSetShipper),
                                    new XElement(ProdmlNs + "buyer", bRow.BlnceSetBuyer),
                                    new XElement(ProdmlNs + "destination",
                                        new XElement(ProdmlNs + "name", bRow.BlnceSetDestinationName),
                                        new XElement(ProdmlNs + "type"),
                                        new XElement(ProdmlNs + "country")
                                    )
                                );

                                var balanceDetailElem = new XElement(ProdmlNs + "balanceDetail",
                                    new XElement(ProdmlNs + "owner", bRow.BlnceSetOwner),
                                    new XElement(ProdmlNs + "sourceUnit"),
                                    new XElement(ProdmlNs + "share"),
                                    new XElement(ProdmlNs + "accountNumber"),
                                    new XElement(ProdmlNs + "event",
                                        new XElement(ProdmlNs + "date"),
                                        new XElement(ProdmlNs + "kind", bRow.BlnceSetEventKind)
                                    )
                                );

                                var bVolVal = new XElement(ProdmlNs + "volumeValue");
                                var bVolElem = new XElement(ProdmlNs + "volume", bRow.PeriodBlnceSetVolValue);
                                if (!string.IsNullOrEmpty(bRow.PeriodBlnceVolValueUom))
                                {
                                    bVolElem.Add(new XAttribute("uom", bRow.PeriodBlnceVolValueUom));
                                }
                                bVolVal.Add(bVolElem);
                                balanceDetailElem.Add(bVolVal);

                                if (!string.IsNullOrEmpty(bRow.PeriodCompContWork))
                                {
                                    var compContElem = new XElement(ProdmlNs + "componentContent");
                                    var workElem = new XElement(ProdmlNs + "work", bRow.PeriodCompContWork);
                                    if (!string.IsNullOrEmpty(bRow.PeriodCompContWorkUom))
                                    {
                                        workElem.Add(new XAttribute("uom", bRow.PeriodCompContWorkUom));
                                    }
                                    compContElem.Add(workElem);
                                    balanceDetailElem.Add(compContElem);
                                }

                                balanceSetElem.Add(balanceDetailElem);
                                periodElem.Add(balanceSetElem);
                            }

                            prodElem.Add(periodElem);
                        }

                        flowElem.Add(prodElem);
                    }

                    facilityElem.Add(flowElem);
                }

                volumeElem.Add(facilityElem);
            }

            root.Add(volumeElem);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            root
        );

        return doc.Declaration?.ToString() + Environment.NewLine + doc.ToString();
    }
}
