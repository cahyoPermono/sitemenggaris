namespace ExcelToXmlApi.Services;

public interface ISotProductionConverter
{
    (string xmlContent, int rowCount) Convert(Stream excelStream, string fileName);
}
