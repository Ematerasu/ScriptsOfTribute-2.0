using System;
using System.Collections.Generic;
using System.Linq;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;

namespace UnityBots
{
    public static class Mapper
    {
        public static MoveDto ToDto(Move move)
        {
            var moveId = move.UniqueId.Value;

            return move switch
            {
                SimpleCardMove scm => new SimpleCardMoveDto
                {
                    Command = scm.Command,
                    MoveId = moveId,
                    CardId = scm.Card.UniqueId.Value
                },
                SimplePatronMove spm => new SimplePatronMoveDto
                {
                    Command = spm.Command,
                    MoveId = moveId,
                    Patron = spm.PatronId
                },
                MakeChoiceMove<UniqueCard> mcc => new MakeChoiceMoveCardDto
                {
                    Command = mcc.Command,
                    MoveId = moveId,
                    CardIds = mcc.Choices.Select(c => c.UniqueId.Value).ToList()
                },
                MakeChoiceMove<UniqueEffect> mce => new MakeChoiceMoveEffectDto
                {
                    Command = mce.Command,
                    MoveId = moveId,
                    EffectNames = mce.Choices.Select(e => SimpleEffectToDto(e)).ToList()
                },
                Move m when m.Command == CommandEnum.END_TURN => new EndTurnMoveDto
                {
                    Command = CommandEnum.END_TURN,
                    MoveId = moveId
                },
                _ => throw new NotSupportedException($"Unknown move type: {move.GetType().Name}")
            };
        }

        public static MoveDto[] ToDtoList(IEnumerable<Move> moves)
        {
            return moves.Select(ToDto).ToArray();
        }

        public static Move? FromDto(MoveDto dto, List<Move> legalMoves)
        {
            return legalMoves.FirstOrDefault(m => m.UniqueId.Value == dto.MoveId);
        }

        public static UniqueEffectDto SimpleEffectToDto(UniqueEffect effect)
        {
            return new UniqueEffectDto
            {
                Type = effect.Type,
                Amount = effect.Amount,
                Combo = effect.Combo,
                OriginCardId = effect.ParentCard.UniqueId.Value
            };
        }

        public static UniqueBaseEffectDto ToDto(UniqueBaseEffect effect)
        {
            return effect switch
            {
                UniqueEffect e => new UniqueEffectDto
                {
                    Type = e.Type,
                    Amount = e.Amount,
                    Combo = e.Combo,
                    OriginCardId = e.ParentCard.UniqueId.Value
                },

                UniqueEffectOr o => new UniqueEffectOrDto
                {
                    Combo = o.Combo,
                    OriginCardId = o.ParentCard.UniqueId.Value,
                    Left = new UniqueEffectDto
                    {
                        Type = o.GetLeft().Type,
                        Amount = o.GetLeft().Amount,
                        Combo = o.GetLeft().Combo,
                        OriginCardId = o.ParentCard.UniqueId.Value
                    },
                    Right = new UniqueEffectDto
                    {
                        Type = o.GetRight().Type,
                        Amount = o.GetRight().Amount,
                        Combo = o.GetRight().Combo,
                        OriginCardId = o.ParentCard.UniqueId.Value
                    }
                },
                _ => throw new NotSupportedException("Unknown effect type")
            };
        }
    }
}
