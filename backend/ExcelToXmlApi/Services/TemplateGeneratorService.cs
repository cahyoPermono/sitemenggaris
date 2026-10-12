using ClosedXML.Excel;

namespace ExcelToXmlApi.Services;

public class TemplateGeneratorService
{
    public byte[] GenerateSotProductionTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("SOT_Production");

        var headers = new[]
        {
            "prodVolumeName", "prodVolumeOperatorName", "facilityName", "facilityKind", "facilityNamingSystem",
            "facilityParent1", "facilityParent1Kind", "facilityParent2", "facilityParent2Kind", "flowName",
            "flowKind", "flowDirection", "flowQualifier", "productName", "productKind", "periodKind",
            "periodDate", "periodVolumeValue", "periodVolumeValueUom", "periodBlnceSetKind", "BlnceSetCargoNumber",
            "BlnceSetCargoBatchNumber", "BlnceSetShipper", "BlnceSetBuyer", "BlnceSetDestinationName", "BlnceSetOwner",
            "BlnceSetEventKind", "periodBlnceSetVolValue", "periodBlnceVolValueUom", "periodCompContWork", "periodCompContWorkUom"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F4C81");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Add 2 sample data rows
        var sampleRows = new[]
        {
            new object[]
            {
                "VOL_PROD_MENGGARIS_20261012", "Pertamina EP", "Menggaris Central Station", "Field", "SKK_STANDARD",
                "Menggaris Block", "Block", "North Kalimantan", "Region", "FLOW_OIL_EXPORT_01",
                "production", "outflow", "allocated", "Crude Oil", "oil", "day",
                "2026-10-12", "1250.75", "bbl", "Cargo", "CRG-2026-001",
                "BATCH-01", "PHE Menggaris", "PT Kilang Pertamina Internasional", "Kilang Balikpapan", "KKKS Menggaris",
                "lifting", "1250.75", "bbl", "Crude Oil Processing", "bbl"
            },
            new object[]
            {
                "VOL_PROD_MENGGARIS_20261012", "Pertamina EP", "Menggaris Central Station", "Field", "SKK_STANDARD",
                "Menggaris Block", "Block", "North Kalimantan", "Region", "FLOW_GAS_SALES_02",
                "production", "outflow", "allocated", "Natural Gas", "gas", "day",
                "2026-10-12", "15.40", "MMSCF", "Pipeline", "PL-2026-088",
                "BATCH-02", "PHE Menggaris", "PT PLN Nusantara Power", "PLTMG Menggaris", "KKKS Menggaris",
                "sales", "15.40", "MMSCF", "Gas Conditioning", "MMSCF"
            }
        };

        for (int r = 0; r < sampleRows.Length; r++)
        {
            for (int c = 0; c < sampleRows[r].Length; c++)
            {
                ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(sampleRows[r][c]);
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] GenerateSotLpumaTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("SOT_L-PUMA");

        var headers = new[]
        {
            "By sistem KKKS", "documentDate", "KKKS", "WK", "[Field/Area/Train/Platform/All]",
            "[Field/Area/Train/Platform/All] Name", "Tanggal", "Hari ke-", "Start Time", "End Time",
            "eventID", "Kategori", "System Source Level 1", "System Source Level 2", "System Cause Level1",
            "System Cause Level2", "Kejadian/Kegiatan", "Update Progress", "Jumlah Sumur OFF", "Est. Tanggal Selesai",
            "Volume LPO Minyak (BBLS)", "Volume LPO Gas (MMSCF)"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0066CC");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Add 2 sample data rows
        var sampleRows = new[]
        {
            new object[]
            {
                "DOC_LPO_20261012_001", "2026-10-12", "Pertamina Hulu Rokan", "WK Rokan", "Field",
                "Duri Field", "2026-10-12", "1", "08:30:00", "14:45:00",
                "EVT-SHUT-2026-091", "Unplanned", "Surface Facilities", "Pumping System", "Mechanical Failure",
                "Motor Bearing Damaged", "Perbaikan pompa transfer utama P-101", "Bearing diganti dan pengujian getaran normal", "3", "2026-10-12",
                "185.50", "0.25"
            },
            new object[]
            {
                "DOC_LPO_20261012_001", "2026-10-12", "Pertamina Hulu Rokan", "WK Rokan", "Platform",
                "Bekasap Platform A", "2026-10-12", "2", "06:00:00", "18:00:00",
                "EVT-SHUT-2026-092", "Planned", "Pipeline System", "Trunkline 12 inch", "Integrity Inspection",
                "Pigging Operation", "Pembersihan jalur pipa utama dan ILI tool inspection", "Inspection run 100% completed", "5", "2026-10-13",
                "420.00", "1.10"
            }
        };

        for (int r = 0; r < sampleRows.Length; r++)
        {
            for (int c = 0; c < sampleRows[r].Length; c++)
            {
                ws.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(sampleRows[r][c]);
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
