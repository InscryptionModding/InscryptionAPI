using DiskCardGame;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch(typeof(AbilityIconInteractable))]
public class BoxColliderNegativeScalingLogSpamFix
{
    /// <summary>
    /// This patch fixes the incredibly annoying warning log spam of this message anytime an ability is flipped for the opponent's side.
    /// ```console
    /// [Warning: Unity Log] BoxColliders does not support negative scale or size.
    /// The effective box size has been forced positive and is likely to give unexpected collision geometry.
    /// If you absolutely need to use negative scaling you can use the convex MeshCollider.
    /// ``` 
    /// </summary>
    /// <param name="__instance">The ability icon</param>
    /// <param name="flippedY">
    /// If the icon needs to be flipped.
    /// Will be true if the `AbilityInfo.flipYIfOpponent` field is true and if the card this icon is on is the opponent card.
    /// </param>
    [HarmonyPrefix, HarmonyPatch(nameof(AbilityIconInteractable.SetFlippedY))]
    public static void ReplaceBoxColliderWithMeshColliderIfIconIsFlipped(AbilityIconInteractable __instance, bool flippedY)
    {
        if (flippedY || SaveManager.SaveFile.IsPart3)
        {
            if (__instance.gameObject.GetComponent<MeshCollider>().SafeIsUnityNull())
            {
                MeshCollider collider = __instance.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                //collider.sharedMesh = null;
                collider.sharedMesh = __instance.GetComponent<MeshFilter>().mesh;

                UnityObject.Destroy(__instance.GetComponent<BoxCollider>());
                //__instance.coll = null;
                __instance.coll = collider;
                // This was the missing piece.
                // The collider box when the MeshCollider is added ends up being right under the card, therefore unable to click.
                // Adjusting the y position here to be higher up allows the icon to be right-clickable again.
                __instance.transform.position += new Vector3(0, 0.1f, 0);
            }
        }
    }
}