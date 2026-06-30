using System.Drawing;
using System.Windows.Forms;
using OfficeOpenXml;
using OxyPlot;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;
using OxyPlot.Series;
using ISO11820.Core;
using ISO11820.Global;
using ISO11820.Services;

namespace ISO11820.Forms;

public partial class MainForm : Form
{
    private TestMaster _testMaster = null!;
    private readonly ExportService _exportService = new();

    private TabControl tabControl = null!;
    private TabPage tabTest = null!;
    private TabPage tabQuery = null!;
    private TabPage tabCalibration = null!;

    private Label lblTf1 = null!, lblTf2 = null!, lblTs = null!, lblTc = null!, lblTcal = null!;
    private Label lblTimer = null!, lblDrift = null!, lblStatus = null!, lblProductId = null!;

    private Button btnNewTest = null!, btnStartHeating = null!, btnStopHeating = null!;
    private Button btnStartRecording = null!, btnStopRecording = null!, btnTestRecord = null!, btnSettings = null!;

    private PlotView plotView = null!;
    private PlotModel plotModel = null!;
    private LineSeries tf1Series = null!, tf2Series = null!, tsSeries = null!, tcSeries = null!;

    private RichTextBox richTextBoxLog = null!;

    private DateTimePicker dtpFromDate = null!, dtpToDate = null!;
    private TextBox txtQueryProductId = null!;
    private ComboBox cmbQueryOperator = null!;
    private Button btnQuery = null!, btnExportQuery = null!;
    private DataGridView dgvQueryResults = null!;

    private Label lblCalibrationTemp = null!;
    private Button btnCalibrate = null!;
    private DataGridView dgvCalibrationHistory = null!;

    public MainForm()
    {
        InitializeComponent();
        InitializePlot();

        _testMaster = new TestMaster(MyAppContext.Instance.Config.Simulation);
        MyAppContext.Instance.CurrentTestMaster = _testMaster;

        _testMaster.DataBroadcast += OnDataBroadcast;
        _testMaster.StateChanged += OnStateChanged;

        AddSystemMessage(DateTime.Now.ToString("HH:mm:ss"), $"系统初始化完成，操作员: {MyAppContext.Instance.CurrentOperator}", MessageType.Normal);
        UpdateButtonStates(TestState.Idle);
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 试验系统";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(1000, 600);

        tabControl = new TabControl { Dock = DockStyle.Fill };
        tabTest = new TabPage { Text = "试验控制" };
        tabQuery = new TabPage { Text = "记录查询" };
        tabCalibration = new TabPage { Text = "校准管理" };

        tabControl.TabPages.AddRange(new TabPage[] { tabTest, tabQuery, tabCalibration });

        InitializeTestTab();
        InitializeQueryTab();
        InitializeCalibrationTab();

        this.Controls.Add(tabControl);
        this.FormClosing += MainForm_FormClosing;
    }

    private void InitializeTestTab()
    {
        var leftPanel = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = Color.FromArgb(40, 40, 40) };

        int yPos = 20;
        var fontValue = new Font("Consolas", 20, FontStyle.Bold);

        lblTf1 = CreateTempLabel("TF1: 0.0 C", yPos, Color.Red); yPos += 50;
        lblTf2 = CreateTempLabel("TF2: 0.0 C", yPos, Color.Orange); yPos += 50;
        lblTs = CreateTempLabel("TS: 0.0 C", yPos, Color.Blue); yPos += 50;
        lblTc = CreateTempLabel("TC: 0.0 C", yPos, Color.Green); yPos += 50;
        lblTcal = CreateTempLabel("TCal: 0.0 C", yPos, Color.Purple); yPos += 50;

