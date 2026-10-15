namespace ExcelToXmlApi.Services;

public interface ISotProductionConverter
{
    (string xmlContent, int rowCount, List<string> warnings) Convert(Stream excelStream, string fileName);
}
