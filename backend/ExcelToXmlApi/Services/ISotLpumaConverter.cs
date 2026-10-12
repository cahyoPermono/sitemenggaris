namespace ExcelToXmlApi.Services;

public interface ISotLpumaConverter
{
    (string xmlContent, int rowCount) Convert(Stream excelStream, string fileName);
}
