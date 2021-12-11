using System;
using System.Reflection;
using APIPlugin;
using DiskCardGame;
using HarmonyLib;

namespace API.Patches
{
    [HarmonyPatch(typeof(TalkingCardChooser), "Awake")]
    public class TalkingCardPatch
    {
        public static bool Prefix(ref TalkingCardChooser __instance)
        {
            Card c = __instance.gameObject.GetComponentInChildren<Card>();
            Type t;
            if (NewTalkingCard.talkingCards.TryGetValue(c.Info.name.Replace(" ", "_"), out t))
            {
                MethodInfo info = c.GetType().GetMethod("AddPermanentBehaviour").MakeGenericMethod(new Type[] { t });
                info.Invoke(c, new object[] {});
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}