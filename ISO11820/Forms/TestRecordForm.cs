using ISO11820.Global;
using ISO11820.Services;

namespace ISO11820.Forms;

public partial class TestRecordForm : Form
{
    private readonly string _productId, _testId;
    private readonly double _preWeight;

    private CheckBox chkFlame = null!;
    private NumericUpDown nudFlameTime = null!, nudFlameDuration = null!;
    private TextBox txtPostWeight = null!, txtMemo = null!;
    private Label lblPreWeight = null!, lblLostWeight = null!, lblLostWeightPer = null!;
    private Button btnSave = null!, btnCancel = null!;

    public double PostWeight => double.TryParse(txtPostWeight.Text, out var v) ? v : 0;
    public bool HasFlame => chkFlame.Checked;
    public int FlameTime => (int)nudFlameTime.Value;
    public int FlameDuration => (int)nudFlameDuration.Value;
    public string Memo => txtMemo.Text.Trim();

    public TestRecordForm(string productId, string testId, double preWeight)
    {
        _productId = productId;
        _testId = testId;
        _preWeight = preWeight;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "试验记录";
        this.Size = new Size(400, 380);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var font = new Font("Microsoft YaHei", 10);
        int y = 20, xLbl = 20, xInp = 120;

        AddLabel("试验前重量", xLbl, y);
        lblPreWeight = new Label { Text = $"{_preWeight:F2} g", Location = new Point(xInp, y), Size = new Size(150, 25), Font = font }; y += 35;

        AddLabel("试验后重量*", xLbl, y);
        txtPostWeight = new TextBox { Location = new Point(xInp, y), Size = new Size(150, 25), Font = font };
        txtPostWeight.TextChanged += TxtPostWeight_TextChanged; y += 35;

        AddLabel("失重", xLbl, y);
        lblLostWeight = new Label { Text = "0.00 g", Location = new Point(xInp, y), Size = new Size(150, 25), Font = font }; y += 35;

        AddLabel("失重率", xLbl, y);
        lblLostWeightPer = new Label { Text = "0.00 %", Location = new Point(xInp, y), Size = new Size(150, 25), Font = font, ForeColor = Color.Red }; y += 40;

        chkFlame = new CheckBox { Text = "观察到火焰", Location = new Point(xLbl, y), Size = new Size(150, 25), Font = font }; y += 30;

        AddLabel("火焰时间(s)", xLbl, y);
        nudFlameTime = new NumericUpDown { Location = new Point(xInp, y), Size = new Size(80, 25), Font = font, Minimum = 0, Maximum = 3600, Enabled = false };

        AddLabel("持续时间(s)", 220, y);
        nudFlameDuration = new NumericUpDown { Location = new Point(300, y), Size = new Size(80, 25), Font = font, Minimum = 0, Maximum = 60, Enabled = false }; y += 40;

        chkFlame.CheckedChanged += (s, e) => { nudFlameTime.Enabled = chkFlame.Checked; nudFlameDuration.Enabled = chkFlame.Checked; };

        AddLabel("备注", xLbl, y);
        txtMemo = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 50), Font = font, Multiline = true }; y += 60;

        btnSave = new Button { Text = "保存", Location = new Point(100, y), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button { Text = "取消", Location = new Point(220, y), Size = new Size(100, 35), Font = font };
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { lblPreWeight, txtPostWeight, lblLostWeight, lblLostWeightPer, chkFlame, nudFlameTime, nudFlameDuration, txtMemo, btnSave, btnCancel });
    }

    private void AddLabel(string text, int x, int y) { this.Controls.Add(new Label { Text = text, Location = new Point(x, y), Size = new Size(100, 25), Font = new Font("Microsoft YaHei", 9) }); }

    private void TxtPostWeight_TextChanged(object? sender, EventArgs e)
    {
        if (double.TryParse(txtPostWeight.Text, out var postWeight))
        {
            double lostWeight = TemperatureCalculator.CalculateWeightLoss(_preWeight, postWeight);
            double lostWeightPer = TemperatureCalculator.CalculateWeightLossPercentage(_preWeight, postWeight);
            lblLostWeight.Text = $"{lostWeight:F2} g";
            lblLostWeightPer.Text = $"{lostWeightPer:F2} %";
            lblLostWeightPer.ForeColor = lostWeightPer > Constants.MaxWeightLossPercent ? Color.Red : Color.Green;
        }
        else { lblLostWeight.Text = "-- g"; lblLostWeightPer.Text = "-- %"; }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!double.TryParse(txtPostWeight.Text, out var postWeight) || postWeight <= 0) { MessageBox.Show("请输入有效的试验后重量", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPostWeight.Focus(); return; }
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}