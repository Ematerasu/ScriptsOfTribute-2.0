using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace UnityBots
{
    public class GrpcBot
    {
        private readonly string _pipeName;

        public GrpcBot(string pipeName = "GrpcBotPipe")
        {
            _pipeName = pipeName;
            GrpcProxyLauncher.LaunchProxyIfNeeded();
        }

        public string Register()
        {
            var response = Send<GenericCommand, RegisterResponse>(
                new GenericCommand { Command = "Register" }
            );

            return response.Name;
        }

        public Move RequestMove(FullGameState fullState, List<Move> legalMoves, TimeSpan timeout)
        {
            var dto = FullGameStateDto.From(fullState);

            var dtoMoves = legalMoves
                .Select(Mapper.ToDto)
                .ToList();

            var request = new PlayRequest
            {
                Command = "Play",
                GameState = dto,
                LegalMoves = dtoMoves,
                TimeoutMs = (long)timeout.TotalMilliseconds
            };

            var response = Send<PlayRequest, PlayResponse>(request);

            var move = legalMoves.Find(m => m.UniqueId.Value == response.MoveId);

            if (move == null)
                throw new InvalidOperationException($"[GrpcBot] Bot zwrócił nieznany ruch MoveId={response.MoveId}");

            return move;
        }

        public PatronId RequestPatron(List<PatronId> patrons, int round)
        {
            var request = new SelectPatronRequest
            {
                Command = "SelectPatron",
                Round = round,
                PatronIds = patrons
            };

            var response = Send<SelectPatronRequest, SelectPatronResponse>(request);
            return response.Selected;
        }

        public void Prepare()
        {
            var _ = Send<GenericCommand, GenericCommand>(
                new GenericCommand { Command = "PregamePrepare" }
            );
        }

        public void NotifyEnd(EndGameState state, FullGameState? finalBoardState)
        {
            var request = new GameEndRequest
            {
                Command = "GameEnd",
                Winner = state.Winner,
                Reason = state.Reason,
                AdditionalContext = state.AdditionalContext
            };

            if (finalBoardState != null)
            {
                request.GameState = FullGameStateDto.From(finalBoardState);
            }

            var _ = Send<GameEndRequest, GenericCommand>(request);

            GrpcProxyLauncher.KillProxy();
        }

        // ======== PIPE LOW-LEVEL ========

        private TResponse Send<TRequest, TResponse>(TRequest request)
        {
            using var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
            pipe.Connect(2000);

            var reqBytes = MessagePackSerializer.Serialize(request);
            pipe.Write(BitConverter.GetBytes(reqBytes.Length));
            pipe.Write(reqBytes);
            pipe.Flush();
            var lenBytes = new byte[4];
            pipe.Read(lenBytes, 0, 4);
            int len = BitConverter.ToInt32(lenBytes, 0);

            var buffer = new byte[len];
            pipe.Read(buffer, 0, len);

            return MessagePackSerializer.Deserialize<TResponse>(buffer);
        }
    }
}
