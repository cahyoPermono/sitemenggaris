namespace ExcelToXmlApi.Models;

public class SotProductionRow
{
    public string ProdVolumeName { get; set; } = string.Empty;
    public string ProdVolumeOperatorName { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public string FacilityKind { get; set; } = string.Empty;
    public string FacilityNamingSystem { get; set; } = string.Empty;
    public string FacilityParent1 { get; set; } = string.Empty;
    public string FacilityParent1Kind { get; set; } = string.Empty;
    public string FacilityParent2 { get; set; } = string.Empty;
    public string FacilityParent2Kind { get; set; } = string.Empty;
    public string FlowName { get; set; } = string.Empty;
    public string FlowKind { get; set; } = string.Empty;
    public string FlowDirection { get; set; } = string.Empty;
    public string FlowQualifier { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string PeriodKind { get; set; } = string.Empty;
    public string PeriodDate { get; set; } = string.Empty;
    public string PeriodVolumeValue { get; set; } = string.Empty;
    public string PeriodVolumeValueUom { get; set; } = string.Empty;
    public string PeriodBlnceSetKind { get; set; } = string.Empty;
    public string BlnceSetCargoNumber { get; set; } = string.Empty;
    public string BlnceSetCargoBatchNumber { get; set; } = string.Empty;
    public string BlnceSetShipper { get; set; } = string.Empty;
    public string BlnceSetBuyer { get; set; } = string.Empty;
    public string BlnceSetDestinationName { get; set; } = string.Empty;
    public string BlnceSetOwner { get; set; } = string.Empty;
    public string BlnceSetEventKind { get; set; } = string.Empty;
    public string PeriodBlnceSetVolValue { get; set; } = string.Empty;
    public string PeriodBlnceVolValueUom { get; set; } = string.Empty;
    public string PeriodCompContWork { get; set; } = string.Empty;
    public string PeriodCompContWorkUom { get; set; } = string.Empty;
}
