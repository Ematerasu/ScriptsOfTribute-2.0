using System;
using System.Collections.Generic;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace UnityBots
{
    public class GrpcBotAI : AI
    {
        private readonly GrpcBot _bot;
        private FullGameState? _lastFullGameState;

        private string _name;

        public GrpcBotAI()
        {
            _bot = new GrpcBot();
            _name = _bot.Register();
        }

        public void SetFullGameState(FullGameState state)
        {
            _lastFullGameState = state;
        }

        public override Move Play(GameState state, List<Move> legalMoves, TimeSpan timeout)
        {
            if (_lastFullGameState == null)
                throw new InvalidOperationException("FullGameState was not injected");

            return _bot.RequestMove(_lastFullGameState, legalMoves, timeout);
        }

        public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
            => _bot.RequestPatron(availablePatrons, round);

        public override void PregamePrepare()
            => _bot.Prepare();

        public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
            => _bot.NotifyEnd(state, finalBoardState);
    }
}
