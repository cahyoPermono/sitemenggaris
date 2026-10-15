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
            "facilityParent1", "facilityParent2", "contextFacility", "flowName", "flowKind",
            "flowDirection", "flowQualifier", "productName", "productKind", "periodKind",
            "periodDate", "periodVolumeValue", "periodVolumeValueUom", "periodBlnceSetKind", "BlnceSetCargoNumber",
            "BlnceSetCargoBatchNumber", "BlnceSetShipper", "BlnceSetDestinationName", "BlnceSetOwner", "BlnceSetEventKind",
            "periodBlnceSetVolValue", "periodBlnceVolValueUom", "periodCompContWork", "periodCompContWorkUom", "BlnceSetBuyer"
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

        // Add sample data rows matching real TDM data
        var sampleRows = new[]
        {
            new object[]
            {
                "JOB PMEP SIMENGGARIS DAILY PRODUCTION REPORT", "JOB PERTAMINA - MEDCO E&P SIMENGGARIS", "TERMINAL BUNYU", "organizational unit", "TDM Jakarta",
                "SIMENGGARIS", "JOB PERTAMINA MEDCO SIMENGGARIS", "", "ACTUAL LIFTING WITH PPL", "sale",
                "outlet", "derived", "SOUTH SEMBAKUNG CONDENSATE", "condensate", "day",
                "2026-02-16", "0", "BBLS", "lifted", "",
                "", "", "", "", "",
                "0", "BBLS", "", "", ""
            },
            new object[]
            {
                "JOB PMEP SIMENGGARIS DAILY PRODUCTION REPORT", "JOB PERTAMINA - MEDCO E&P SIMENGGARIS", "SOUTH SEMBAKUNG", "field", "TDM Jakarta",
                "SIMENGGARIS", "JOB PERTAMINA MEDCO SIMENGGARIS", "", "SOUTH SEMBAKUNG CONDENSATE PRODUCTION", "production",
                "outlet", "derived", "SOUTH SEMBAKUNG CONDENSATE", "condensate", "day",
                "2026-02-16", "19.620000", "BBLS", "", "",
                "", "", "", "", "",
                "", "", "", "", ""
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
        var ws = workbook.Worksheets.Add("LPO");

        var headers = new[]
        {
            "KKKS", "WK", "[Field/Area/Train/Platform/All]", "[Field/Area/Train/Platform/All] Name", "Tanggal",
            "Hari ke-", "Start Time", "End Time", "Event ID", "Kategori",
            "System Source Level 1", "System Source Level 2", "System Cause Level1", "System Cause Level2", "Kejadian/Kegiatan",
            "Update Progress", "Jumlah Sumur OFF", "Est. Tanggal Selesai", "Volume LPO Minyak (BBLS)", "Volume LPO Gas (MMSCF)"
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

        // Add sample data rows matching official SKK Migas format with valid enums
        var sampleRows = new[]
        {
            new object[]
            {
                "JOB PERTAMINA MEDCO SIMENGGARIS", "WK00000391", "Field", "South Sembakung", "2026-08-23",
                "21", "2026-08-23 00:00:00", "2026-08-23 23:59:59", "MEPS-U-2026-35", "Unplanned",
                "Wells", "Reservoir", "Wellwork Activity", "WO/WS Activity", "Well SSB-2LS Shut in due to WO/WS",
                "Masih Berlanjut", "1", "2026-08-31", "0.000000", "1.585000"
            },
            new object[]
            {
                "JOB PERTAMINA MEDCO SIMENGGARIS", "WK00000391", "Field", "South Sembakung", "2026-08-23",
                "17", "2026-08-23 00:00:00", "2026-08-23 23:59:59", "MEPS-U-2026-40", "Unplanned",
                "Offtaker", "Offtaker", "Offtaker", "Offtaker", "SSB-8 LS shut in due to operational disruption on the buyer (KAYAN)",
                "Masih Berlanjut", "1", "2026-08-27", "17.105143", "0.780837"
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

        // Sheet 2: Referensi Enum SKK Migas
        var wsRef = workbook.Worksheets.Add("Referensi Enum");
        
        void PopulateRefColumn(int col, string title, IEnumerable<string> items)
        {
            var cell = wsRef.Cell(1, col);
            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
            
            int row = 2;
            foreach (var item in items)
            {
                wsRef.Cell(row++, col).Value = item;
            }
        }

        PopulateRefColumn(1, "Kategori", LpoEnumValidator.ValidCategories);
        PopulateRefColumn(2, "Facility Type", LpoEnumValidator.ValidFacilityKinds);
        PopulateRefColumn(3, "Cause / Source Level 1", LpoEnumValidator.ValidCauseSourceLevel1);
        PopulateRefColumn(4, "Source Level 2", LpoEnumValidator.ValidSourceLevel2);
        PopulateRefColumn(5, "Cause Level 2", LpoEnumValidator.ValidCauseLevel2);

        wsRef.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