        lblTimer = new Label { Text = "计时: 0 s", Location = new Point(10, yPos), Size = new Size(280, 30), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 11), BackColor = Color.FromArgb(60, 60, 60) };
        yPos += 35;
        lblDrift = new Label { Text = "漂移: 0 C/10min", Location = new Point(10, yPos), Size = new Size(280, 30), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10), BackColor = Color.FromArgb(60, 60, 60) };
        yPos += 35;
        lblStatus = new Label { Text = "状态: 待机", Location = new Point(10, yPos), Size = new Size(280, 30), ForeColor = Color.Cyan, Font = new Font("Microsoft YaHei", 11, FontStyle.Bold), BackColor = Color.FromArgb(50, 50, 50) };
        yPos += 35;
        lblProductId = new Label { Text = "样品: --", Location = new Point(10, yPos), Size = new Size(280, 30), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10), BackColor = Color.FromArgb(50, 50, 50) };
        yPos += 45;

        btnNewTest = CreateButton("新建试验", yPos, Color.FromArgb(70, 130, 180));
        btnNewTest.Click += BtnNewTest_Click;
        yPos += 40;

        btnStartHeating = CreateButton("开始加热", yPos, Color.FromArgb(255, 140, 0));
        btnStartHeating.Click += BtnStartHeating_Click;
        yPos += 40;

        btnStopHeating = CreateButton("停止加热", yPos, Color.Gray);
        btnStopHeating.Click += BtnStopHeating_Click;
        yPos += 40;

        btnStartRecording = CreateButton("开始记录", yPos, Color.Green);
        btnStartRecording.Click += BtnStartRecording_Click;
        yPos += 40;

        btnStopRecording = CreateButton("停止记录", yPos, Color.FromArgb(200, 50, 50));
        btnStopRecording.Click += BtnStopRecording_Click;
        yPos += 40;

        btnTestRecord = CreateButton("保存记录", yPos, Color.FromArgb(60, 60, 140));
        btnTestRecord.Click += BtnTestRecord_Click;
        yPos += 40;

        btnSettings = CreateButton("系统设置", yPos, Color.FromArgb(80, 80, 80));
        btnSettings.Click += BtnSettings_Click;

        leftPanel.Controls.AddRange(new Control[] {
            lblTf1, lblTf2, lblTs, lblTc, lblTcal, lblTimer, lblDrift, lblStatus, lblProductId,
            btnNewTest, btnStartHeating, btnStopHeating, btnStartRecording, btnStopRecording, btnTestRecord, btnSettings
        });

        var rightPanel = new Panel { Dock = DockStyle.Fill };
        var plotPanel = new Panel { Dock = DockStyle.Top, Height = 350, BackColor = Color.White };
        plotView = new PlotView { Dock = DockStyle.Fill, BackColor = Color.White };
        plotPanel.Controls.Add(plotView);

        var logPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30) };
        var lblLogTitle = new Label { Text = "系统消息", Dock = DockStyle.Top, Height = 25, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold), BackColor = Color.FromArgb(50, 50, 50) };
        richTextBoxLog = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Consolas", 10), ReadOnly = true };
        logPanel.Controls.AddRange(new Control[] { lblLogTitle, richTextBoxLog });

        rightPanel.Controls.AddRange(new Control[] { plotPanel, logPanel });
        tabTest.Controls.AddRange(new Control[] { rightPanel, leftPanel });
    }

    private Label CreateTempLabel(string text, int y, Color color)
    {
        return new Label { Text = text, Location = new Point(10, y), Size = new Size(280, 40), ForeColor = color, Font = new Font("Consolas", 18, FontStyle.Bold), BackColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
    }

    private Button CreateButton(string text, int y, Color color)
    {
        return new Button { Text = text, Location = new Point(10, y), Size = new Size(280, 35), BackColor = color, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) };
    }

    private void InitializePlot()
    {
        plotModel = new PlotModel { Title = "温度曲线", Background = OxyColors.White };
        plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "时间(s)", Minimum = 0, Maximum = 600 });
        plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "温度(C)", Minimum = 0, Maximum = 800 });

        tf1Series = new LineSeries { Title = "TF1", Color = OxyColors.Red };
        tf2Series = new LineSeries { Title = "TF2", Color = OxyColors.Orange };
        tsSeries = new LineSeries { Title = "TS", Color = OxyColors.Blue };
        tcSeries = new LineSeries { Title = "TC", Color = OxyColors.Green };

        plotModel.Series.Add(tf1Series);
        plotModel.Series.Add(tf2Series);
        plotModel.Series.Add(tsSeries);
        plotModel.Series.Add(tcSeries);

        plotView.Model = plotModel;
    }

    private void InitializeQueryTab()
    {
        var queryPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(240, 240, 240) };

        dtpFromDate = new DateTimePicker { Location = new Point(90, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
        dtpToDate = new DateTimePicker { Location = new Point(330, 15), Size = new Size(150, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        txtQueryProductId = new TextBox { Location = new Point(570, 15), Size = new Size(150, 25) };
        cmbQueryOperator = new ComboBox { Location = new Point(790, 15), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };

        btnQuery = new Button { Text = "查询", Location = new Point(20, 45), Size = new Size(100, 30), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White };
        btnQuery.Click += BtnQuery_Click;

        btnExportQuery = new Button { Text = "导出Excel", Location = new Point(130, 45), Size = new Size(100, 30), BackColor = Color.FromArgb(60, 140, 60), ForeColor = Color.White };
        btnExportQuery.Click += BtnExportQuery_Click;

        queryPanel.Controls.AddRange(new Control[] {
            new Label { Text = "开始日期", Location = new Point(20, 15), Size = new Size(70, 20) }, dtpFromDate,
            new Label { Text = "结束日期", Location = new Point(260, 15), Size = new Size(70, 20) }, dtpToDate,
            new Label { Text = "样品编号", Location = new Point(500, 15), Size = new Size(70, 20) }, txtQueryProductId,
            new Label { Text = "操作员", Location = new Point(740, 15), Size = new Size(50, 20) }, cmbQueryOperator,
            btnQuery, btnExportQuery
        });

        dgvQueryResults = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
        dgvQueryResults.CellDoubleClick += DgvQueryResults_CellDoubleClick;

        tabQuery.Controls.AddRange(new Control[] { dgvQueryResults, queryPanel });

        cmbQueryOperator.Items.Add("(全部)");
        var operators = MyAppContext.Instance.DbHelper.GetAllOperatorNames();
        cmbQueryOperator.Items.AddRange(operators.ToArray());
        cmbQueryOperator.SelectedIndex = 0;
    }

    private void InitializeCalibrationTab()
    {
        var calibrationPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(240, 240, 240) };

        lblCalibrationTemp = new Label { Text = "0.0 C", Location = new Point(130, 20), Size = new Size(120, 30), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Purple, BackColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

        btnCalibrate = new Button { Text = "记录校准", Location = new Point(20, 60), Size = new Size(120, 30), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White };
        btnCalibrate.Click += BtnCalibrate_Click;

        calibrationPanel.Controls.AddRange(new Control[] {
            new Label { Text = "校准温度", Location = new Point(20, 20), Size = new Size(100, 20), Font = new Font("Microsoft YaHei", 10, FontStyle.Bold) },
            lblCalibrationTemp, btnCalibrate
        });

        dgvCalibrationHistory = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };

        tabCalibration.Controls.AddRange(new Control[] { dgvCalibrationHistory, calibrationPanel });
        LoadCalibrationHistory();
    }

    private void LoadCalibrationHistory()
    {
        var records = MyAppContext.Instance.DbHelper.QueryCalibrationRecords(null, null);
        dgvCalibrationHistory.DataSource = records.Select(r => new { 日期 = r.CalibrationDate, 类型 = r.CalibrationType, 操作员 = r.Operator, 平均值 = r.TAvg?.ToString("F2") ?? "--", 结果 = r.PassedCriteria == 1 ? "合格" : "不合格" }).ToList();
    }

    private void OnDataBroadcast(object? sender, DataBroadcastEventArgs e)
    {
        if (this.InvokeRequired) { this.Invoke(() => OnDataBroadcast(sender, e)); return; }

        lblTf1.Text = $"TF1: {e.Tf1:F1} C";
        lblTf2.Text = $"TF2: {e.Tf2:F1} C";
        lblTs.Text = $"TS: {e.Ts:F1} C";
        lblTc.Text = $"TC: {e.Tc:F1} C";
        lblTcal.Text = $"TCal: {e.Tcal:F1} C";
        lblCalibrationTemp.Text = $"{e.Tcal:F1} C";
        lblTimer.Text = $"计时: {e.ElapsedSeconds} s";
        lblDrift.Text = $"漂移: {e.TempDrift:F2} C/10min";

        UpdateStatusDisplay(e.CurrentState);

        // 加热阶段和记录阶段都显示温度曲线
        if (e.CurrentState == TestState.Preparing || e.CurrentState == TestState.Ready)
        {
            AddDataPoint(e.HeatingElapsedSeconds, e.Tf1, e.Tf2, e.Ts, e.Tc);
        }
        else if (e.CurrentState == TestState.Recording)
        {
            // 记录阶段使用记录时间，延续加热阶段的时间轴
            int totalHeatingTime = e.HeatingElapsedSeconds + e.ElapsedSeconds;
            AddDataPoint(totalHeatingTime, e.Tf1, e.Tf2, e.Ts, e.Tc);
        }

        foreach (var msg in e.Messages)
            AddSystemMessage(msg.Time, msg.Message, msg.Type);

        UpdateButtonStates(e.CurrentState);
    }

    private void OnStateChanged(object? sender, TestState newState)
    {
        if (this.InvokeRequired) { this.Invoke(() => OnStateChanged(sender, newState)); return; }
        UpdateButtonStates(newState);
    }

    private void UpdateStatusDisplay(TestState state)
    {
        var statusText = state == TestState.Idle ? "待机" : state == TestState.Preparing ? "准备中" : state == TestState.Ready ? "就绪" : state == TestState.Recording ? "记录中" : state == TestState.Complete ? "已完成" : state.ToString();
        lblStatus.Text = $"状态: {statusText}";
        lblStatus.ForeColor = state == TestState.Idle ? Color.Gray : state == TestState.Recording ? Color.Green : Color.Cyan;
    }

    private void AddDataPoint(int time, double tf1, double tf2, double ts, double tc)
    {
        tf1Series.Points.Add(new DataPoint(time, tf1));
        tf2Series.Points.Add(new DataPoint(time, tf2));
        tsSeries.Points.Add(new DataPoint(time, ts));
        tcSeries.Points.Add(new DataPoint(time, tc));

        if (tf1Series.Points.Count > Constants.MaxHistoryPoints)
        {
            tf1Series.Points.RemoveAt(0);
            tf2Series.Points.RemoveAt(0);
            tsSeries.Points.RemoveAt(0);
            tcSeries.Points.RemoveAt(0);
        }

        plotModel.Axes[0].Minimum = Math.Max(0, time - Constants.MaxHistoryPoints);
        plotModel.Axes[0].Maximum = time;
        plotView.InvalidatePlot(true);
    }

    private void AddSystemMessage(string time, string message, MessageType type)
    {
        var color = type == MessageType.Warning ? Color.Yellow : type == MessageType.Error ? Color.Red : Color.White;
        richTextBoxLog.SelectionColor = color;
        richTextBoxLog.AppendText($"{time}  {message}\n");
        richTextBoxLog.ScrollToCaret();
    }

    private void UpdateButtonStates(TestState state)
    {
        bool hasUnsavedTest = _testMaster.HasUnsavedTest();

        btnNewTest.Enabled = state == TestState.Idle || (state == TestState.Complete && !hasUnsavedTest);
        btnStartHeating.Enabled = state == TestState.Idle;
        btnStopHeating.Enabled = state == TestState.Preparing || state == TestState.Ready || state == TestState.Complete;
        btnStartRecording.Enabled = state == TestState.Ready && !hasUnsavedTest;
        btnStopRecording.Enabled = state == TestState.Recording;
        btnTestRecord.Enabled = state == TestState.Complete && hasUnsavedTest;
        btnSettings.Enabled = state != TestState.Recording;

        var info = _testMaster.GetCurrentTestInfo();
        lblProductId.Text = info.ProductId != null ? $"样品: {info.ProductId}" : "样品: --";
    }

    private void BtnNewTest_Click(object? sender, EventArgs e)
    {
        if (_testMaster.CurrentState != TestState.Idle && _testMaster.CurrentState != TestState.Complete)
        {
            MessageBox.Show("请先停止当前操作后再新建试验", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new NewTestForm();
        if (form.ShowDialog() == DialogResult.OK)
        {
            var apparatus = MyAppContext.Instance.DbHelper.GetApparatus(0);
            if (apparatus == null) { MessageBox.Show("设备未配置", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            _testMaster.CreateNewTest(form.ProductId, form.TestId, form.PreWeight, form.AmbTemp, form.AmbHumi, MyAppContext.Instance.CurrentOperator, form.Duration, apparatus.InnerNumber, apparatus.ApparatusName, apparatus.CheckDateT);
            ClearPlot();
            lblProductId.Text = $"样品: {form.ProductId}";
            AddSystemMessage(DateTime.Now.ToString("HH:mm:ss"), $"试验已创建: {form.ProductId}", MessageType.Normal);
        }
    }

    private void BtnStartHeating_Click(object? sender, EventArgs e)
    {
        if (_testMaster.CurrentProductId == null)
        {
            MessageBox.Show("请先点击\"新建试验\"创建试验", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_testMaster.HasUnsavedTest()) { MessageBox.Show("请先保存上一次试验", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try { _testMaster.StartHeating(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BtnStopHeating_Click(object? sender, EventArgs e)
    {
        _testMaster.StopHeating();
        // 停止加热后清除曲线，避免下次加热时数据叠加
        ClearPlot();
    }

    private void BtnStartRecording_Click(object? sender, EventArgs e)
    {
        if (_testMaster.CurrentProductId == null)
        {
            MessageBox.Show("请先点击\"新建试验\"创建试验", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_testMaster.CurrentState != TestState.Ready)
        {
            MessageBox.Show("请先点击\"开始加热\"，等待温度稳定后（状态显示\"就绪\"）再开始记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_testMaster.HasUnsavedTest())
        {
            MessageBox.Show("请先保存上一次试验", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try { _testMaster.StartRecording(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BtnStopRecording_Click(object? sender, EventArgs e)
    {
        if (_testMaster.CurrentState != TestState.Recording)
        {
            MessageBox.Show("当前不在记录状态", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _testMaster.StopRecording();
    }

    private void BtnTestRecord_Click(object? sender, EventArgs e)
    {
        var info = _testMaster.GetCurrentTestInfo();
        if (info.ProductId == null) { MessageBox.Show("没有可保存的试验", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        using var form = new TestRecordForm(info.ProductId, info.TestId!, info.PreWeight);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _testMaster.SaveTestRecord(form.PostWeight, form.HasFlame, form.FlameTime, form.FlameDuration, form.Memo);

            var testData = _testMaster.GetTemperatureHistory();
            var record = MyAppContext.Instance.DbHelper.GetTest(info.ProductId, info.TestId);
            if (record != null)
            {
                _exportService.ExportToExcel(record, testData);
                if (MyAppContext.Instance.Config.Report.EnablePdfExport)
                    _exportService.ExportToPdf(record, testData);
            }

            MessageBox.Show("试验记录已保存，报告已生成", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _testMaster.ResetToIdle();
        }
    }

    private void BtnSettings_Click(object? sender, EventArgs e) { using var form = new SettingsForm(); form.ShowDialog(); }

    private void BtnQuery_Click(object? sender, EventArgs e)
    {
        var tests = MyAppContext.Instance.DbHelper.QueryTests(dtpFromDate.Value, dtpToDate.Value, txtQueryProductId.Text, cmbQueryOperator.SelectedIndex > 0 ? cmbQueryOperator.SelectedItem?.ToString() : null);
        dgvQueryResults.DataSource = tests.Select(t => new { 试验编号 = t.TestId, 样品编号 = t.ProductId, 样品名称 = t.ProductName, 日期 = t.TestDate.ToString("yyyy-MM-dd"), 操作员 = t.Operator, 失重率 = $"{t.LostWeightPer:F2}%", 温升 = $"{t.DeltaTf:F1}C", 状态 = t.Flag == "10000000" ? "已完成" : "待处理" }).ToList();
    }

    private void BtnExportQuery_Click(object? sender, EventArgs e)
    {
        var tests = MyAppContext.Instance.DbHelper.QueryTests(null, null, null, null);
        if (tests.Count == 0) { MessageBox.Show("无数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        var filePath = Path.Combine(MyAppContext.Instance.Config.Report.OutputDirectory, $"TestRecords_{DateTime.Now:yyyyMMdd}.xlsx");
        try
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Records");
            for (int i = 0; i < tests.Count; i++)
            {
                var t = tests[i];
                int row = i + 2;
                sheet.Cells[row, 1].Value = t.TestId;
                sheet.Cells[row, 2].Value = t.ProductId;
                sheet.Cells[row, 3].Value = t.TestDate.ToString("yyyy-MM-dd");
                sheet.Cells[row, 4].Value = t.Operator;
                sheet.Cells[row, 5].Value = t.LostWeightPer;
                sheet.Cells[row, 6].Value = t.DeltaTf;
            }
            package.SaveAs(new FileInfo(filePath));
            MessageBox.Show($"已导出: {filePath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DgvQueryResults_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var testId = dgvQueryResults.Rows[e.RowIndex].Cells["TestID"].Value?.ToString();
        var productId = dgvQueryResults.Rows[e.RowIndex].Cells["ProductID"].Value?.ToString();
        if (testId == null || productId == null) return;

        var test = MyAppContext.Instance.DbHelper.GetTest(productId, testId);
        if (test != null) { using var detailForm = new TestDetailForm(test); detailForm.ShowDialog(); }
    }

    private void BtnCalibrate_Click(object? sender, EventArgs e)
    {
        using var form = new CalibrationForm();
        if (form.ShowDialog() == DialogResult.OK) LoadCalibrationHistory();
    }

    private void ClearPlot()
    {
        tf1Series.Points.Clear();
        tf2Series.Points.Clear();
        tsSeries.Points.Clear();
        tcSeries.Points.Clear();
        plotView.InvalidatePlot(true);
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_testMaster.HasUnsavedTest())
        {
            var result = MessageBox.Show("存在未保存的试验，确定关闭?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No) { e.Cancel = true; return; }
        }
        _testMaster.Dispose();
    }
}