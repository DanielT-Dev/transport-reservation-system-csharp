# RaceApp – C# gRPC Client

This repository contains the C# client application for a distributed Race Management System built with gRPC. The backend is implemented in Java and exposes services for races, reservations, and users. The client is a WinForms desktop application that communicates with the server using gRPC streaming and unary calls.

---

## Overview

The system is composed of:

- C# WinForms client application
- Java gRPC server application
- Protocol Buffers for service definitions
- gRPC streaming for real-time updates

The client is responsible for displaying and managing:

- Races
- Reservations
- Users

---

## Features

- User authentication via gRPC
- Data visualization using DataGridView controls
- CRUD operations for reservations
- Seat selection per race
- Filtering by race, user, or name
- Real-time synchronization using server streaming

---

## Technologies

### Client (C#)
- .NET WinForms
- gRPC client (Grpc.Net.Client)
- Data binding (BindingList)
- Asynchronous programming (async/await)

### Server (Java)
- Java
- gRPC server (io.grpc)
- DAO-based persistence layer
- BCrypt password hashing

### Communication
- Protocol Buffers (proto3)
- gRPC unary and streaming RPCs
- HTTP/2 transport

---

## Architecture

```
C# WinForms Client
        |
        | gRPC (HTTP/2)
        v
Java gRPC Server
        |
        v
DAO / Database Layer
```

---

## Real-Time Updates

The system uses server streaming RPCs to propagate changes to clients.

Example service definition:

```proto
rpc WatchRaces(Empty) returns (stream Race);
rpc WatchReservations(Empty) returns (stream Reservation);
rpc WatchUsers(Empty) returns (stream User);
```

Client-side implementation example:

```csharp
using var call = _client.GetAllReservations(new Empty());

while (await call.ResponseStream.MoveNext(CancellationToken.None))
{
    var reservation = call.ResponseStream.Current;
    // update UI model
}
```

---

## Reservation Flow

1. User logs in
2. Races are displayed in a grid
3. A race is selected
4. Available seats are loaded via gRPC
5. User selects seats and submits reservation
6. Server updates propagate to all clients

Example reservation creation:

```csharp
var request = new ReservationCreateRequest
{
    RaceId = raceId,
    Name = name
};
request.Seats.AddRange(selectedSeats);

await _reservationClient.CreateReservationAsync(request);
```

---

## Project Structure (Client)

```
Client/
 ├── MainForm.cs
 ├── LoginForm.cs
 ├── ManageReservationsForm.cs
 ├── AddReservationForm.cs
 ├── GrpcWatcher.cs
 ├── Generated gRPC classes
```

---

## Server Responsibilities (Java)

- Manages races, users, and reservations
- Provides gRPC service implementations
- Handles persistence through DAO layer
- Broadcasts updates through streaming RPCs

---

## Key Implementation Notes

Streaming pattern example:

```csharp
while (await call.ResponseStream.MoveNext(CancellationToken.None))
{
    var item = call.ResponseStream.Current;
    // update grid
}
```

Data reload pattern:

```csharp
private async Task LoadReservations()
{
    var list = new List<ReservationRow>();

    using var call = _client.GetAllReservations(new Empty());

    while (await call.ResponseStream.MoveNext(CancellationToken.None))
    {
        var r = call.ResponseStream.Current;
        list.Add(new ReservationRow
        {
            Id = r.Id,
            RaceId = r.RaceId,
            UserId = r.UserId,
            Name = r.Name
        });
    }

    _grid.DataSource = new BindingList<ReservationRow>(list);
}
```

---

## Notes

- The server must run on localhost:9090
- The client depends on generated gRPC classes from the proto definition
- Real-time behavior depends on streaming RPC stability

---

## Future Improvements

- Stronger state management architecture (MVVM-like separation)
- Automatic reconnect for gRPC streams
- Authentication tokens instead of plain credentials
- Improved UI