namespace ExcelToXmlApi.Services;

public class LpoEnumValidator
{
    public static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Planned",
        "Unplanned",
        "Operational Activity",
        "Normal Operation"
    };

    public static readonly HashSet<string> ValidFacilityKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "Field",
        "Area",
        "Train",
        "Platform",
        "All"
    };

    public static readonly HashSet<string> ValidCauseSourceLevel1 = new(StringComparer.OrdinalIgnoreCase)
    {
        "Wells",
        "Facilities",
        "Wellwork Activity",
        "Offtaker",
        "External",
        "Regulatory Compliance",
        "Human Error",
        "Operational Activity",
        "Normal Operation",
        "Export"
    };

    public static readonly HashSet<string> ValidSourceLevel2 = new(StringComparer.OrdinalIgnoreCase)
    {
        "Wellhead Equipment",
        "Downhole Equipment",
        "Artificial Lift",
        "Reservoir",
        "Instrument & Control",
        "Electrical",
        "Engine/Prime Mover",
        "Compressor",
        "Heat Exchanger",
        "Surface Pumps",
        "Vessels",
        "Platform/Structure",
        "Pipings & Valves",
        "Pipelines",
        "Marine",
        "Transport",
        "Export Pipelines",
        "Offtaker",
        "Operational Activity",
        "Normal Operation",
        "Turn Around/Integrated Shutdown"
    };

    public static readonly HashSet<string> ValidCauseLevel2 = new(StringComparer.OrdinalIgnoreCase)
    {
        "Wax/Scale",
        "High Water Cut",
        "High GOR",
        "Liquid Loading",
        "Sand",
        "Low influx",
        "Wellhead Equipment Failure",
        "Downhole Equipment Failure",
        "Artificial Lift Failure",
        "Gas Lift Source",
        "Mechanical Rotating Failure",
        "Mechanical Static Failure",
        "Electrical Failure",
        "Instrument Failure",
        "System Restriction",
        "Contaminant/Process Upset",
        "Wells Maintenance",
        "Drilling Activity",
        "WO/WS Activity",
        "Force Majeur",
        "Weather",
        "Animal Disturbance",
        "Social & Security",
        "3rd Party",
        "HSE Related",
        "Flaring",
        "Emissions",
        "Water Discharge",
        "Permit",
        "Human Error",
        "Operational Activity",
        "Normal Operation",
        "Project and Modification",
        "Maintenance",
        "Offtaker",
        "Heavy Oil Issue",
        "Capacity Constraint"
    };

    public static string? MatchEnum(HashSet<string> set, string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var trimmed = input.Trim();
        foreach (var val in set)
        {
            if (string.Equals(val, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return val; // Kembalikan canonical casing sesuai XSD
            }
        }
        return null;
    }

    public static List<string> Validate(List<Models.SotLpumaRow> rows)
    {
        var warnings = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int rowNumber = i + 2; // Asumsi row 1 adalah header

            // Validate Category
            if (!string.IsNullOrWhiteSpace(row.Category))
            {
                var canonical = MatchEnum(ValidCategories, row.Category);
                if (canonical != null)
                {
                    row.Category = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: Kategori '{row.Category}' tidak valid menurut skema LPO (pilihan: {string.Join(", ", ValidCategories)}).");
                }
            }

            // Validate FacilityKind
            if (!string.IsNullOrWhiteSpace(row.FacilityKind))
            {
                var canonical = MatchEnum(ValidFacilityKinds, row.FacilityKind);
                if (canonical != null)
                {
                    row.FacilityKind = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: Facility Type '{row.FacilityKind}' tidak valid menurut skema LPO (pilihan: {string.Join(", ", ValidFacilityKinds)}).");
                }
            }

            // Validate SystemSourceLevel1
            if (!string.IsNullOrWhiteSpace(row.SystemSourceLevel1))
            {
                var canonical = MatchEnum(ValidCauseSourceLevel1, row.SystemSourceLevel1);
                if (canonical != null)
                {
                    row.SystemSourceLevel1 = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: System Source Level 1 '{row.SystemSourceLevel1}' tidak terdaftar di skema LPO.");
                }
            }

            // Validate SystemSourceLevel2
            if (!string.IsNullOrWhiteSpace(row.SystemSourceLevel2))
            {
                var canonical = MatchEnum(ValidSourceLevel2, row.SystemSourceLevel2);
                if (canonical != null)
                {
                    row.SystemSourceLevel2 = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: System Source Level 2 '{row.SystemSourceLevel2}' tidak terdaftar di skema LPO.");
                }
            }

            // Validate SystemCauseLevel1
            if (!string.IsNullOrWhiteSpace(row.SystemCauseLevel1))
            {
                var canonical = MatchEnum(ValidCauseSourceLevel1, row.SystemCauseLevel1);
                if (canonical != null)
                {
                    row.SystemCauseLevel1 = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: System Cause Level 1 '{row.SystemCauseLevel1}' tidak terdaftar di skema LPO.");
                }
            }

            // Validate SystemCauseLevel2
            if (!string.IsNullOrWhiteSpace(row.SystemCauseLevel2))
            {
                var canonical = MatchEnum(ValidCauseLevel2, row.SystemCauseLevel2);
                if (canonical != null)
                {
                    row.SystemCauseLevel2 = canonical;
                }
                else
                {
                    warnings.Add($"Baris {rowNumber}: System Cause Level 2 '{row.SystemCauseLevel2}' tidak terdaftar di skema LPO.");
                }
            }
        }

        return warnings;
    }
}
