# LudoGameNET: SignalR Multiplayer Context (Compact)

## 🏗️ Architecture Decisions
- **Room Management (`IRoomManager`)**: In-memory state. Rooms are created with a 6-character code. `RoomCleanupService` sweeps inactive rooms (>15 mins).
- **Grace Period & Reconnect**: Players disconnecting trigger a 30-60s grace period. They can rejoin using the `RoomCode` and resume via `RequestFullState()`.
- **State Synchronization**: 
  - **Full Snapshot**: Broadcasted on `GameStarted` and `RoomStateUpdated` to ensure consistency. Includes a `Version` field.
  - **Delta Events**: `DiceRolled` and `PieceMoved` are used primarily to trigger frontend UI animations.
- **Seat Assignment**: Controlled via atomic `TryClaimSeat` (locks). Map is `ConnectionId` ↔ `PlayerColor`. Host migration delegates the Host role to the next player based on join order.
- **Turn Timeout**: Backend enforces a 30s turn timer. Auto-skips roll on timeout, or auto-picks first valid piece if timeout during movement. Zombie pieces stay as obstacles.

## ✅ Work Completed (Tickets 1 to 3)
1. **Backend Domain**: `RoomSession.cs` & `RoomManager.cs` implemented. Single-instance hotseat state (`IGameManager.cs`) removed.
2. **SignalR Hub**: `LudoHub.cs` created to handle WS connections, `CreateRoom`, `JoinRoom`, `TryClaimSeat`, `StartGame`, `RollDice`, and `MovePiece`. REST controllers (`GameController.cs`, `DevController.cs`) removed. `Program.cs` wired with SignalR & DI.
3. **Frontend Client**: `@microsoft/signalr` installed. `useLudoHub.js` created to wrap WebSocket connection and hub invocations.

## 🚀 Next Phase: Ticket 4 (UI Lobby & Papan)
- **Objective**: Wire the frontend to the new SignalR backend.
- **Target Components**: 
  - Delete/Rewrite current hotseat setup screen.
  - Create **`LobbySetup.jsx`**: UI for "Create Room" or "Join Room via Code".
  - Create **`SeatSelection.jsx`**: UI to claim a color (`Red`/`Green`/`Blue`/`Yellow`).
  - Refactor **`useGameState.js`**: Remove old `gameApi` REST calls. Subscribe to `useLudoHub` events (`RoomStateUpdated`, `DiceRolled`, `PieceMoved`) to trigger board animations and update local React state.

---
**Instruction for new session:** Read this file, understand the architecture, and immediately propose the implementation strategy for Ticket 4.
