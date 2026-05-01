using Client.Grpc;
using Grpc.Net.Client;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client;

public partial class MainForm : Form
{
    private readonly RaceService.RaceServiceClient _raceClient;
    private readonly ReservationService.ReservationServiceClient _reservationClient;
    private readonly UserService.UserServiceClient _userClient;

    private readonly DataGridView _raceGrid = new();
    private readonly DataGridView _reservationGrid = new();
    private readonly DataGridView _userGrid = new();

    private readonly Button _refreshButton = new();
    private readonly Button _logoutButton = new();
    private readonly Button _manageReservationsButton = new();

    private readonly Label _statusLabel = new();

    private readonly User _currentUser;

    private readonly CancellationTokenSource _cts = new();

    public MainForm(User user)
    {
        _currentUser = user;

        Text = $"Dashboard - {user.Name}";
        Width = 1200;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Color.White;
        ForeColor = Color.FromArgb(30, 30, 30);

        var channel = GrpcChannel.ForAddress("http://localhost:9090");

        _raceClient = new RaceService.RaceServiceClient(channel);
        _reservationClient = new ReservationService.ReservationServiceClient(channel);
        _userClient = new UserService.UserServiceClient(channel);

        BuildUi();

        Load += async (_, _) =>
        {
            await LoadAllTablesAsync();
            _ = StartObservers();
        };
    }

    private async Task StartObservers()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:9090");

        var raceClient = new RaceService.RaceServiceClient(channel);
        var reservationClient = new ReservationService.ReservationServiceClient(channel);
        var userClient = new UserService.UserServiceClient(channel);

        _ = GrpcWatcher.WatchRaces(raceClient, _ =>
        {
            BeginInvoke(async () => await LoadAllTablesAsync());
        }, _cts.Token);

        _ = GrpcWatcher.WatchReservations(reservationClient, _ =>
        {
            BeginInvoke(async () => await LoadAllTablesAsync());
        }, _cts.Token);

        _ = GrpcWatcher.WatchUsers(userClient, _ =>
        {
            BeginInvoke(async () => await LoadAllTablesAsync());
        }, _cts.Token);
    }

    private void BuildUi()
    {
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
        };

        _refreshButton.Text = "Refresh";
        StyleButton(_refreshButton, 12);
        _refreshButton.Click += async (_, _) => await LoadAllTablesAsync();

        _manageReservationsButton.Text = "Manage Reservations";
        StyleButton(_manageReservationsButton, 170);
        _manageReservationsButton.Click += (_, _) =>
        {
            new ManageReservationsForm(_currentUser).Show();
        };

        _logoutButton.Text = "Logout";
        StyleButton(_logoutButton, 330);
        _logoutButton.Click += (_, _) =>
        {
            Hide();
            new LoginForm().Show();
            Close();
        };

        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = Color.FromArgb(60, 60, 60);
        _statusLabel.Left = 520;
        _statusLabel.Top = 20;
        _statusLabel.Text = "Ready";

        topPanel.Controls.Add(_refreshButton);
        topPanel.Controls.Add(_manageReservationsButton);
        topPanel.Controls.Add(_logoutButton);
        topPanel.Controls.Add(_statusLabel);

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
        };

        tabs.TabPages.Add(CreateTab("Races", _raceGrid));
        tabs.TabPages.Add(CreateTab("Reservations", _reservationGrid));
        tabs.TabPages.Add(CreateTab("Users", _userGrid));

        Controls.Add(tabs);
        Controls.Add(topPanel);

        ConfigureGrid(_raceGrid);
        ConfigureGrid(_reservationGrid);
        ConfigureGrid(_userGrid);
    }

    private static void StyleButton(Button btn, int left)
    {
        btn.Width = 150;
        btn.Height = 35;
        btn.Left = left;
        btn.Top = 12;

        btn.FlatStyle = FlatStyle.Flat;
        btn.BackColor = Color.FromArgb(0, 120, 215);
        btn.ForeColor = Color.White;
        btn.FlatAppearance.BorderSize = 0;
    }

    private static TabPage CreateTab(string title, DataGridView grid)
    {
        var page = new TabPage(title)
        {
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        grid.Dock = DockStyle.Fill;
        page.Controls.Add(grid);

        return page;
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        grid.BackgroundColor = Color.White;
        grid.GridColor = Color.FromArgb(230, 230, 230);

        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.Black;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 220, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        grid.EnableHeadersVisualStyles = false;
    }

    private async Task LoadAllTablesAsync()
    {
        var races = await LoadRacesAsync();
        var reservations = await LoadReservationsAsync();
        var users = await LoadUsersAsync();

        _raceGrid.DataSource = new BindingList<RaceRow>(races);
        _reservationGrid.DataSource = new BindingList<ReservationRow>(reservations);
        _userGrid.DataSource = new BindingList<UserRow>(users);
    }

    private async Task<List<RaceRow>> LoadRacesAsync()
    {
        var list = new List<RaceRow>();
        using var call = _raceClient.GetAllRaces(new Empty());

        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            var r = call.ResponseStream.Current;

            list.Add(new RaceRow
            {
                Id = r.Id,
                Destination = r.Destination,
                Date = r.Date,
                Time = r.Time
            });
        }

        return list;
    }

    private async Task<List<ReservationRow>> LoadReservationsAsync()
    {
        var list = new List<ReservationRow>();
        using var call = _reservationClient.GetAllReservations(new Empty());

        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            var r = call.ResponseStream.Current;

            list.Add(new ReservationRow
            {
                Id = r.Id,
                UserId = r.UserId,
                RaceId = r.RaceId,
                Name = r.Name
            });
        }

        return list;
    }

    private async Task<List<UserRow>> LoadUsersAsync()
    {
        var list = new List<UserRow>();
        using var call = _userClient.GetAllUsers(new Empty());

        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            var u = call.ResponseStream.Current;

            list.Add(new UserRow
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });
        }

        return list;
    }

    private sealed class RaceRow
    {
        public int Id { get; set; }
        public string Destination { get; set; } = "";
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
    }

    private sealed class ReservationRow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RaceId { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class UserRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}