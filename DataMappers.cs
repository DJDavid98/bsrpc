using System.Text;
using DataPuller.Data;
using IPALogger = IPA.Logging.Logger;

namespace bsrpc
{
    internal class DataMappers
    {
        internal static string GetPlayState()
        {
            if (MapData.Instance.LevelPaused)
            {
                return "⏸️";
            }

            if (MapData.Instance.LevelFailed)
            {
                return "☠️";
            }

            if (MapData.Instance.LevelFinished)
            {
                return "🎉";
            }

            if (LiveData.Instance.TimeElapsed > 0)
            {
                return "▶️";
            }

            return "⌛";
        }

        internal static string GetReadableRank(string originalRank)
        {
            switch (originalRank)
            {
                case "SSS":
                    // Match in-game rank shown during zen mode
                    return "E";
                default:
                    return originalRank;
            }
        }

        internal static string GetReadableDifficulty(string originalDifficulty)
        {
            switch (originalDifficulty)
            {
                case "ExpertPlus":
                    return "Expert+";
                default:
                    return originalDifficulty;
            }
        }

        internal static string GetModifiersState()
        {
            var modifiersString = new StringBuilder();
            if (LiveData.Instance.TimeElapsed > 0)
            {
                if (MapData.Instance.Modifiers.ZenMode)
                {
                    modifiersString.Append("🧘");
                }
                else
                {
                    // Life modifiers
                    if (MapData.Instance.Modifiers.NoFailOn0Energy || MapData.Instance.Modifiers.OneLife ||
                        MapData.Instance.Modifiers.FourLives)
                    {
                        if (MapData.Instance.Modifiers.NoFailOn0Energy)
                        {
                            modifiersString.Append("🛡");
                        }
                        else if (MapData.Instance.Modifiers.OneLife)
                        {
                            modifiersString.Append("1🤍");
                        }
                        else if (MapData.Instance.Modifiers.FourLives)
                        {
                            modifiersString.Append("4🤍");
                        }

                        modifiersString.Append(" ");
                    }

                    // Difficulty decreasing modifiers
                    if (MapData.Instance.Modifiers.NoBombs || MapData.Instance.Modifiers.NoWalls ||
                        MapData.Instance.Modifiers.NoArrows)
                    {
                        modifiersString.Append("🚫");
                        if (MapData.Instance.Modifiers.NoBombs)
                        {
                            modifiersString.Append("💣");
                        }

                        if (MapData.Instance.Modifiers.NoWalls)
                        {
                            modifiersString.Append("🧱");
                        }

                        if (MapData.Instance.Modifiers.NoArrows)
                        {
                            modifiersString.Append("🔽");
                        }

                        modifiersString.Append(" ");
                    }

                    // Difficulty increasing modifiers
                    if (MapData.Instance.Modifiers.GhostNotes || MapData.Instance.Modifiers.DisappearingArrows)
                    {
                        if (MapData.Instance.Modifiers.GhostNotes)
                        {
                            modifiersString.Append("👻");
                        }
                        else if (MapData.Instance.Modifiers.DisappearingArrows)
                        {
                            modifiersString.Append("🟦");
                        }

                        modifiersString.Append(" ");
                    }

                    if (MapData.Instance.Modifiers.SmallNotes || MapData.Instance.Modifiers.ProMode ||
                        MapData.Instance.Modifiers.StrictAngles)
                    {
                        if (MapData.Instance.Modifiers.SmallNotes)
                        {
                            modifiersString.Append("🔹");
                        }

                        if (MapData.Instance.Modifiers.ProMode)
                        {
                            modifiersString.Append("⚔️");
                        }

                        if (MapData.Instance.Modifiers.StrictAngles)
                        {
                            modifiersString.Append("📐");
                        }

                        modifiersString.Append(" ");
                    }
                }

                // Speed modifiers
                if (MapData.Instance.Modifiers.SlowerSong)
                {
                    modifiersString.Append("🐌");
                }
                else if (MapData.Instance.Modifiers.FasterSong)
                {
                    modifiersString.Append("⏩");
                }

                if (MapData.Instance.Modifiers.SuperFastSong)
                {
                    modifiersString.Append("⏩");
                }
            }

            return modifiersString.ToString().Trim();
        }

        internal static string GetEnvironmentImage()
        {
            switch (MapData.Instance.Environment)
            {
                case "TheWeekndEnvironment":
                    return RichPresenceAssetKeys.TheWeekend;
                case "LizzoEnvironment":
                    return RichPresenceAssetKeys.Lizzo;
                case "TheSecondEnvironment":
                    return RichPresenceAssetKeys.TheSecond;
                case "EDMEnvironment":
                    return RichPresenceAssetKeys.Edm;
                case "PyroEnvironment":
                    return RichPresenceAssetKeys.FallOutBoy;
                case "WeaveEnvironment":
                    return RichPresenceAssetKeys.Weave;
                case "GagaEnvironment":
                    return RichPresenceAssetKeys.Gaga;
                case "HalloweenEnvironment":
                    return RichPresenceAssetKeys.Spooky;
                case "BillieEnvironment":
                    return RichPresenceAssetKeys.Billie;
                case "SkrillexEnvironment":
                    return RichPresenceAssetKeys.Skrillex;
                case "InterscopeEnvironment":
                    return RichPresenceAssetKeys.Interscope;
                case "KaleidoscopeEnvironment":
                    return RichPresenceAssetKeys.Kaleidoscope;
                case "BTSEnvironment":
                    return RichPresenceAssetKeys.Bts;
                case "LinkinParkEnvironment":
                    return RichPresenceAssetKeys.LinkinPark;
                case "FitBeatEnvironment":
                    return RichPresenceAssetKeys.FitBeat;
                case "TimbalandEnvironment":
                    return RichPresenceAssetKeys.Timbaland;
                case "GreenDayEnvironment":
                    return RichPresenceAssetKeys.GreenDay;
                case "GreenDayGrenadeEnvironment":
                    return RichPresenceAssetKeys.GreenDayGrenade;
                case "RocketEnvironment":
                    return RichPresenceAssetKeys.Rocket;
                case "PanicEnvironment":
                    return RichPresenceAssetKeys.Panic;
                case "CrabRaveEnvironment":
                    return RichPresenceAssetKeys.CrabRave;
                case "MonstercatEnvironment":
                    return RichPresenceAssetKeys.Monstercat;
                case "KDAEnvironment":
                    return RichPresenceAssetKeys.Kda;
                case "DragonsEnvironment":
                    return RichPresenceAssetKeys.Dragons;
                case "BigMirrorEnvironment":
                    return RichPresenceAssetKeys.BigMirror;
                case "NiceEnvironment":
                    return RichPresenceAssetKeys.Nice;
                case "TriangleEnvironment":
                    return RichPresenceAssetKeys.Triangle;
                case "OriginsEnvironment":
                    return RichPresenceAssetKeys.Origins;
                case "DefaultEnvironment":
                default:
                    return RichPresenceAssetKeys.TheFirst;
            }
        }
    }
}
