using ISO11820.Global;

namespace ISO11820.Forms;

public partial class CalibrationForm : Form
{
    private ComboBox cmbCalibrationType = null!;
    private TextBox txtTempA1 = null!, txtTempA2 = null!, txtTempA3 = null!;
    private TextBox txtTempB1 = null!, txtTempB2 = null!, txtTempB3 = null!;
    private TextBox txtTempC1 = null!, txtTempC2 = null!, txtTempC3 = null!;
    private TextBox txtRemarks = null!;
    private Label lblResult = null!;
    private Button btnSave = null!, btnCancel = null!;

    public CalibrationForm() { InitializeComponent(); }

    private void InitializeComponent()
    {
        this.Text = "校准记录";
        this.Size = new Size(400, 350);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var font = new Font("Microsoft YaHei", 10);
        int y = 20, xLbl = 20, xInp = 120;

        AddLabel("校准类型", xLbl, y);
        cmbCalibrationType = new ComboBox { Location = new Point(xInp, y), Size = new Size(150, 25), Font = font, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbCalibrationType.Items.AddRange(new object[] { "表面", "中心" });
        cmbCalibrationType.SelectedIndex = 0; y += 40;

        AddLabel("A点 (C)", xLbl, y);
        txtTempA1 = new TextBox { Location = new Point(xInp, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempA2 = new TextBox { Location = new Point(xInp + 70, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempA3 = new TextBox { Location = new Point(xInp + 140, y), Size = new Size(60, 25), Font = font, Text = "750" }; y += 35;

        AddLabel("B点 (C)", xLbl, y);
        txtTempB1 = new TextBox { Location = new Point(xInp, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempB2 = new TextBox { Location = new Point(xInp + 70, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempB3 = new TextBox { Location = new Point(xInp + 140, y), Size = new Size(60, 25), Font = font, Text = "750" }; y += 35;

        AddLabel("C点 (C)", xLbl, y);
        txtTempC1 = new TextBox { Location = new Point(xInp, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempC2 = new TextBox { Location = new Point(xInp + 70, y), Size = new Size(60, 25), Font = font, Text = "750" };
        txtTempC3 = new TextBox { Location = new Point(xInp + 140, y), Size = new Size(60, 25), Font = font, Text = "750" }; y += 50;

        var btnCalculate = new Button { Text = "计算", Location = new Point(xInp, y), Size = new Size(100, 30), Font = font };
        btnCalculate.Click += BtnCalculate_Click; y += 35;

        AddLabel("结果", xLbl, y);
        lblResult = new Label { Location = new Point(xInp, y), Size = new Size(250, 25), Font = font, ForeColor = Color.Green }; y += 40;

        AddLabel("备注", xLbl, y);
        txtRemarks = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 50), Font = font, Multiline = true }; y += 60;

        btnSave = new Button { Text = "保存", Location = new Point(100, y), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button { Text = "取消", Location = new Point(220, y), Size = new Size(100, 35), Font = font };
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] {
            cmbCalibrationType, txtTempA1, txtTempA2, txtTempA3, txtTempB1, txtTempB2, txtTempB3, txtTempC1, txtTempC2, txtTempC3,
            btnCalculate, lblResult, txtRemarks, btnSave, btnCancel
        });
    }

    private void AddLabel(string text, int x, int y) { this.Controls.Add(new Label { Text = text, Location = new Point(x, y), Size = new Size(100, 25), Font = new Font("Microsoft YaHei", 9) }); }

    private void BtnCalculate_Click(object? sender, EventArgs e)
    {
        var temps = GetTemperatures();
        if (temps == null) return;
        double avg = temps.Average();
        double maxDev = temps.Max(t => Math.Abs(t - avg));
        lblResult.Text = $"平均: {avg:F2}C, 最大偏差: {maxDev:F2}C";
        lblResult.ForeColor = maxDev <= 5 ? Color.Green : Color.Red;
    }

    private List<double>? GetTemperatures()
    {
        try
        {
            return new List<double> {
                double.Parse(txtTempA1.Text), double.Parse(txtTempA2.Text), double.Parse(txtTempA3.Text),
                double.Parse(txtTempB1.Text), double.Parse(txtTempB2.Text), double.Parse(txtTempB3.Text),
                double.Parse(txtTempC1.Text), double.Parse(txtTempC2.Text), double.Parse(txtTempC3.Text)
            };
        }
        catch { MessageBox.Show("无效的温度值", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var temps = GetTemperatures();
        if (temps == null) return;

        double avg = temps.Average();
        double maxDev = temps.Max(t => Math.Abs(t - avg));
        bool passed = maxDev <= 5;

        var record = new Data.Models.CalibrationRecord
        {
            Id = Guid.NewGuid().ToString(),
            CalibrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            CalibrationType = cmbCalibrationType.SelectedItem?.ToString() ?? "Surface",
            ApparatusId = 0,
            Operator = MyAppContext.Instance.CurrentOperator,
            TemperatureData = System.Text.Json.JsonSerializer.Serialize(temps),
            AverageTemperature = avg,
            MaxDeviation = maxDev,
            PassedCriteria = passed ? 1 : 0,
            Remarks = txtRemarks.Text,
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            TempA1 = temps[0], TempA2 = temps[1], TempA3 = temps[2],
            TempB1 = temps[3], TempB2 = temps[4], TempB3 = temps[5],
            TempC1 = temps[6], TempC2 = temps[7], TempC3 = temps[8],
            TAvg = avg
        };

        MyAppContext.Instance.DbHelper.InsertCalibrationRecord(record);
        MessageBox.Show("校准记录已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
    }
}