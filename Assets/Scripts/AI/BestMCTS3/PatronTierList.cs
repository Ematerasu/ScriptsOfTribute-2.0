using System.Collections.Generic;
using ScriptsOfTribute;

namespace Bots
{
    public struct PatronTier
    {
        public int Favoured;
        public int Neutral;
        public int Unfavoured;

        public PatronTier(int favoured, int neutral, int unfavoured)
        {
            Favoured = favoured;
            Neutral = neutral;
            Unfavoured = unfavoured;
        }
    }

    public static class PatronTierList
    {
        private static readonly Dictionary<PatronId, PatronTier[]> patronTierDict = new Dictionary<PatronId, PatronTier[]>
        {
            { PatronId.ANSEI,         new[] { new PatronTier(1, 0, -1), new PatronTier(1, 0, -1), new PatronTier(1, 0, -1) } },
            { PatronId.DUKE_OF_CROWS, new[] { new PatronTier(-1, 0, 1), new PatronTier(-1, 0, 1), new PatronTier(-1, 0, 1) } },
            { PatronId.HLAALU,        new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.PELIN,         new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.RAJHIN,        new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.RED_EAGLE,     new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.ORGNUM,        new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.TREASURY,      new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
            { PatronId.SAINT_ALESSIA, new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
        };

        public static PatronTier GetPatronTier(PatronId patron, GamePhase gamePhase)
        {
            return patronTierDict[patron][(int)gamePhase];
        }
    }
}
