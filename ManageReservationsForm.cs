using Client.Grpc;
using Grpc.Net.Client;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client;

public class ManageReservationsForm : Form
{
    private readonly ReservationService.ReservationServiceClient _client;
    private readonly RaceService.RaceServiceClient _raceClient;
    private readonly User _currentUser;

    private readonly CancellationTokenSource _cts = new();
    private bool _isReloading;

    private readonly DataGridView _grid = new();

    private readonly TextBox _nameBox = new();
    private readonly TextBox _raceIdBox = new();
    private readonly TextBox _userIdBox = new();

    private readonly Button _searchBtn = new();
    private readonly Button _addBtn = new();
    private readonly Button _deleteBtn = new();

    public ManageReservationsForm(User currentUser)
    {
        _currentUser = currentUser;

        Text = "Manage Reservations";
        Width = 1000;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        var channel = GrpcChannel.ForAddress("http://localhost:9090");

        _client = new ReservationService.ReservationServiceClient(channel);
        _raceClient = new RaceService.RaceServiceClient(channel);

        BuildUi();

        Load += async (_, _) =>
        {
            await LoadReservations();
            _ = StartObservers();
        };

        FormClosed += (_, _) => _cts.Cancel();
    }

    // ===================== OBSERVER =====================

    private async Task StartObservers()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:9090");
        var client = new ReservationService.ReservationServiceClient(channel);

        _ = Task.Run(async () =>
        {
            using var call = client.WatchReservations(new Empty());

            while (await call.ResponseStream.MoveNext(_cts.Token))
            {
                SafeReload();
            }
        });
    }

    // ===================== UI =====================

    private void BuildUi()
    {
        var top = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(10)
        };

        var nameLbl = new Label { Text = "Name", Top = 15, Left = 10, Width = 60 };
        _nameBox.SetBounds(70, 12, 150, 25);

        var raceLbl = new Label { Text = "Race ID", Top = 15, Left = 240, Width = 60 };
        _raceIdBox.SetBounds(310, 12, 100, 25);

        var userLbl = new Label { Text = "User ID", Top = 15, Left = 430, Width = 60 };
        _userIdBox.SetBounds(500, 12, 100, 25);

        _searchBtn.Text = "Search";
        StyleButton(_searchBtn);
        _searchBtn.SetBounds(630, 10, 120, 30);
        _searchBtn.Click += async (_, _) => await LoadReservations();

        _addBtn.Text = "Add Reservation";
        StyleButton(_addBtn);
        _addBtn.SetBounds(10, 55, 180, 35);

        _addBtn.Click += (_, _) =>
        {
            if (_grid.CurrentRow?.DataBoundItem is not ReservationRow row)
                return;

            new AddReservationForm(
                _currentUser,
                row.RaceId,
                row.Destination,
                row.Date,
                row.Time
            ).ShowDialog();
        };

        _deleteBtn.Text = "Delete Selected";
        StyleButton(_deleteBtn);
        _deleteBtn.SetBounds(200, 55, 180, 35);
        _deleteBtn.Click += async (_, _) => await DeleteSelected();

        top.Controls.AddRange(new Control[]
        {
            nameLbl, _nameBox,
            raceLbl, _raceIdBox,
            userLbl, _userIdBox,
            _searchBtn,
            _addBtn,
            _deleteBtn
        });

        ConfigureGrid(_grid);
        _grid.Dock = DockStyle.Fill;

        Controls.Add(_grid);
        Controls.Add(top);
    }

    private static void StyleButton(Button btn)
    {
        btn.BackColor = Color.SteelBlue;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        grid.BackgroundColor = Color.White;
        grid.GridColor = Color.LightGray;

        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.Black;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
        grid.EnableHeadersVisualStyles = false;
    }

    // ===================== SAFE RELOAD =====================

    private void SafeReload()
    {
        if (IsDisposed || !IsHandleCreated) return;

        BeginInvoke(async () => await LoadReservations());
    }

    // ===================== LOAD =====================

    private async Task LoadReservations()
    {
        if (_isReloading) return;
        _isReloading = true;

        try
        {
            var list = new List<ReservationRow>();
            var raceMap = await LoadRacesMap();

            using var call = _client.GetAllReservations(new Empty());

            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                var r = call.ResponseStream.Current;

                if (!MatchesFilters(r))
                    continue;

                raceMap.TryGetValue(r.RaceId, out var race);

                list.Add(new ReservationRow
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    RaceId = r.RaceId,
                    Name = r.Name,
                    Seats = string.Join(",", r.Seats),
                    Destination = race?.Destination ?? "Unknown",
                    Date = race?.Date ?? "",
                    Time = race?.Time ?? ""
                });
            }

            _grid.DataSource = new BindingList<ReservationRow>(list);
        }
        finally
        {
            _isReloading = false;
        }
    }

    private async Task<Dictionary<int, Race>> LoadRacesMap()
    {
        var map = new Dictionary<int, Race>();

        using var call = _raceClient.GetAllRaces(new Empty());

        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            var r = call.ResponseStream.Current;
            map[r.Id] = r;
        }

        return map;
    }

    private bool MatchesFilters(Reservation r)
    {
        if (!string.IsNullOrWhiteSpace(_nameBox.Text) &&
            !r.Name.Contains(_nameBox.Text, StringComparison.OrdinalIgnoreCase))
            return false;

        if (int.TryParse(_raceIdBox.Text, out int raceId) && r.RaceId != raceId)
            return false;

        if (int.TryParse(_userIdBox.Text, out int userId) && r.UserId != userId)
            return false;

        return true;
    }

    private async Task DeleteSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not ReservationRow row)
            return;

        await _client.DeleteReservationAsync(new IdRequest { Id = row.Id });
        await LoadReservations();
    }

    private sealed class ReservationRow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RaceId { get; set; }

        public string Name { get; set; } = "";
        public string Seats { get; set; } = "";

        public string Destination { get; set; } = "";
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
    }
}