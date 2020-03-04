using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace BiomesPreview
{
    [StaticConstructorOnStartup]
    public static class BiomesPreviewPatches
    {
        //public const string Id = "rimworld.biomes.core";
        //public const string Name = "Biomes! Core";
        //public static string Version = (Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute).InformationalVersion;

        static BiomesPreviewPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.biomespreview");
            //HarmonyInstance.Create(Id).PatchAll();
            harmony.PatchAll();
            Log.Message("Biomes! Preview initialized");
            //Log("Initialized");
        }


        //private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";
    }


    [HarmonyPatch(typeof(BeachMaker), nameof(BeachMaker.Init))]
    internal static class BeachMaker_NoBeach
    {
        internal static bool Prefix(Map map)
        {
            if (map.Biome.defName == "BiomesPreview_Atoll")
            {
                return false;
            }
            return true;
        }
    }


    //[HarmonyPatch(typeof(World), nameof(World.CoastDirectionAt))]
    //internal static class World_NoBeachBiomes
    //{
    //    // from RF-Archipelagos
    //    internal static bool Prefix(int tileID, ref Rot4 __result, ref World __instance)
    //    {
    //        var world = Traverse.Create(__instance);
    //        WorldGrid worldGrid = world.Field("grid").GetValue<WorldGrid>();
    //        if (worldGrid[tileID].biome.defName == "BiomesPreview_Atoll")
    //        {
    //            __result = Rot4.Invalid;
    //            return false;
    //        }
    //        return true;
    //    }
    //}


    [HarmonyPatch(typeof(World), nameof(World.NaturalRockTypesIn))]
    internal static class World_AddNaturalRockTypes
    {
        internal static void Postfix(int tile, ref IEnumerable<ThingDef> __result, ref World __instance)
        {
            var world = Traverse.Create(__instance);
            WorldGrid worldGrid = world.Field("grid").GetValue<WorldGrid>();
            if (worldGrid[tile].biome.defName == "BiomesPreview_Atoll")
            {
                List<ThingDef> rocks = new List<ThingDef>() { BiomesPreviewDefOf.BiomesIslands_CoralRock };
                __result = rocks;
            }
            else if (__result.Contains(BiomesPreviewDefOf.BiomesIslands_CoralRock))
            {
                Rand.PushState();
                Rand.Seed = tile;

                List<ThingDef> rocks = __result.ToList();
                rocks.Remove(BiomesPreviewDefOf.BiomesIslands_CoralRock);

                List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
                                       where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock && !d.IsSmoothed && !rocks.Contains(d) && d.defName != "BiomesIslands_CoralRock"
                                       select d).ToList<ThingDef>();
                if (!list.NullOrEmpty())
                {
                    rocks.Add(list.RandomElement<ThingDef>());
                }

                __result = rocks;

                Rand.PopState();
            }
        }

    }


}
