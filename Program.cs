using System;
using System.Threading;
using System.Windows.Forms;

namespace Client;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var loginForm = new LoginForm();
        if (loginForm.ShowDialog() != DialogResult.OK)
            return;

        var client1 = new MainForm(loginForm.LoggedInUser);
        var client2 = new MainForm(loginForm.LoggedInUser);

        client1.Text += " #1";
        client2.Text += " #2";

        Application.Run(new DualFormContext(client1, client2));
    }

    private sealed class DualFormContext : ApplicationContext
    {
        private int _openForms = 2;

        public DualFormContext(Form form1, Form form2)
        {
            form1.FormClosed += OnFormClosed;
            form2.FormClosed += OnFormClosed;

            form1.Show();
            form2.Show();
        }

        private void OnFormClosed(object? sender, FormClosedEventArgs e)
        {
            if (Interlocked.Decrement(ref _openForms) == 0)
                ExitThread();
        }
    }
}