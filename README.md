# LudoGameNET 🎲

A full-stack implementation of **Ludo** (a Parcheesi-style board game): a REST API backend built with **ASP.NET Core 8**, and a **React + Vite + Tailwind** frontend with an interactive board, animations, sound effects, and a **DevTools** panel for manual debugging (forcing dice rolls, teleporting pieces, etc.).

```
LudoGameNET/
├── Backend/
│   ├── LudoGameNET.Api/      # REST API (ASP.NET Core 8)
│   └── LudoGameNET.Tests/    # Unit tests (xUnit)
├── Frontend/
│   └── ludo-game-net/        # React SPA (Vite)
└── LudoGameNET.sln
```

---

## Table of Contents

- [Features](#features)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Game Rules](#game-rules)
- [Running the Project](#running-the-project)
  - [Prerequisites](#prerequisites)
  - [Running the Backend](#running-the-backend)
  - [Running the Frontend](#running-the-frontend)
- [Configuration](#configuration)
- [REST API](#rest-api)
  - [Gameplay Endpoints](#gameplay-endpoints)
  - [DevTools Endpoints (dev only)](#devtools-endpoints-dev-only)
- [Backend Code Structure](#backend-code-structure)
- [Frontend Code Structure](#frontend-code-structure)
- [DevTools Panel](#devtools-panel)
- [Edge Cases Worth Testing](#edge-cases-worth-testing)
- [Testing](#testing)
- [Class Diagram](#class-diagram)
- [Asset Credits](#asset-credits)
- [Roadmap / Future Ideas](#roadmap--future-ideas)

---

## Features

- ✅ 2–4 players, 4 pieces per player, standard 15×15 Ludo board.
- ✅ Full rule set: entering the board requires a 6, capturing opponent pieces, safe squares, private home stretches, and the "three consecutive sixes forfeits the turn" rule.
- ✅ The backend is stateless per request but keeps **one active game** in memory (single-session, ideal for a hobby project, demo, or local hotseat play).
- ✅ Frontend: SVG/DOM board rendering with step-by-step piece walking animation, a 3D dice, sound effects (roll, capture, finish, victory), a winner modal, and a dark theme.
- ✅ **DevTools panel** (development-only): force dice values, send all pieces of a color out of Base at once, send all pieces straight to Goal (instant win), teleport a single piece to any state/position, jump turns, and set the consecutive-sixes counter — all to reproduce edge cases without having to play through them manually.

## Architecture & Tech Stack

| Layer      | Technology |
|------------|-----------|
| Backend    | ASP.NET Core 8 Web API (C#), Swagger/Swashbuckle, xUnit for tests |
| Frontend   | React 18, Vite 5, Tailwind CSS, lucide-react (icons), GSAP (cursor animation) |
| Communication | Plain REST JSON (`fetch`), CORS `AllowAll` for easy local development |
| State      | Backend: one in-memory `LudoGame` per process (via an `IGameManager` singleton). Frontend: React state (`useGameState` hook) resynced with server state after each action |

The backend and frontend talk over plain HTTP — there's no SignalR/WebSocket, so this is **hotseat / single-machine play** (all players take turns on the same device), not real-time online multiplayer.

## Game Rules

1. Each player has 4 pieces that start in their own **Base** (yard).
2. A piece can only leave Base and enter the board when the dice shows a **6**.
3. Pieces move clockwise along the shared **common track** (51 steps), offset per color, then enter their own **home stretch** (6 steps) leading to their **Goal**.
4. A piece needs an **exact** roll to enter Goal — if the dice value would overshoot, that piece simply can't move (other pieces are checked instead).
5. Landing on a square occupied by an opponent's piece **captures** it — the opponent's piece is sent back to Base — **unless** that square is a **safe** square (Safe, Yard, HomeStretch, or Goal).
6. Rolling a **6** grants an extra roll. However, rolling six **three times in a row** forces the turn to pass to the next player anyway (to prevent an infinite loop).
7. If no piece can legally move with the rolled value, the turn passes automatically (unless the roll was a 6, in which case the turn stays but nothing happens).
8. A player wins once **all four** of their pieces reach Finished (Goal).

> This implementation does **not** enforce a *blockade* rule (two of your own pieces stacked together blocking an opponent from passing) — that's a deliberate design choice, not a bug. See [Edge Cases](#edge-cases-worth-testing) below for the full list of relevant boundary cases.

## Running the Project

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm

### Running the Backend

```bash
cd Backend/LudoGameNET.Api
dotnet restore
dotnet run
```

By default the backend runs on **`http://localhost:5286`** (the `http` profile in `Properties/launchSettings.json`, which automatically sets the environment to `Development`). Open `http://localhost:5286/swagger` for interactive Swagger docs.

> The `Development` environment matters: the DevTools endpoints (`/api/game/dev/*`) are only active when `ASPNETCORE_ENVIRONMENT=Development` — exactly the default condition for a local `dotnet run`.

### Running the Frontend

```bash
cd Frontend/ludo-game-net
npm install
npm run dev
```

Open the URL Vite prints (default `http://localhost:5173`). Make sure the backend is already running.

Production build:

```bash
npm run build      # outputs to dist/
npm run preview    # preview the build
```

## Configuration

The frontend reads the API base URL from a Vite environment variable, set in `Frontend/ludo-game-net/.env`:

```env
VITE_API_BASE_URL=http://localhost:5286/api/game
```

Change this if the backend runs on a different host/port.

The DevTools panel on the frontend is controlled by `Frontend/ludo-game-net/src/config/devtools.js`:

```js
export const DEV_TOOLS_ENABLED =
  import.meta.env.DEV || import.meta.env.VITE_ENABLE_DEVTOOLS === 'true';
```

- Automatically **on** during `npm run dev`.
- Automatically **off** on a production `npm run build`, unless you build with `VITE_ENABLE_DEVTOOLS=true npm run build`.
- The backend has its own protection too (see above), so even if a production frontend build were forced to show the DevTools button, the endpoints behind it would still be rejected (404) unless the API itself is running in the Development environment.

## REST API

Base URL: `{VITE_API_BASE_URL}` (default `http://localhost:5286/api/game`).

### Gameplay Endpoints

| Method | Path | Body / Query | Description |
|---|---|---|---|
| `POST` | `/api/game` | `{ colors: ["Red","Blue",...] }` | Start a new game (2–4 players, distinct colors). Replaces any existing game. |
| `GET`  | `/api/game` | — | Get the current game's full state. |
| `GET`  | `/api/game/current-player` | — | Get the player whose turn it currently is. |
| `POST` | `/api/game/roll` | — | Roll the dice for the current player; returns the dice value plus which pieces can legally move. Auto-passes the turn if no piece is valid. |
| `GET`  | `/api/game/valid-pieces` | `?playerId=&diceValue=` | Check which pieces are valid to move for a given player/dice combination. |
| `POST` | `/api/game/move` | `{ pieceId, diceValue }` | Move the current player's piece; handles capture/finish/turn-passing rules automatically. |
| `GET`  | `/api/game/board` | — | Get all 225 board squares along with which pieces occupy them. |
| `GET`  | `/api/game/square` | `?row=&column=` | Get a single specific square. |

All enums (`PlayerColor`, `PieceState`, `GameState`, `SquareType`) are serialized as **strings** (e.g. `"Red"`, `"OnBoard"`), not numbers.

### DevTools Endpoints (dev only)

> ⚠️ Every endpoint below returns `404 Not Found` unless `ASPNETCORE_ENVIRONMENT=Development` (the default for a local `dotnet run`). They can never be reached from a real deployment.

| Method | Path | Body | Description |
|---|---|---|---|
| `GET`  | `/api/game/dev/dice` | — | Read the current forced-dice status (`forcedValue`, `locked`). |
| `POST` | `/api/game/dev/dice` | `{ value: 1-6 \| null, lock: bool }` | Force the next roll's result. `lock: true` keeps forcing it on every subsequent roll until cleared. |
| `POST` | `/api/game/dev/dice/clear` | — | Clear any forced dice value, returning to normal random rolls. |
| `POST` | `/api/game/dev/enter-all` | `{ color }` | Send **every** Base piece of that color onto the board at once (bypasses the "must roll a 6" rule). |
| `POST` | `/api/game/dev/finish-all` | `{ color }` | Send every piece of that color straight to Goal. Automatically ends the game (declares a winner) if that completes the player. |
| `POST` | `/api/game/dev/reset-base` | `{ color }` | The reverse of `enter-all`: sends every piece of that color back to Base. |
| `POST` | `/api/game/dev/force-piece` | `{ color, pieceId, state, pathIndex? }` | **Generic** tool: teleports a single piece to any state/position. `pathIndex` is required (0..50) when `state = "OnBoard"`. |
| `POST` | `/api/game/dev/set-turn` | `{ playerIndex }` | Jump straight to a given player's turn. |
| `POST` | `/api/game/dev/set-sixes` | `{ count }` | Directly set the `ConsecutiveSixes` counter (to test the forfeit-on-third-six rule). |

Every mutating endpoint (except the dice ones) returns the latest `GameStateDto` — the exact same shape as `GET /api/game`.

## Backend Code Structure

```
Backend/LudoGameNET.Api/
├── Controllers/
│   ├── GameController.cs     # Official gameplay endpoints
│   └── DevController.cs      # Debug-only endpoints (dev environment only, see above)
├── Game/IGameManager.cs      # Singleton holding the one active LudoGame
├── Models/
│   ├── LudoGame.cs           # Core rules engine (board, paths, turns, capture, dev helpers)
│   ├── Board.cs / Square.cs  # 15×15 board representation
│   ├── Player.cs / Piece.cs  # Player & piece data
│   ├── Dice.cs                # Holder for the last dice value
│   └── Point.cs               # (row, column) coordinate
├── Interfaces/                # IBoard, IPlayer, IPiece, IDice — abstractions for testability
├── Enums/                     # GameState, PieceState, PlayerColor, SquareType
├── DTOs/GameDtos.cs           # All request/response DTOs (including DevTools DTOs)
├── Mapping/GameStateMapper.cs # Shared Model → DTO mapper (used by both controllers)
└── ClassDiagram/               # Class diagram (Mermaid + PNG)
```

`LudoGame` is the **single source of truth** for all the rules: board construction, per-color paths (with each color's start offset), move validation, capture, turn handling, and — for DevTools — a set of `Dev*` methods that intentionally bypass all normal validation, clearly separated with comments in the file so they never get mixed up with real gameplay logic.

## Frontend Code Structure

```
Frontend/ludo-game-net/src/
├── api/gameApi.js              # All REST API calls (including the dev* functions)
├── hooks/useGameState.js       # Main game-screen state management (roll, move, sync with server)
├── lib/
│   ├── boardGeometry.js        # Converts board (row, col) ↔ rendered position
│   └── gameLogic.js            # Piece position & walk-animation helpers
├── components/
│   ├── board/                  # Board, Cell, PieceLayer
│   ├── dice/Dice3D.jsx         # Animated 3D dice component
│   ├── sidebar/                # TurnPanel, PlayersList
│   ├── setup/PlayerSetup.jsx   # Player/color selection screen
│   ├── common/                 # Header, ErrorBanner, WinnerModal
│   └── devtools/DevTools.jsx   # DevTools panel (see below)
├── constants/                  # Colors, board geometry
├── config/devtools.js          # DevTools on/off switch
└── sounds.js                   # Sound-effect wrapper
```

## DevTools Panel

A floating bug 🐞 button in the bottom-right corner (only shown when `DEV_TOOLS_ENABLED`, see [Configuration](#configuration)). It has 4 tabs:

- **state** — a read-only snapshot of the current React state (can be copied or logged to console).
- **actions** — manual controls:
  - *Manual dice*: force a value 1–6 for the next roll, optionally lock it, or hit "Set & roll now".
  - *Per-color actions*: send all pieces of a color out of Base, send them all to Goal (instant win), or reset them back to Base.
  - *Teleport a piece*: the most flexible tool — pick a color, piece id (0–3), state (`Base`/`OnBoard`/`Finished`), and any `pathIndex` for `OnBoard`. Great for setting up any edge case (two pieces about to collide, one step from Goal, etc.).
  - *Turn & sixes*: jump to a specific player's turn, or force the "consecutive sixes" counter to test the third-six forfeit rule.
- **server** — manually fetch state straight from `GET /api/game` to compare against the React state.
- **logs** — a history of the last 20 actions (from DevTools or manual fetches) with their payloads.

## Edge Cases Worth Testing

Boundary cases relevant to the implemented rules — all reproducible manually via the DevTools panel:

1. **Entering the board** — a Base piece is only valid to move when the dice is 6; a full Base (4 pieces) getting consecutive sixes, and which piece is considered valid first.
2. **Three-six forfeit** — rolling six 3× in a row must force the turn to pass even though another six was rolled (`Set consecutiveSixes = 2`, then force a 6).
3. **Normal capture** — landing on a square with an opponent's piece sends it back to Base.
4. **Capture on a safe square is blocked** — capture must **not** happen on `Safe`, `Yard`, `HomeStretch`, or `Goal` squares.
5. **No same-color captures** — two pieces of the same color stacked on one square never capture each other.
6. **Overshoot is rejected** — a piece can't move past the last square of its path; it needs an exact count to reach Goal.
7. **Finishing exactly on the last square** — `pathIndex` == path length - 1 → state becomes `Finished`.
8. **Winning** — all four of a player's pieces `Finished` → the game ends and `WinnerColor` is set.
9. **No legal move at all** — if no piece can move with the rolled value, the turn passes automatically (unless it was a 6, in which case the turn stays but nothing happens).
10. **No blockade rule** — this design does **not** prevent an opponent from passing through a square stacked with 2+ of your own pieces (a deliberate rule variation, not a bug).
11. **Common-track wrap-around** — pieces of colors with different start offsets stay consistent as they loop around the 52-square `CommonPath` before entering their own home stretch.
12. **Locked dice (dev-only)** — a locked forced dice value is still counted normally by the consecutive-sixes counter, even though it isn't a genuinely random roll.
13. **Resetting the game** — starting a new game must fully discard all previous state (including any DevTools dice override), since `GameManager.CreateGame` creates a brand-new `LudoGame` instance.

## Testing

Backend unit tests live in `Backend/LudoGameNET.Tests` (xUnit), covering: game construction, board & path rules, movement rules (`CanEnterBoard`, `CanMove`, capture, etc.), turn handling, and supporting models.

```bash
cd Backend/LudoGameNET.Tests
dotnet test
```

## Class Diagram

The full class diagram lives in `Backend/LudoGameNET.Api/ClassDiagram/` (`ClassDiagram.mmd` is the Mermaid source, `ClassDiagram.png` is the rendered image) — it shows the relationships between `LudoGame`, `Board`, `Square`, `Player`, `Piece`, `Dice`, and their supporting interfaces and enums.

## Asset Credits

Sound effects live in `Frontend/ludo-game-net/public/sfx/` (see the `README.md` in that folder for their source/license).

## Roadmap / Future Ideas

- [ ] Real-time multiplayer (SignalR/WebSocket) — currently pure REST-based hotseat play.
- [ ] Game persistence (all state is currently lost when the backend process restarts).
- [ ] Optional rules: blockades, double captures, tournament mode.
- [ ] AI/bot mode for solo play.
