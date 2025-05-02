using System;
using System.Collections.Generic;
using System.Linq;
using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using UnityEngine;

namespace UnityBots
{
    // TODO: Neural Net enhanced heuristic bot
    public class Akame : AI
    {
        private static readonly Dictionary<PatronId, int> PatronTierList = new()
        {
            { PatronId.HLAALU, 5 },
            { PatronId.DUKE_OF_CROWS, 5 },
            { PatronId.ORGNUM, 4 },
            { PatronId.RED_EAGLE, 4 },
            { PatronId.ANSEI, 3 },
            { PatronId.RAJHIN, 2 },
            { PatronId.PELIN, 2 },
            { PatronId.SAINT_ALESSIA, 1 },
        };

        private readonly List<PatronId> _pickedPatrons = new();
        private readonly SeededRandom rng = new SeededRandom();

        public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        {
            PatronId bestPick = availablePatrons[0];
            int bestScore = int.MinValue;

            foreach (var patron in availablePatrons)
            {
                if (patron == PatronId.TREASURY)
                    continue;

                int baseScore = PatronTierList.TryGetValue(patron, out int tier) ? tier : 1;
                int synergyBonus = 0;

                if (_pickedPatrons.Contains(PatronId.HLAALU) && patron == PatronId.DUKE_OF_CROWS)
                    synergyBonus += 2;
                if (_pickedPatrons.Contains(PatronId.DUKE_OF_CROWS) && patron == PatronId.HLAALU)
                    synergyBonus += 2;

                if (_pickedPatrons.Contains(PatronId.PELIN) && patron == PatronId.RED_EAGLE)
                    synergyBonus += 1;
                if (_pickedPatrons.Contains(PatronId.ORGNUM) && patron == PatronId.RAJHIN)
                    synergyBonus -= 1;

                int score = baseScore + synergyBonus;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPick = patron;
                }
            }

            _pickedPatrons.Add(bestPick);

            return bestPick;
        }

        public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
        {
            if (possibleMoves.Count == 1)
                return possibleMoves[0];

            foreach (var move in possibleMoves)
            {
                if (IsInstantMove(move, gameState))
                {
                    Log($"Picking move {move}");
                    return move;
                }
            }
            var randomMove = possibleMoves.PickRandom(rng);
            Log($"Picking move {randomMove}");
            return randomMove;
        }

        private bool IsInstantMove(Move move, GameState gameState)
        {
            if (move is not SimpleCardMove cardMove)
                return false;

            var card = cardMove.Card;

            var activationEffect = card.Effects[0];
            if (activationEffect != null)
            {
                foreach (var baseEffect in activationEffect.Decompose())
                {
                    if (baseEffect is UniqueEffect e)
                    {
                        if (!IsInstantEffect(e.Type))
                            return false;
                    }
                    else if (baseEffect is UniqueEffectOr)
                    {
                        return false;
                    }
                }
            }
            if (gameState.ComboStates.All.TryGetValue(card.Deck, out var comboState))
            {
                int currentCombo = comboState.CurrentCombo;
                for (int i = 1; i < Math.Min(currentCombo + 1, card.Effects.Length); i++)
                {
                    var comboEffect = card.Effects[i];
                    if (comboEffect != null)
                    {
                        foreach (var baseEffect in comboEffect.Decompose())
                        {
                            if (!IsSafeBaseEffect(baseEffect))
                                return false;
                        }
                    }

                    foreach(var hiddenEffect in comboState.All[i])
                    {
                        if (!IsSafeBaseEffect(hiddenEffect))
                            return false;                    
                    }
                }
                
            }

            return true;
        }

        private bool IsSafeBaseEffect(UniqueBaseEffect effect)
        {
            if (effect is UniqueEffect e)
                return IsInstantEffect(e.Type);
            if (effect is UniqueEffectOr)
                return false;
            if (effect is UniqueEffectComposite composite)
                return composite.Decompose().All(IsSafeBaseEffect);
            return false;
        }

        private bool IsInstantEffect(EffectType type)
        {
            return type switch
            {
                EffectType.GAIN_COIN => true,
                EffectType.GAIN_POWER => true,
                EffectType.GAIN_PRESTIGE => true,
                EffectType.OPP_LOSE_PRESTIGE => true,
                EffectType.DRAW => true,
                EffectType.OPP_DISCARD => true,
                EffectType.PATRON_CALL => true,
                EffectType.CREATE_SUMMERSET_SACKING => true,
                EffectType.HEAL => true,
                _ => false,
            };
        }

        public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
        {
            Debug.Log($"Game ended. Result: {state.ToSimpleString()}");
        }
    }
}