using System.Collections.Generic;
using ScriptsOfTribute;

namespace SOISMCTS
{
        public struct PatronTier
        {
            public int favoured;
            public int neutral;
            public int unfavoured;

            public PatronTier(int favoured, int neutral, int unfavoured)
            {
                this.favoured = favoured;
                this.neutral = neutral;
                this.unfavoured = unfavoured;
            }
        }

    public static class PatronTierList
    {
        static readonly Dictionary<PatronId, PatronTier[]> patronTierDict = new Dictionary<PatronId, PatronTier[]> {
            { PatronId.ANSEI,         new PatronTier[] { new(1, 0, -1), new(1, 0, -1), new(1, 0, -1) } },
            { PatronId.DUKE_OF_CROWS, new PatronTier[] { new(-1, 0, 1), new(-1, 0, 1), new(-1, 0, 1) } },
            { PatronId.HLAALU,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.PELIN,         new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.RAJHIN,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.RED_EAGLE,     new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.ORGNUM,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.TREASURY,      new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
            { PatronId.SAINT_ALESSIA, new[] { new PatronTier(0, 0, 0), new PatronTier(0, 0, 0), new PatronTier(0, 0, 0) } },
        };

        public static PatronTier GetPatronTier(PatronId patron, GamePhase gamePhase) => patronTierDict[patron][(int)gamePhase];
    }
}


