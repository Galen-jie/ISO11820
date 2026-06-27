using ISO11820.Global;

namespace ISO11820.Forms;

public partial class NewTestForm : Form
{
    private TextBox txtProductId = null!, txtTestId = null!, txtProductName = null!, txtSpec = null!;
    private TextBox txtDiameter = null!, txtHeight = null!, txtPreWeight = null!, txtAmbTemp = null!, txtAmbHumi = null!;
    private TextBox txtApparatusId = null!, txtApparatusName = null!, txtCheckDate = null!;
    private ComboBox cmbOperator = null!;
    private RadioButton rdoStandard = null!, rdoCustom = null!;
    private NumericUpDown nudCustomDuration = null!;
    private Button btnSave = null!, btnCancel = null!;

    public string ProductId => txtProductId.Text.Trim();
    public string TestId => txtTestId.Text.Trim();
    public string ProductName => txtProductName.Text.Trim();
    public double PreWeight => double.TryParse(txtPreWeight.Text, out var v) ? v : 0;
    public double AmbTemp => double.TryParse(txtAmbTemp.Text, out var v) ? v : 25;
    public double AmbHumi => double.TryParse(txtAmbHumi.Text, out var v) ? v : 50;
    public int Duration => rdoStandard.Checked ? Constants.StandardTestDuration : (int)nudCustomDuration.Value * 60;

    public NewTestForm() { InitializeComponent(); LoadDefaults(); }

    private void InitializeComponent()
    {
        this.Text = "新建试验";
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var font = new Font("Microsoft YaHei", 10);
        int y = 20, xLbl = 20, xInp = 130;

        AddLabel("样品编号*", xLbl, y);
        txtProductId = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font }; y += 35;

        AddLabel("试验编号", xLbl, y);
        txtTestId = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font, ReadOnly = true }; y += 35;

        AddLabel("样品名称*", xLbl, y);
        txtProductName = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font }; y += 35;

        AddLabel("规格", xLbl, y);
        txtSpec = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font }; y += 35;

        AddLabel("直径(mm)", xLbl, y);
        txtDiameter = new TextBox { Location = new Point(xInp, y), Size = new Size(80, 25), Font = font, Text = "45" };
        AddLabel("高度(mm)", xInp + 100, y);
        txtHeight = new TextBox { Location = new Point(xInp + 180, y), Size = new Size(80, 25), Font = font, Text = "50" }; y += 35;

        AddLabel("试验前重量(g)*", xLbl, y);
        txtPreWeight = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font }; y += 35;

        AddLabel("环境温度(C)", xLbl, y);
        txtAmbTemp = new TextBox { Location = new Point(xInp, y), Size = new Size(80, 25), Font = font, Text = "25" };
        AddLabel("湿度(%)", xInp + 100, y);
        txtAmbHumi = new TextBox { Location = new Point(xInp + 180, y), Size = new Size(80, 25), Font = font, Text = "50" }; y += 35;

        AddLabel("操作员", xLbl, y);
        cmbOperator = new ComboBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font, DropDownStyle = ComboBoxStyle.DropDownList }; y += 35;

        AddLabel("试验时长", xLbl, y);
        rdoStandard = new RadioButton { Text = "标准60分钟", Location = new Point(xInp, y), Size = new Size(120, 25), Font = font, Checked = true };
        rdoCustom = new RadioButton { Text = "自定义", Location = new Point(xInp + 130, y), Size = new Size(80, 25), Font = font };
        nudCustomDuration = new NumericUpDown { Location = new Point(xInp + 220, y), Size = new Size(60, 25), Font = font, Minimum = 1, Maximum = 120, Value = 30, Enabled = false };
        rdoCustom.CheckedChanged += (s, e) => nudCustomDuration.Enabled = rdoCustom.Checked; y += 45;

        AddLabel("设备编号", xLbl, y);
        txtApparatusId = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font, ReadOnly = true }; y += 35;

        AddLabel("设备名称", xLbl, y);
        txtApparatusName = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font, ReadOnly = true }; y += 35;

        AddLabel("校验日期", xLbl, y);
        txtCheckDate = new TextBox { Location = new Point(xInp, y), Size = new Size(200, 25), Font = font, ReadOnly = true }; y += 50;

        btnSave = new Button { Text = "保存", Location = new Point(130, y), Size = new Size(100, 35), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button { Text = "取消", Location = new Point(250, y), Size = new Size(100, 35), Font = font };
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.AcceptButton = btnSave;
        this.CancelButton = btnCancel;

        this.Controls.AddRange(new Control[] {
            txtProductId, txtTestId, txtProductName, txtSpec, txtDiameter, txtHeight, txtPreWeight, txtAmbTemp, txtAmbHumi,
            cmbOperator, rdoStandard, rdoCustom, nudCustomDuration, txtApparatusId, txtApparatusName, txtCheckDate, btnSave, btnCancel
        });
    }

    private void AddLabel(string text, int x, int y) { this.Controls.Add(new Label { Text = text, Location = new Point(x, y), Size = new Size(100, 25), Font = new Font("Microsoft YaHei", 9) }); }

    private void LoadDefaults()
    {
        txtTestId.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var operators = MyAppContext.Instance.DbHelper.GetAllOperatorNames();
        cmbOperator.Items.AddRange(operators.ToArray());
        cmbOperator.SelectedItem = MyAppContext.Instance.CurrentOperator;

        var apparatus = MyAppContext.Instance.DbHelper.GetApparatus(0);
        if (apparatus != null)
        {
            txtApparatusId.Text = apparatus.InnerNumber;
            txtApparatusName.Text = apparatus.ApparatusName;
            txtCheckDate.Text = apparatus.CheckDateT.ToString("yyyy-MM-dd");
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput()) return;

        if (!MyAppContext.Instance.DbHelper.ProductExists(ProductId))
            MyAppContext.Instance.DbHelper.InsertProduct(new Data.Models.ProductMaster { ProductId = ProductId, ProductName = ProductName, Specific = txtSpec.Text, Diameter = double.Parse(txtDiameter.Text), Height = double.Parse(txtHeight.Text) });

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ProductId)) { MessageBox.Show("请输入样品编号", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtProductId.Focus(); return false; }
        if (string.IsNullOrWhiteSpace(ProductName)) { MessageBox.Show("请输入样品名称", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtProductName.Focus(); return false; }
        if (!double.TryParse(txtPreWeight.Text, out var w) || w <= 0) { MessageBox.Show("请输入有效的重量", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPreWeight.Focus(); return false; }
        return true;
    }
}