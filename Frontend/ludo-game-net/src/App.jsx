import { useState } from 'react';
import TargetCursor from './components/TargetCursor/TargetCursor.jsx';
import Header from './components/common/Header.jsx';
import ErrorBanner from './components/common/ErrorBanner.jsx';
import WinnerModal from './components/common/WinnerModal.jsx';
import AboutModal from './components/common/AboutModal.jsx';
import LobbySetup from './components/setup/LobbySetup.jsx';
import SeatSelection from './components/setup/SeatSelection.jsx';
import Board from './components/board/Board.jsx';
import TurnPanel from './components/sidebar/TurnPanel.jsx';
import PlayersList from './components/sidebar/PlayersList.jsx';
import { useGameState } from './hooks/useGameState.js';

export default function App() {
  const [showAbout, setShowAbout] = useState(false);
  const gameStateApi = useGameState();
  const {
    screen, gameState, diceValue, diceDisplayValue, rollToken, validPieces,
    rolling, busy, error, muted, cellGroups, validIds, canRoll, currentPlayer,
    room, colorConnections, myConnectionId, hostConnectionId,
    toggleMuted, createRoom, joinRoom, claimSeat, startGame, rollDice, movePiece, resetGame,
  } = gameStateApi;

  // Handle enums from backend
  const isFinished = gameState?.state === 'Finished' || gameState?.state === 2;

  return (
    <div
      style={{
        fontFamily: "'Inter', sans-serif",
        backgroundImage:
          'linear-gradient(rgba(10, 22, 12, 0.55), rgba(10, 22, 12, 0.55)), url(/textures/grass.jpg)',
        backgroundSize: '260px 260px',
        backgroundRepeat: 'repeat',
      }}
      className="w-full min-h-screen flex flex-col items-center p-4 sm:p-6"
    >
      <TargetCursor spinDuration={2} hideDefaultCursor={true} parallaxOn={true} />

      <Header
        screen={screen}
        muted={muted}
        onToggleMuted={toggleMuted}
        onResetGame={resetGame}
        onShowAbout={() => setShowAbout(true)}
      />

      <ErrorBanner message={error} />

      {screen === 'lobby' && (
        <LobbySetup
          onCreateRoom={createRoom}
          onJoinRoom={joinRoom}
          busy={busy}
          error={error}
        />
      )}

      {screen === 'seatSelection' && room && (
        <SeatSelection
          roomCode={room.roomCode}
          colorConnections={colorConnections}
          myConnectionId={myConnectionId}
          hostConnectionId={hostConnectionId}
          onClaimSeat={claimSeat}
          onStartGame={startGame}
          busy={busy}
        />
      )}

      {screen === 'game' && gameState && (
        <div className="w-full max-w-5xl flex flex-col lg:flex-row gap-5">
          <div className="flex-1 flex justify-center">
            <Board
              cellGroups={cellGroups}
              currentColor={currentPlayer?.color}
              validIds={validIds}
              onPieceClick={movePiece}
              canClick={diceValue != null}
            />
          </div>

          <div className="w-full lg:w-72 flex flex-col gap-4">
            <TurnPanel
              currentPlayer={currentPlayer}
              diceDisplayValue={diceDisplayValue}
              rollToken={rollToken}
              rolling={rolling}
              busy={busy}
              canRoll={canRoll}
              onRollDice={rollDice}
              showHint={diceValue != null && validPieces.length > 0}
            />
            <PlayersList players={gameState.players} currentPlayerIndex={gameState.currentPlayerIndex} />
          </div>
        </div>
      )}

      <WinnerModal
        winnerColor={isFinished ? gameState.winnerColor : null}
        onPlayAgain={resetGame}
      />

      <AboutModal open={showAbout} onClose={() => setShowAbout(false)} />
    </div>
  );
}
