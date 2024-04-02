using System.Security.Cryptography.X509Certificates;

namespace Gazillionaire.Models;

public class Game
{
    public string Status => TurnState.ToString();
    private readonly IWinCondition _winCondition;
    private readonly GameState _gameState;
    private TurnState TurnState;
    private readonly Dictionary<TurnState, Func<TurnState>> _stateMachine;

    public Game()
    {
        TurnState = TurnState.Setup;
        _winCondition = new CashBalanceWinCondition(1_000_000_000);
        _stateMachine = new Dictionary<TurnState, Func<TurnState>>
        {
            { TurnState.Setup, SetupGame },
            { TurnState.Preparing, PrepareGame },
            { TurnState.Turn, Turn },
            { TurnState.Evaluating, Evaluate },
            { TurnState.Won, SetupGame },
            { TurnState.Lost, SetupGame }
        };
        _gameState = new();
    }

    public void AdvanceState()
    {
        TurnState = _stateMachine[TurnState]();
    }

    public void CheckForWinCondition()
    {
        if (_winCondition.IsMet(_gameState))
        {
            return;
        }
    }

    public TurnState SetupGame()
    {
        return TurnState.Preparing;
    }

    public TurnState PrepareGame()
    {
        return TurnState.Turn;
    }

    public TurnState Turn()
    {
        // do the turn
        return TurnState.Evaluating;
    }

    public TurnState Evaluate()
    {
        if (_winCondition.IsMet(_gameState))
        {
            // Determine victory/loss
            return TurnState.Won;
        }
        return TurnState.Turn;
    }
}



internal interface IWinCondition
{
    public bool IsMet(GameState gameState);
}

internal class CashBalanceWinCondition : IWinCondition
{
    private readonly decimal _cashLimit;

    public CashBalanceWinCondition(decimal cashLimit)
    {
        _cashLimit = cashLimit;
    }

    public bool IsMet(GameState gameState)
    {
        return gameState.AnyPlayerOverCashLimit(_cashLimit);
    }
}

internal class GameState
{
    private readonly List<Player> _players = new();
    private readonly int _turnsElapsed = 0;

    public bool AnyPlayerOverCashLimit(decimal cashLimit)
    {
        return _players.Any(p => p.Cash > cashLimit);
    }

    public bool AllTurnsElapsed(int maxTurns)
    {
        return _turnsElapsed >= maxTurns;
    }
}

public enum TurnState
{
    Setup,
    Preparing,
    Turn,
    Evaluating,
    Won,
    Lost
}

internal class Player
{
    public string Name;
    public Dictionary<Item, decimal> Inventory;
    public decimal Cash;
}
