using System.Data;
using System.Text;
using System.Xml.Linq;
using ExcelDataReader;
using ExcelToXmlApi.Models;

namespace ExcelToXmlApi.Services;

public class SotProductionConverter : ISotProductionConverter
{
    public (string xmlContent, int rowCount) Convert(Stream excelStream, string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var rows = ParseExcelRows(excelStream);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("File Excel tidak memiliki data atau format tidak dikenali.");
        }

        var xmlString = GenerateProdMlXml(rows);
        return (xmlString, rows.Count);
    }

    private List<SotProductionRow> ParseExcelRows(Stream stream)
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

        var table = dataSet.Tables[0];
        if (table.Rows.Count < 2) return list;

        // Find header row (search first 5 rows for column names like 'prodVolumeName' or 'facilityName')
        int headerRowIndex = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int r = 0; r < Math.Min(5, table.Rows.Count); r++)
        {
            var tempMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var val = table.Rows[r][c]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    // Normalize: remove spaces and underscores for flexible match
                    var normalized = val.Replace(" ", "").Replace("_", "").ToLowerInvariant();
                    tempMap[normalized] = c;
                }
            }

            if (tempMap.ContainsKey("prodvolumename") || tempMap.ContainsKey("facilityname"))
            {
                headerRowIndex = r;
                colMap = tempMap;
                break;
            }
        }

        // If no named header found, fallback to row 0
        if (headerRowIndex == -1)
        {
            headerRowIndex = 0;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var val = table.Rows[0][c]?.ToString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    var normalized = val.Replace(" ", "").Replace("_", "").ToLowerInvariant();
                    colMap[normalized] = c;
                }
            }
        }

        string GetColValue(DataRow row, params string[] candidateKeys)
        {
            foreach (var key in candidateKeys)
            {
                var norm = key.Replace(" ", "").Replace("_", "").ToLowerInvariant();
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

            var item = new SotProductionRow
            {
                ProdVolumeName = GetColValue(dr, "prodVolumeName", "volumeName"),
                ProdVolumeOperatorName = GetColValue(dr, "prodVolumeOperatorName", "operatorName", "operator"),
                FacilityName = GetColValue(dr, "facilityName"),
                FacilityKind = GetColValue(dr, "facilityKind"),
                FacilityNamingSystem = GetColValue(dr, "facilityNamingSystem", "namingSystem"),
                FacilityParent1 = GetColValue(dr, "facilityParent1"),
                FacilityParent1Kind = GetColValue(dr, "facilityParent1Kind"),
                FacilityParent2 = GetColValue(dr, "facilityParent2"),
                FacilityParent2Kind = GetColValue(dr, "facilityParent2Kind"),
                FlowName = GetColValue(dr, "flowName"),
                FlowKind = GetColValue(dr, "flowKind"),
                FlowDirection = GetColValue(dr, "flowDirection"),
                FlowQualifier = GetColValue(dr, "flowQualifier"),
                ProductName = GetColValue(dr, "productName"),
                ProductKind = GetColValue(dr, "productKind"),
                PeriodKind = GetColValue(dr, "periodKind"),
                PeriodDate = GetColValue(dr, "periodDate"),
                PeriodVolumeValue = GetColValue(dr, "periodVolumeValue", "volumeValue", "volume"),
                PeriodVolumeValueUom = GetColValue(dr, "periodVolumeValueUom", "volumeUom", "uom"),
                PeriodBlnceSetKind = GetColValue(dr, "periodBlnceSetKind", "balanceSetKind"),
                BlnceSetCargoNumber = GetColValue(dr, "BlnceSetCargoNumber", "cargoNumber"),
                BlnceSetCargoBatchNumber = GetColValue(dr, "BlnceSetCargoBatchNumber", "cargoBatchNumber"),
                BlnceSetShipper = GetColValue(dr, "BlnceSetShipper", "shipper"),
                BlnceSetBuyer = GetColValue(dr, "BlnceSetBuyer", "buyer"),
                BlnceSetDestinationName = GetColValue(dr, "BlnceSetDestinationName", "destinationName", "destination"),
                BlnceSetOwner = GetColValue(dr, "BlnceSetOwner", "owner"),
                BlnceSetEventKind = GetColValue(dr, "BlnceSetEventKind", "eventKind"),
                PeriodBlnceSetVolValue = GetColValue(dr, "periodBlnceSetVolValue", "balanceVolumeValue"),
                PeriodBlnceVolValueUom = GetColValue(dr, "periodBlnceVolValueUom", "balanceVolumeUom"),
                PeriodCompContWork = GetColValue(dr, "periodCompContWork", "componentContentWork"),
                PeriodCompContWorkUom = GetColValue(dr, "periodCompContWorkUom", "componentContentWorkUom")
            };

            list.Add(item);
        }

        return list;
    }

    private string GenerateProdMlXml(List<SotProductionRow> rows)
    {
        // Root element <productVolumes>
        var root = new XElement("productVolumes",
            new XAttribute("version", "1.0"));

        // Group by prodVolumeName (or operator if empty)
        var volumeGroups = rows.GroupBy(r => string.IsNullOrEmpty(r.ProdVolumeName) ? "SOT_PRODUCTION" : r.ProdVolumeName);

        foreach (var volGroup in volumeGroups)
        {
            var firstRow = volGroup.First();
            var volumeElem = new XElement("productVolume",
                new XElement("name", volGroup.Key),
                new XElement("operator",
                    new XElement("name", firstRow.ProdVolumeOperatorName))
            );

            // Group by facilityName
            var facilityGroups = volGroup.GroupBy(r => r.FacilityName);
            foreach (var facGroup in facilityGroups)
            {
                var facFirst = facGroup.First();
                var facilityElem = new XElement("facility");

                var facNameElem = new XElement("name", facGroup.Key);
                if (!string.IsNullOrEmpty(facFirst.FacilityKind))
                {
                    facNameElem.Add(new XAttribute("kind", facFirst.FacilityKind));
                }
                facilityElem.Add(facNameElem);

                if (!string.IsNullOrEmpty(facFirst.FacilityNamingSystem))
                    facilityElem.Add(new XElement("namingSystem", facFirst.FacilityNamingSystem));

                if (!string.IsNullOrEmpty(facFirst.FacilityParent1))
                {
                    var parent1Elem = new XElement("facilityParent1", facFirst.FacilityParent1);
                    if (!string.IsNullOrEmpty(facFirst.FacilityParent1Kind))
                        parent1Elem.Add(new XAttribute("kind", facFirst.FacilityParent1Kind));
                    facilityElem.Add(parent1Elem);
                }

                if (!string.IsNullOrEmpty(facFirst.FacilityParent2))
                {
                    var parent2Elem = new XElement("facilityParent2", facFirst.FacilityParent2);
                    if (!string.IsNullOrEmpty(facFirst.FacilityParent2Kind))
                        parent2Elem.Add(new XAttribute("kind", facFirst.FacilityParent2Kind));
                    facilityElem.Add(parent2Elem);
                }

                // Group by flowName
                var flowGroups = facGroup.GroupBy(r => r.FlowName);
                foreach (var flowGroup in flowGroups)
                {
                    var flowFirst = flowGroup.First();
                    var flowElem = new XElement("flow",
                        new XElement("name", flowGroup.Key),
                        new XElement("kind", flowFirst.FlowKind),
                        new XElement("direction", flowFirst.FlowDirection),
                        new XElement("qualifier", flowFirst.FlowQualifier)
                    );

                    // Group by productName
                    var productGroups = flowGroup.GroupBy(r => r.ProductName);
                    foreach (var prodGroup in productGroups)
                    {
                        var prodFirst = prodGroup.First();
                        var prodElem = new XElement("product",
                            new XElement("name", prodGroup.Key),
                            new XElement("kind", prodFirst.ProductKind)
                        );

                        // Group by period (date & kind)
                        var periodGroups = prodGroup.GroupBy(r => new { r.PeriodDate, r.PeriodKind });
                        foreach (var perGroup in periodGroups)
                        {
                            var perFirst = perGroup.First();
                            var periodElem = new XElement("period",
                                new XElement("kind", perGroup.Key.PeriodKind),
                                new XElement("date", perGroup.Key.PeriodDate)
                            );

                            var volElem = new XElement("volume", perFirst.PeriodVolumeValue);
                            if (!string.IsNullOrEmpty(perFirst.PeriodVolumeValueUom))
                                volElem.Add(new XAttribute("uom", perFirst.PeriodVolumeValueUom));
                            periodElem.Add(volElem);

                            // Balance sets
                            foreach (var bRow in perGroup)
                            {
                                if (string.IsNullOrEmpty(bRow.PeriodBlnceSetKind) &&
                                    string.IsNullOrEmpty(bRow.BlnceSetCargoNumber) &&
                                    string.IsNullOrEmpty(bRow.BlnceSetOwner))
                                {
                                    continue;
                                }

                                var balanceSetElem = new XElement("balanceSet",
                                    new XElement("kind", bRow.PeriodBlnceSetKind),
                                    new XElement("cargoNumber", bRow.BlnceSetCargoNumber),
                                    new XElement("cargoBatchNumber", bRow.BlnceSetCargoBatchNumber),
                                    new XElement("shipper", bRow.BlnceSetShipper),
                                    new XElement("buyer", bRow.BlnceSetBuyer),
                                    new XElement("destination",
                                        new XElement("name", bRow.BlnceSetDestinationName))
                                );

                                var balanceDetailElem = new XElement("balanceDetail",
                                    new XElement("owner", bRow.BlnceSetOwner),
                                    new XElement("event",
                                        new XElement("kind", bRow.BlnceSetEventKind))
                                );

                                var bVolElem = new XElement("volumeValue", bRow.PeriodBlnceSetVolValue);
                                if (!string.IsNullOrEmpty(bRow.PeriodBlnceVolValueUom))
                                    bVolElem.Add(new XAttribute("uom", bRow.PeriodBlnceVolValueUom));
                                balanceDetailElem.Add(bVolElem);

                                var workElem = new XElement("work", bRow.PeriodCompContWork);
                                if (!string.IsNullOrEmpty(bRow.PeriodCompContWorkUom))
                                    workElem.Add(new XAttribute("uom", bRow.PeriodCompContWorkUom));
                                balanceDetailElem.Add(new XElement("componentContent", workElem));

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
            new XDeclaration("1.0", "utf-8", "yes"),
            root
        );

        return doc.Declaration?.ToString() + Environment.NewLine + doc.ToString();
    }
}
