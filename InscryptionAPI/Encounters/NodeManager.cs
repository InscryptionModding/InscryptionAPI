using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace InscryptionAPI.Encounters;

[HarmonyPatch]
public static class NodeManager
{
    private static List<NodeInfo> AllNodes = new();

    public class NodeInfo
    {
        public Texture2D[] animatedMapNode { get; set; }
        public Type sequencerType { get; set; }
        public Type nodeDataType { get; set; }
        public string guid { get; internal set; }
    }

    public static NodeInfo Add<S>(Texture2D[] animatedMapNode) where S : ICustomNodeSequence
    {
        return NodeManager.Add<S, CustomNodeData>(animatedMapNode);
    }

    public static NodeInfo Add<S, N>(Texture2D[] animatedMapNode) where S : ICustomNodeSequence where N : CustomNodeData
    {
        if (animatedMapNode.Length != 4)
            throw new InvalidDataException($"There must be exactly four animated map textures");

        NodeInfo info = new NodeInfo() {
            animatedMapNode = animatedMapNode,
            sequencerType = typeof(S),
            nodeDataType = typeof(N),
            guid = typeof(S).Name
        };
        AllNodes.Add(info);
        return info;
    }

    [HarmonyPatch(typeof(SpecialNodeHandler), nameof(SpecialNodeHandler.StartSpecialNodeSequence))]
    [HarmonyPrefix]
    public static bool CustomNodeGenericSelect(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
    {
        // This sends the player to the upgrade shop if the triggering node is SpendExcessTeeth
        if (nodeData is CustomNodeData genericNode)
        {
            NodeInfo info = AllNodes.FirstOrDefault(ni => ni.guid == genericNode.guid);

            if (info == null)
                return true;

            Type customNodeType = info.sequencerType;

            if (customNodeType == null)
                return true;

            ICustomNodeSequence sequence = __instance.gameObject.GetComponent(customNodeType) as ICustomNodeSequence;
            if (sequence == null)
                sequence = __instance.gameObject.AddComponent(customNodeType) as ICustomNodeSequence;

            __instance.StartCoroutine(sequence.ExecuteCustomSequence(genericNode));
            return false; // This prevents the rest of the thing from running.
        }
        return true; // This makes the rest of the thing run
    }

    [HarmonyPatch(typeof(MapDataReader), nameof(MapDataReader.GetPrefabPath))]
    [HarmonyPostfix]
    public static void TrimPrefabPath(ref string __result)
    {
        // So, for some reason, the map data reader doesn't just
        // straight up read the property of the map node.
        // It passes through here first.
        // That's convenient! We will trim our extra instructions off here
        // Then we'll read that information off later
        if (__result.Contains('@'))
            __result = __result.Substring(0, __result.IndexOf('@')); // Get rid of everything after the @
    }

    [HarmonyPatch(typeof(MapDataReader), nameof(MapDataReader.SpawnAndPlaceElement))]
    [HarmonyPostfix]
    public static void TransformMapNode(ref GameObject __result, MapElementData data)
    {
        // First, let's see if we need to do anything
        if (data.PrefabPath.Contains('@'))
        {   
            string guid = data.PrefabPath.Substring(data.PrefabPath.IndexOf('@') + 1);

            NodeInfo info = AllNodes.FirstOrDefault(ni => ni.guid == guid);

            if (info == null)
                return;

            Texture2D[] nodeTextures = info.animatedMapNode;

            if (nodeTextures == null)
                return;

            // Replace the sprite
            AnimatingSprite sprite = __result.GetComponentInChildren<AnimatingSprite>();

            bool loadedTexture = false;
            for (int i = 0; i < sprite.textureFrames.Count; i++)
            {
                if (sprite.textureFrames[i].name != $"InscryptionAPI_{guid}_{i+1}")
                {
                    sprite.textureFrames[i] = nodeTextures[i];
                    loadedTexture = true;
                }
            }

            if (loadedTexture)
                sprite.IterateFrame();
        }
    }
}