using Grpc.Net.Client;
using Client.Grpc;
using System.Drawing;
using System.Windows.Forms;

namespace Client;

public class LoginForm : Form
{
    private readonly UserService.UserServiceClient _userClient;

    private readonly TextBox txtEmail = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnLogin = new();
    private readonly Label lblStatus = new();

    public User LoggedInUser { get; private set; } = null!;

    public LoginForm()
    {
        Text = "Login";
        Width = 350;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;

        var channel = GrpcChannel.ForAddress("http://localhost:9090");
        _userClient = new UserService.UserServiceClient(channel);

        BuildUi();
        ApplyTheme();
    }

    private void BuildUi()
    {
        var lblEmail = new Label { Text = "Email", Left = 20, Top = 20, Width = 80 };
        txtEmail.Left = 100;
        txtEmail.Top = 20;
        txtEmail.Width = 200;

        var lblPassword = new Label { Text = "Password", Left = 20, Top = 60, Width = 80 };
        txtPassword.Left = 100;
        txtPassword.Top = 60;
        txtPassword.Width = 200;
        txtPassword.PasswordChar = '*';

        btnLogin.Text = "Login";
        btnLogin.Left = 100;
        btnLogin.Top = 100;
        btnLogin.Width = 140;
        btnLogin.Height = 35;
        btnLogin.Click += BtnLogin_Click;

        lblStatus.Left = 20;
        lblStatus.Top = 145;
        lblStatus.Width = 280;

        Controls.Add(lblEmail);
        Controls.Add(txtEmail);
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(btnLogin);
        Controls.Add(lblStatus);
    }

    private void ApplyTheme()
    {
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        btnLogin.BackColor = Color.FromArgb(0, 120, 215);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.FlatAppearance.BorderSize = 0;

        lblStatus.ForeColor = Color.LightGray;
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        try
        {
            lblStatus.Text = "Logging in...";

            var reply = await _userClient.LoginAsync(new LoginRequest
            {
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Text
            });

            LoggedInUser = reply;

            DialogResult = DialogResult.OK;
            Close();
        }
        catch
        {
            lblStatus.Text = "Invalid email or password";
        }
    }
}