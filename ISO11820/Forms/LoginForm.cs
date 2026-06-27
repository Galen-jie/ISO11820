using ISO11820.Global;

namespace ISO11820.Forms;

public partial class LoginForm : Form
{
    private RadioButton rdoAdmin = null!;
    private RadioButton rdoOperator = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Button btnCancel = null!;
    private Label lblTitle = null!;
    private GroupBox grpRole = null!;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "ISO 11820 登录";
        this.Size = new Size(400, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        lblTitle = new Label
        {
            Text = "ISO 11820 建筑材料不燃性试验系统",
            Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, 20)
        };

        grpRole = new GroupBox
        {
            Text = "选择角色",
            Location = new Point(40, 60),
            Size = new Size(300, 50)
        };

        rdoAdmin = new RadioButton
        {
            Text = "管理员",
            Location = new Point(20, 20),
            Size = new Size(100, 25),
            Checked = true
        };

        rdoOperator = new RadioButton
        {
            Text = "操作员",
            Location = new Point(150, 20),
            Size = new Size(100, 25)
        };

        grpRole.Controls.AddRange(new Control[] { rdoAdmin, rdoOperator });

        var lblPassword = new Label
        {
            Text = "密码：",
            Location = new Point(40, 130),
            Size = new Size(60, 25)
        };

        txtPassword = new TextBox
        {
            Location = new Point(100, 130),
            Size = new Size(200, 25),
            PasswordChar = '*',
            MaxLength = 20
        };

        btnLogin = new Button
        {
            Text = "登录",
            Location = new Point(100, 170),
            Size = new Size(100, 35)
        };
        btnLogin.Click += BtnLogin_Click;

        btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(220, 170),
            Size = new Size(100, 35),
            DialogResult = DialogResult.Cancel
        };
        btnCancel.Click += (s, e) => Application.Exit();

        this.Controls.AddRange(new Control[] {
            lblTitle, grpRole, lblPassword, txtPassword, btnLogin, btnCancel
        });

        this.AcceptButton = btnLogin;
        this.CancelButton = btnCancel;
        txtPassword.Focus();
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        string username = rdoAdmin.Checked ? Constants.DefaultAdminUser : Constants.DefaultOperatorUser;
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(password))
        {
            MessageBox.Show("请输入密码", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPassword.Focus();
            return;
        }

        if (MyAppContext.Instance.DbHelper.ValidateLogin(username, password))
        {
            MyAppContext.Instance.CurrentOperator = username;
            MyAppContext.Instance.CurrentOperatorType = MyAppContext.Instance.DbHelper.GetOperatorType(username);

            Serilog.Log.Information($"User logged in: {username}");

            this.Hide();
            var mainForm = new MainForm();
            mainForm.FormClosed += (s, args) => this.Close();
            mainForm.Show();
        }
        else
        {
            MessageBox.Show("密码错误", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }
}