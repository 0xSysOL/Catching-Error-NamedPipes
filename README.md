# IPC Error Logging Engine using Named Pipes

A lightweight, high-throughput Inter-Process Communication (IPC) error logging system built with `.NET 8`. It establishes a persistent connection between a client application (e.g., HR Client) and a background server worker using sequential, non-overlapping binary stream transmission.

## Core Features
* **Persistent Stream Transmission:** Transmits log streams over a single continuous pipe instance using raw byte length prefixes, avoiding the overhead of connection handshakes for every message.
* **Thread-Safe local Buffering:** Utilizes a `ConcurrentQueue` on the client-side alongside a `SemaphoreSlim` (capacity = 1) to regulate data flow during concurrent production and preserve strict FIFO ordering.
* **Fault Tolerance & Recovery:** If the server disconnects, client threads gracefully fallback to buffering inputs locally without application crashes, automatically clearing the backlog upon server reconnection.
* **Adaptive Server Loop:** The server utilizes connection state tracking (`IsConnected`) to differentiate between short-lived memory idle gaps and full client process shutdowns.
* **Graceful Shutdown:** Implements `CancellationToken` in the server loop to flush any remaining in-memory queues before releasing system stream resources.

# IPC Error Logging Engine using Named Pipes

A lightweight, high-throughput Inter-Process Communication (IPC) error logging system built with `.NET 8`. It establishes a persistent connection between a client application (e.g., HR Client) and a background server worker using sequential, non-overlapping binary stream transmission.

## Core Features
* **Persistent Stream Transmission:** Transmits log streams over a single continuous pipe instance using raw byte length prefixes, avoiding the overhead of connection handshakes for every message.
* **Thread-Safe local Buffering:** Utilizes a `ConcurrentQueue` on the client-side alongside a `SemaphoreSlim` (capacity = 1) to regulate data flow during concurrent production and preserve strict FIFO ordering.
* **Fault Tolerance & Recovery:** If the server disconnects, client threads gracefully fallback to buffering inputs locally without application crashes, automatically clearing the backlog upon server reconnection.
* **Adaptive Server Loop:** The server utilizes connection state tracking (`IsConnected`) to differentiate between short-lived memory idle gaps and full client process shutdowns.
* **Graceful Shutdown:** Implements `CancellationToken` in the server loop to flush any remaining in-memory queues before releasing system stream resources.

## Solution Architecture
* **Data_Layer.Errore_Catching (Client):** Implements `PipeOptions.WriteThrough` for unbuffered byte transmission, preventing system caching delays.
* **ErrorLoggingService (Server):** Runs as a `BackgroundService` that processes messages using immediate `Dequeue()` extraction to maintain stable memory footprints under load.
## Solution Architecture
* **Data_Layer.Errore_Catching (Client):** Implements `PipeOptions.WriteThrough` for unbuffered byte transmission, preventing system caching delays.
* **ErrorLoggingService (Server):** Runs as a `BackgroundService` that processes messages using immediate `Dequeue()` extraction to maintain stable memory footprints under load.
