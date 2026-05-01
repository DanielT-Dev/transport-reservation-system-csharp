using Client.Grpc;
using Grpc.Net.Client;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Client;

public class AddReservationForm : Form
{
    private readonly User _currentUser;
    private readonly int _raceId;
    private readonly string _destination;
    private readonly string _date;
    private readonly string _time;

    private readonly ReservationService.ReservationServiceClient _reservationClient;

    private readonly TextBox txtName = new();
    private readonly FlowLayoutPanel seatsPanel = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();
    private readonly Label lblStatus = new();
    private readonly Label lblRaceInfo = new();

    public AddReservationForm(User currentUser, int raceId, string destination, string date, string time)
    {
        _currentUser = currentUser;
        _raceId = raceId;
        _destination = destination;
        _date = date;
        _time = time;

        Text = "Add Reservation";
        Width = 560;
        Height = 560;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        ForeColor = Color.FromArgb(45, 45, 45);
        Font = new Font("Segoe UI", 10);

        var channel = GrpcChannel.ForAddress("http://localhost:9090");
        _reservationClient = new ReservationService.ReservationServiceClient(channel);

        BuildUi();
        ApplyTheme();

        Load += async (_, _) => await LoadFreeSeatsAsync();
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "Create reservation",
            Left = 20,
            Top = 16,
            Width = 250,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 35, 35)
        };

        lblRaceInfo.Left = 20;
        lblRaceInfo.Top = 48;
        lblRaceInfo.Width = 500;
        lblRaceInfo.Height = 40;
        lblRaceInfo.Text = $"Race: {_destination} | {_date} | {_time} | ID: {_raceId}";
        lblRaceInfo.ForeColor = Color.FromArgb(70, 70, 70);

        var lblName = new Label
        {
            Text = "Client Name",
            Left = 20,
            Top = 100,
            Width = 120,
            ForeColor = Color.FromArgb(70, 70, 70)
        };

        txtName.Left = 20;
        txtName.Top = 125;
        txtName.Width = 500;
        txtName.Height = 30;
        txtName.Text = _currentUser.Name;
        txtName.BorderStyle = BorderStyle.FixedSingle;

        var lblSeats = new Label
        {
            Text = "Available seats",
            Left = 20,
            Top = 175,
            Width = 120,
            ForeColor = Color.FromArgb(70, 70, 70)
        };

        seatsPanel.Left = 20;
        seatsPanel.Top = 200;
        seatsPanel.Width = 500;
        seatsPanel.Height = 260;
        seatsPanel.AutoScroll = true;
        seatsPanel.BackColor = Color.White;
        seatsPanel.BorderStyle = BorderStyle.FixedSingle;
        seatsPanel.WrapContents = true;
        seatsPanel.FlowDirection = FlowDirection.LeftToRight;

        btnSave.Text = "Save";
        btnSave.Left = 20;
        btnSave.Top = 475;
        btnSave.Width = 130;
        btnSave.Height = 42;
        btnSave.Click += BtnSave_Click;

        btnCancel.Text = "Cancel";
        btnCancel.Left = 170;
        btnCancel.Top = 475;
        btnCancel.Width = 130;
        btnCancel.Height = 42;
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        lblStatus.Left = 20;
        lblStatus.Top = 525;
        lblStatus.Width = 500;
        lblStatus.ForeColor = Color.FromArgb(90, 90, 90);

        Controls.Add(title);
        Controls.Add(lblRaceInfo);
        Controls.Add(lblName);
        Controls.Add(txtName);
        Controls.Add(lblSeats);
        Controls.Add(seatsPanel);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        Controls.Add(lblStatus);
    }

    private void ApplyTheme()
    {
        btnSave.BackColor = Color.FromArgb(0, 120, 215);
        btnSave.ForeColor = Color.White;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;

        btnCancel.BackColor = Color.FromArgb(235, 235, 235);
        btnCancel.ForeColor = Color.FromArgb(45, 45, 45);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
    }

    private async Task LoadFreeSeatsAsync()
    {
        try
        {
            lblStatus.Text = "Loading...";
            seatsPanel.Controls.Clear();

            var freeSeats = new List<int>();
            using var call = _reservationClient.GetFreeSeats(new IdRequest { Id = _raceId });

            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                freeSeats.Add(call.ResponseStream.Current.Id);
            }

            if (freeSeats.Count == 0)
            {
                seatsPanel.Controls.Add(new Label
                {
                    Text = "No free seats available.",
                    AutoSize = true
                });

                btnSave.Enabled = false;
                return;
            }

            foreach (var seat in freeSeats.OrderBy(x => x))
            {
                seatsPanel.Controls.Add(new CheckBox
                {
                    Text = $"Seat {seat}",
                    Width = 110,
                    Height = 30,
                    Margin = new Padding(8)
                });
            }

            lblStatus.Text = $"Loaded {freeSeats.Count} seats";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        var name = txtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            lblStatus.Text = "Name required";
            return;
        }

        var seats = seatsPanel.Controls
            .OfType<CheckBox>()
            .Where(c => c.Checked)
            .Select(c => int.Parse(c.Text.Replace("Seat ", "")))
            .ToList();

        if (seats.Count == 0)
        {
            lblStatus.Text = "Select seats";
            return;
        }

        var request = new ReservationCreateRequest
        {
            RaceId = _raceId,
            Name = name
        };
        request.Seats.AddRange(seats);

        await _reservationClient.CreateReservationAsync(request);

        // IMPORTANT:
        // NO manual refresh needed anymore.
        // MainForm WatchReservations stream will update automatically.

        DialogResult = DialogResult.OK;
        Close();
    }
}