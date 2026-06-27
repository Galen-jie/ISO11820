using ISO11820.Global;

namespace ISO11820.Forms;

public partial class SettingsForm : Form
{
    private TextBox txtHeatingRate = null!, txtTargetTemp = null!, txtTempFluctuation = null!;
    private TextBox txtStableThreshold = null!, txtStableCount = null!, txtMaxDrift = null!;
    private Button btnSave = null!, btnCancel = null!;

    public SettingsForm() { InitializeComponent(); LoadCurrentSettings(); }

    private void InitializeComponent()
    {
        this.Text = "系统设置";
        this.Size = new Size(350, 300);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var font = new Font("Microsoft YaHei", 10);
        int y = 20, xLbl = 20, xInp = 150;

        AddLabel("加热速率(C/s)", xLbl, y);
        txtHeatingRate = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 35;

        AddLabel("目标温度(C)", xLbl, y);
        txtTargetTemp = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 35;

        AddLabel("温度波动(C)", xLbl, y);
        txtTempFluctuation = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 35;

        AddLabel("稳定阈值(C)", xLbl, y);
        txtStableThreshold = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 35;

        AddLabel("稳定计数", xLbl, y);
        txtStableCount = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 35;

        AddLabel("最大漂移(C/10min)", xLbl, y);
        txtMaxDrift = new TextBox { Location = new Point(xInp, y), Size = new Size(100, 25), Font = font }; y += 50;

        btnSave = new Button { Text = "保存", Location = new Point(80, y), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button { Text = "取消", Location = new Point(200, y), Size = new Size(100, 35), Font = font };
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { txtHeatingRate, txtTargetTemp, txtTempFluctuation, txtStableThreshold, txtStableCount, txtMaxDrift, btnSave, btnCancel });
    }

    private void AddLabel(string text, int x, int y) { this.Controls.Add(new Label { Text = text, Location = new Point(x, y), Size = new Size(130, 25), Font = new Font("Microsoft YaHei", 9) }); }

    private void LoadCurrentSettings()
    {
        var config = MyAppContext.Instance.Config;
        txtHeatingRate.Text = config.Simulation.HeatingRatePerSecond.ToString();
        txtTargetTemp.Text = config.Simulation.TargetFurnaceTemp.ToString();
        txtTempFluctuation.Text = config.Simulation.TempFluctuation.ToString();
        txtStableThreshold.Text = config.Simulation.StableThreshold.ToString();
        txtStableCount.Text = config.Simulation.StableCountThreshold.ToString();
        txtMaxDrift.Text = config.Hardware.MaxTemperatureDriftPerTenMinutes.ToString();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        try
        {
            var json = File.ReadAllText(configPath);
            var jsonObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonNode>(json);
            if (jsonObj != null)
            {
                jsonObj["Simulation"]!["HeatingRatePerSecond"] = double.Parse(txtHeatingRate.Text);
                jsonObj["Simulation"]!["TargetFurnaceTemp"] = double.Parse(txtTargetTemp.Text);
                jsonObj["Simulation"]!["TempFluctuation"] = double.Parse(txtTempFluctuation.Text);
                jsonObj["Simulation"]!["StableThreshold"] = double.Parse(txtStableThreshold.Text);
                jsonObj["Simulation"]!["StableCountThreshold"] = int.Parse(txtStableCount.Text);
                jsonObj["Hardware"]!["MaxTemperatureDriftPerTenMinutes"] = double.Parse(txtMaxDrift.Text);
                File.WriteAllText(configPath, System.Text.Json.JsonSerializer.Serialize(jsonObj, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("设置已保存，重启后生效", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
        }
        catch (Exception ex) { MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}