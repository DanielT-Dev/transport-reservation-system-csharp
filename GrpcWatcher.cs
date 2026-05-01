using Client.Grpc;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Client;

public static class GrpcWatcher
{
    public static async Task WatchRaces(
        RaceService.RaceServiceClient client,
        Action<Race> onUpdate,
        CancellationToken ct)
    {
        using var call = client.WatchRaces(new Empty());

        while (await call.ResponseStream.MoveNext(ct))
        {
            var ev = call.ResponseStream.Current;
            onUpdate(ev.Race);
        }
    }

    public static async Task WatchReservations(
        ReservationService.ReservationServiceClient client,
        Action<Reservation> onUpdate,
        CancellationToken ct)
    {
        using var call = client.WatchReservations(new Empty());

        while (await call.ResponseStream.MoveNext(ct))
        {
            var ev = call.ResponseStream.Current;
            onUpdate(ev.Reservation);
        }
    }

    public static async Task WatchUsers(
        UserService.UserServiceClient client,
        Action<User> onUpdate,
        CancellationToken ct)
    {
        using var call = client.WatchUsers(new Empty());

        while (await call.ResponseStream.MoveNext(ct))
        {
            var ev = call.ResponseStream.Current;
            onUpdate(ev.User);
        }
    }
}