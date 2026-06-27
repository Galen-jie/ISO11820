using ISO11820.Data.Models;
using ISO11820.Services;

namespace ISO11820.Forms;

public partial class TestDetailForm : Form
{
    private readonly TestMasterRecord _test;
    private Button btnExport = null!, btnClose = null!;
    private DataGridView dgvDetail = null!;

    public TestDetailForm(TestMasterRecord test)
    {
        _test = test;
        InitializeComponent();
        LoadTestDetails();
    }

    private void InitializeComponent()
    {
        this.Text = $"试验详情 - {_test.TestId}";
        this.Size = new Size(550, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        dgvDetail = new DataGridView { Location = new Point(10, 10), Size = new Size(530, 350), AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, SelectionMode = DataGridViewSelectionMode.CellSelect };

        btnExport = new Button { Text = "导出报告", Location = new Point(170, 380), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
        btnExport.Click += BtnExport_Click;

        btnClose = new Button { Text = "关闭", Location = new Point(290, 380), Size = new Size(100, 35), Font = new Font("Microsoft YaHei", 10) };
        btnClose.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] { dgvDetail, btnExport, btnClose });
    }

    private void LoadTestDetails()
    {
        var details = new List<(string Item, string Value)>
        {
            ("试验编号", _test.TestId),
            ("样品编号", _test.ProductId),
            ("样品名称", _test.ProductName),
            ("试验日期", _test.TestDate.ToString("yyyy-MM-dd HH:mm")),
            ("操作员", _test.Operator),
            ("试验前重量", $"{_test.PreWeight:F2} g"),
            ("试验后重量", $"{_test.PostWeight:F2} g"),
            ("失重", $"{_test.LostWeight:F2} g"),
            ("失重率", $"{_test.LostWeightPer:F2} %"),
            ("总时间", $"{_test.TotalTestTime} s"),
            ("温升", $"{_test.DeltaTf:F1} C"),
            ("火焰", _test.FlameTime > 0 ? $"有, {_test.FlameDuration}s" : "无"),
            ("判定结果", TemperatureCalculator.GetJudgmentText(_test.DeltaTf, _test.LostWeightPer, _test.FlameDuration))
        };
        dgvDetail.DataSource = details.Select(d => new { 项目 = d.Item, 值 = d.Value }).ToList();
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        var exportService = new ExportService();
        var csvPath = exportService.GetExistingCsvPath(_test.ProductId, _test.TestId);

        List<Core.SensorDataPoint> data = new();
        if (csvPath != null) data = exportService.LoadFromCsv(csvPath);

        try
        {
            var excelPath = exportService.ExportToExcel(_test, data);
            MessageBox.Show($"报告已导出: {excelPath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}