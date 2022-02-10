using HarmonyLib;

namespace MoreSizesPlugin.Patches
{
    [HarmonyPatch(typeof(CreatureMenuBoardTool), "Begin")]
    internal class CreatureMenuBoardPatch
    {
        internal static float _hitHeightDif;

        public static void Postfix(ref float ____hitHeightDif)
        {
            _hitHeightDif = ____hitHeightDif;
        }
    }
}
