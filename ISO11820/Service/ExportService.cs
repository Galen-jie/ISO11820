using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using ISO11820.Core;
using ISO11820.Data.Models;
using ISO11820.Global;

namespace ISO11820.Services;

public class ExportService
{
    private readonly string _baseDir;
    private readonly string _reportDir;

    public ExportService()
    {
        _baseDir = MyAppContext.Instance.Config.FileStorage.BaseDirectory;
        _reportDir = MyAppContext.Instance.Config.Report.OutputDirectory;
    }

    public string? GetExistingCsvPath(string productId, string testId)
    {
        var filePath = Path.Combine(_baseDir, "TestData", productId, testId, "sensor_data.csv");
        return File.Exists(filePath) ? filePath : null;
    }

    public string ExportToCsv(string productId, string testId, List<SensorDataPoint> data)
    {
        var dir = Path.Combine(_baseDir, "TestData", productId, testId);
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, "sensor_data.csv");

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("Time,Temp1,Temp2,TempSurface,TempCenter,TempCalibration");
        foreach (var point in data)
            writer.WriteLine($"{point.Time},{point.Tf1:F1},{point.Tf2:F1},{point.Ts:F1},{point.Tc:F1},{point.Tcal:F1}");
        return filePath;
    }

    public string ExportToExcel(TestMasterRecord record, List<SensorDataPoint> data)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var filePath = Path.Combine(_reportDir, $"{record.TestId}_report.xlsx");
        Directory.CreateDirectory(_reportDir);

        using var package = new ExcelPackage();
        var infoSheet = package.Workbook.Worksheets.Add("Test Info");
        PopulateTestInfo(infoSheet, record);

        var dataSheet = package.Workbook.Worksheets.Add("Temperature Data");
        PopulateTemperatureData(dataSheet, data);

        package.SaveAs(new FileInfo(filePath));
        return filePath;
    }

    private void PopulateTestInfo(ExcelWorksheet sheet, TestMasterRecord record)
    {
        sheet.Column(1).Width = 20;
        sheet.Column(2).Width = 30;

        int row = 1;
        AddInfoRow(sheet, row++, "Test ID", record.TestId);
        AddInfoRow(sheet, row++, "Product ID", record.ProductId);
        AddInfoRow(sheet, row++, "Product Name", record.ProductName);
        AddInfoRow(sheet, row++, "Test Date", record.TestDate.ToString("yyyy-MM-dd"));
        AddInfoRow(sheet, row++, "Operator", record.Operator);
        AddInfoRow(sheet, row++, "Pre-weight (g)", record.PreWeight.ToString("F2"));
        AddInfoRow(sheet, row++, "Post-weight (g)", record.PostWeight.ToString("F2"));
        AddInfoRow(sheet, row++, "Weight Loss (%)", record.LostWeightPer.ToString("F2"));
        AddInfoRow(sheet, row++, "Temperature Rise (C)", record.DeltaTf.ToString("F1"));
        AddInfoRow(sheet, row++, "Total Time (s)", record.TotalTestTime.ToString());

        row++;
        var judgment = TemperatureCalculator.GetJudgmentText(record.DeltaTf, record.LostWeightPer, record.FlameDuration);
        AddInfoRow(sheet, row, "Result", judgment);
    }

    private void AddInfoRow(ExcelWorksheet sheet, int row, string label, string value)
    {
        sheet.Cells[row, 1].Value = label;
        sheet.Cells[row, 2].Value = value;
        sheet.Cells[row, 1].Style.Font.Bold = true;
    }

    private void PopulateTemperatureData(ExcelWorksheet sheet, List<SensorDataPoint> data)
    {
        sheet.Cells[1, 1].Value = "Time(s)";
        sheet.Cells[1, 2].Value = "TF1(C)";
        sheet.Cells[1, 3].Value = "TF2(C)";
        sheet.Cells[1, 4].Value = "TS(C)";
        sheet.Cells[1, 5].Value = "TC(C)";

        for (int col = 1; col <= 5; col++) sheet.Cells[1, col].Style.Font.Bold = true;

        for (int i = 0; i < data.Count; i++)
        {
            int row = i + 2;
            var point = data[i];
            sheet.Cells[row, 1].Value = point.Time;
            sheet.Cells[row, 2].Value = point.Tf1;
            sheet.Cells[row, 3].Value = point.Tf2;
            sheet.Cells[row, 4].Value = point.Ts;
            sheet.Cells[row, 5].Value = point.Tc;
        }
    }

    public string ExportToPdf(TestMasterRecord record, List<SensorDataPoint> data)
    {
        var filePath = Path.Combine(_reportDir, $"{record.TestId}_report.pdf");
        Directory.CreateDirectory(_reportDir);

        var document = new PdfDocument();
        document.Info.Title = $"Test Report - {record.TestId}";
        var page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
        gfx.DrawString("ISO 11820 Test Report", titleFont, XBrushes.Black, new XRect(0, 40, page.Width.Point, 30), XStringFormats.Center);

        var infoFont = new XFont("Arial", 11, XFontStyleEx.Regular);
        int y = 80;

        gfx.DrawString($"Test ID: {record.TestId}", infoFont, XBrushes.Black, 50, y); y += 22;
        gfx.DrawString($"Product: {record.ProductId} - {record.ProductName}", infoFont, XBrushes.Black, 50, y); y += 22;
        gfx.DrawString($"Date: {record.TestDate:yyyy-MM-dd}", infoFont, XBrushes.Black, 50, y); y += 22;
        gfx.DrawString($"Operator: {record.Operator}", infoFont, XBrushes.Black, 50, y); y += 22;
        gfx.DrawString($"Weight Loss: {record.LostWeightPer:F2}%", infoFont, XBrushes.Black, 50, y); y += 22;
        gfx.DrawString($"Temperature Rise: {record.DeltaTf:F1}C", infoFont, XBrushes.Black, 50, y); y += 22;

        y += 20;
        var judgment = TemperatureCalculator.GetJudgmentText(record.DeltaTf, record.LostWeightPer, record.FlameDuration);
        var resultFont = new XFont("Arial", 14, XFontStyleEx.Bold);
        gfx.DrawString($"Result: {judgment}", resultFont, judgment == "合格" ? XBrushes.Green : XBrushes.Red, 50, y);

        document.Save(filePath);
        return filePath;
    }

    public List<SensorDataPoint> LoadFromCsv(string csvPath)
    {
        var data = new List<SensorDataPoint>();
        if (!File.Exists(csvPath)) return data;

        using var reader = new StreamReader(csvPath);
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split(',');
            if (parts.Length >= 6)
            {
                data.Add(new SensorDataPoint
                {
                    Time = int.Parse(parts[0]),
                    Tf1 = double.Parse(parts[1]),
                    Tf2 = double.Parse(parts[2]),
                    Ts = double.Parse(parts[3]),
                    Tc = double.Parse(parts[4]),
                    Tcal = double.Parse(parts[5])
                });
            }
        }
        return data;
    }
}
