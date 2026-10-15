namespace ExcelToXmlApi.Services;

public interface ISotLpumaConverter
{
    (string xmlContent, int rowCount, List<string> warnings) Convert(Stream excelStream, string fileName);
}
