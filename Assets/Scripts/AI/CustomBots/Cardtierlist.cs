using System;
using System.Collections.Generic;
using ScriptsOfTribute;

namespace UnityBots
{
    public enum TierEnum
    {
        S = 50,
        A = 25,
        B = 10,
        C = 5,
        D = 1,
        UNKNOWN = 0,
    }

    public static class CardTierList
    {
        private static readonly Dictionary<CardId, TierEnum> CardTierDict = new()
        {
            // Hlaalu
            [CardId.CURRENCY_EXCHANGE] = TierEnum.S,
            [CardId.LUXURY_EXPORTS] = TierEnum.S,
            [CardId.OATHMAN] = TierEnum.A,
            [CardId.EBONY_MINE] = TierEnum.B,
            [CardId.HLAALU_COUNCILOR] = TierEnum.B,
            [CardId.HLAALU_KINSMAN] = TierEnum.B,
            [CardId.HOUSE_EMBASSY] = TierEnum.B,
            [CardId.HOUSE_MARKETPLACE] = TierEnum.B,
            [CardId.HIRELING] = TierEnum.C,
            [CardId.HOSTILE_TAKEOVER] = TierEnum.C,
            [CardId.KWAMA_EGG_MINE] = TierEnum.C,
            [CardId.CUSTOMS_SEIZURE] = TierEnum.D,
            [CardId.GOODS_SHIPMENT] = TierEnum.D,

            // Treasury
            [CardId.AMBUSH] = TierEnum.B,
            [CardId.BARTERER] = TierEnum.C,
            [CardId.BLACK_SACRAMENT] = TierEnum.B,
            [CardId.BLACKMAIL] = TierEnum.B,
            [CardId.GOLD] = TierEnum.D,
            [CardId.HARVEST_SEASON] = TierEnum.C,
            [CardId.IMPRISONMENT] = TierEnum.C,
            [CardId.RAGPICKER] = TierEnum.C,
            [CardId.TITHE] = TierEnum.C,
            [CardId.WRIT_OF_COIN] = TierEnum.B,

            // Saint Alessia
            [CardId.ALESSIAN_REBEL] = TierEnum.C,
            [CardId.AYLEID_DEFECTOR] = TierEnum.B,
            [CardId.AYLEID_QUARTERMASTER] = TierEnum.B,
            [CardId.CHAINBREAKER_CAPTAIN] = TierEnum.A,
            [CardId.CHAINBREAKER_SERGEANT] = TierEnum.B,
            [CardId.MORIHAUS_SACRED_BULL] = TierEnum.S,
            [CardId.MORIHAUS_THE_ARCHER] = TierEnum.A,
            [CardId.PELINAL_WHITESTRAKE] = TierEnum.S,
            [CardId.PRIESTESS_OF_THE_EIGHT] = TierEnum.B,
            [CardId.SAINTS_WRATH] = TierEnum.B,
            [CardId.SOLDIER_OF_THE_EMPIRE] = TierEnum.C,
            [CardId.WHITESTRAKE_ASCENDANT] = TierEnum.S,

            // Red Eagle
            [CardId.MIDNIGHT_RAID] = TierEnum.S,
            [CardId.BLOOD_SACRIFICE] = TierEnum.S,
            [CardId.BLOODY_OFFERING] = TierEnum.S,
            [CardId.BONFIRE] = TierEnum.C,
            [CardId.BRIARHEART_RITUAL] = TierEnum.C,
            [CardId.CLANWITCH] = TierEnum.C,
            [CardId.ELDER_WITCH] = TierEnum.B,
            [CardId.HAGRAVEN] = TierEnum.B,
            [CardId.HAGRAVEN_MATRON] = TierEnum.A,
            [CardId.IMPERIAL_PLUNDER] = TierEnum.A,
            [CardId.IMPERIAL_SPOILS] = TierEnum.B,
            [CardId.KARTH_MANHUNTER] = TierEnum.A,
            [CardId.WAR_SONG] = TierEnum.D,

            // Rajhin
            [CardId.BAG_OF_TRICKS] = TierEnum.B,
            [CardId.BEWILDERMENT] = TierEnum.D,
            [CardId.GRAND_LARCENY] = TierEnum.A,
            [CardId.JARRING_LULLABY] = TierEnum.S,
            [CardId.JEERING_SHADOW] = TierEnum.B,
            [CardId.MOONLIT_ILLUSION] = TierEnum.A,
            [CardId.POUNCE_AND_PROFIT] = TierEnum.S,
            [CardId.PROWLING_SHADOW] = TierEnum.B,
            [CardId.RINGS_GUILE] = TierEnum.B,
            [CardId.SHADOWS_SLUMBER] = TierEnum.A,
            [CardId.SLIGHT_OF_HAND] = TierEnum.B,
            [CardId.STUBBORN_SHADOW] = TierEnum.B,
            [CardId.SWIPE] = TierEnum.D,
            [CardId.TWILIGHT_REVELRY] = TierEnum.S,

            // Orgnum
            [CardId.GHOSTSCALE_SEA_SERPENT] = TierEnum.B,
            [CardId.KING_ORGNUMS_COMMAND] = TierEnum.C,
            [CardId.MAORMER_BOARDING_PARTY] = TierEnum.B,
            [CardId.MAORMER_CUTTER] = TierEnum.B,
            [CardId.PYANDONEAN_WAR_FLEET] = TierEnum.B,
            [CardId.SEA_ELF_RAID] = TierEnum.C,
            [CardId.SEA_RAIDERS_GLORY] = TierEnum.C,
            [CardId.SEA_SERPENT_COLOSSUS] = TierEnum.B,
            [CardId.SERPENTGUARD_RIDER] = TierEnum.A,
            [CardId.SERPENTPROW_SCHOONER] = TierEnum.B,
            [CardId.SNAKESKIN_FREEBOOTER] = TierEnum.S,
            [CardId.STORM_SHARK_WAVECALLER] = TierEnum.B,
            [CardId.SUMMERSET_SACKING] = TierEnum.B,

            // Ansei
            [CardId.CONQUEST] = TierEnum.S,
            [CardId.GRAND_ORATORY] = TierEnum.S,
            [CardId.HIRAS_END] = TierEnum.S,
            [CardId.HEL_SHIRA_HERALD] = TierEnum.A,
            [CardId.MARCH_ON_HATTU] = TierEnum.A,
            [CardId.SHEHAI_SUMMONING] = TierEnum.A,
            [CardId.WARRIOR_WAVE] = TierEnum.A,
            [CardId.ANSEI_ASSAULT] = TierEnum.B,
            [CardId.ANSEIS_VICTORY] = TierEnum.B,
            [CardId.BATTLE_MEDITATION] = TierEnum.B,
            [CardId.NO_SHIRA_POET] = TierEnum.C,
            [CardId.WAY_OF_THE_SWORD] = TierEnum.D,

            // Pelin
            [CardId.RALLY] = TierEnum.S,
            [CardId.SIEGE_WEAPON_VOLLEY] = TierEnum.S,
            [CardId.THE_ARMORY] = TierEnum.S,
            [CardId.BANNERET] = TierEnum.A,
            [CardId.KNIGHT_COMMANDER] = TierEnum.A,
            [CardId.REINFORCEMENTS] = TierEnum.A,
            [CardId.ARCHERS_VOLLEY] = TierEnum.B,
            [CardId.LEGIONS_ARRIVAL] = TierEnum.B,
            [CardId.SHIELD_BEARER] = TierEnum.B,
            [CardId.BANGKORAI_SENTRIES] = TierEnum.C,
            [CardId.KNIGHTS_OF_SAINT_PELIN] = TierEnum.C,
            [CardId.THE_PORTCULLIS] = TierEnum.D,
            [CardId.FORTIFY] = TierEnum.D,

            // Crows
            [CardId.BLACKFEATHER_KNAVE] = TierEnum.S,
            [CardId.PLUNDER] = TierEnum.S,
            [CardId.TOLL_OF_FLESH] = TierEnum.S,
            [CardId.TOLL_OF_SILVER] = TierEnum.S,
            [CardId.MURDER_OF_CROWS] = TierEnum.A,
            [CardId.PILFER] = TierEnum.A,
            [CardId.SQUAWKING_ORATORY] = TierEnum.A,
            [CardId.LAW_OF_SOVEREIGN_ROOST] = TierEnum.B,
            [CardId.POOL_OF_SHADOW] = TierEnum.B,
            [CardId.SCRATCH] = TierEnum.B,
            [CardId.BLACKFEATHER_BRIGAND] = TierEnum.C,
            [CardId.BLACKFEATHER_KNIGHT] = TierEnum.C,
            [CardId.PECK] = TierEnum.D,
        };

        public static TierEnum GetCardTier(CardId cardId)
        {
            if (CardTierDict.TryGetValue(cardId, out var tier))
            {
                return tier;
            }
            return TierEnum.UNKNOWN;
        }

        public static void RegisterNewCard(CardId cardId, TierEnum tier)
        {
            if (!CardTierDict.ContainsKey(cardId))
            {
                CardTierDict.Add(cardId, tier);
            }
        }
    }
}
