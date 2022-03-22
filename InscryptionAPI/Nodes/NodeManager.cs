using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DiskCardGame;
using InscryptionAPI.Helpers;
using HarmonyLib;

namespace InscryptionAPI.Nodes
{
    [HarmonyPatch]
    public static class NodeManager
    {
        public static List<FullNode> NewNodes = new();
        public static readonly List<Texture2D> MAP_EVENT_MISSING = new()
        {
            TextureHelper.GetTextureFromResource("InscryptionAPI/mapevent_missing.png")
        };

        public static void New(string name, MapNodeType type, List<Texture2D> mapAnimationFrames, GameObject nodeSequencerPrefab, List<NodeData.SelectionCondition> generationPrerequisites = null,
           List<NodeData.SelectionCondition> forceGenerationConditions = null)
        {
            New(name, new List<MapNodeType> { type }, mapAnimationFrames, nodeSequencerPrefab, generationPrerequisites, forceGenerationConditions);
        }

        public static void New(string name, List<MapNodeType> type, List<Texture2D> mapAnimationFrames, GameObject nodeSequencerPrefab, List<NodeData.SelectionCondition> generationPrerequisites = null, 
            List<NodeData.SelectionCondition> forceGenerationConditions = null)
        {
            FullNode n = new();
            n.name = name;
            n.type = new(type);
            n.mapAnimationFrames = new List<Texture2D>(mapAnimationFrames);
            n.generationPrerequisiteConditions = new List<NodeData.SelectionCondition>();
            if (generationPrerequisites != null)
            {
                n.generationPrerequisiteConditions = new List<NodeData.SelectionCondition>(generationPrerequisites);
            }
            n.forceGenerationConditions = new List<NodeData.SelectionCondition>();
            if (forceGenerationConditions != null)
            {
                n.forceGenerationConditions = new List<NodeData.SelectionCondition>(forceGenerationConditions);
            }
            n.nodeSequencerPrefab = nodeSequencerPrefab;
            NewNodes.Add(n);
        }

        public static void New(string name, MapNodeType type, List<Texture2D> mapAnimationFrames, Type nodeSequenceHandlerType, List<NodeData.SelectionCondition> generationPrerequisites = null,
            List<NodeData.SelectionCondition> forceGenerationConditions = null)
        {
            New(name, new List<MapNodeType> { type }, mapAnimationFrames, nodeSequenceHandlerType, generationPrerequisites, forceGenerationConditions);
        }

        public static void New(string name, List<MapNodeType> type, List<Texture2D> mapAnimationFrames, Type nodeSequenceHandlerType, List<NodeData.SelectionCondition> generationPrerequisites = null, 
            List<NodeData.SelectionCondition> forceGenerationConditions = null)
        {
            FullNode n = new();
            n.name = name;
            n.type = new(type);
            n.mapAnimationFrames = new List<Texture2D>(mapAnimationFrames);
            n.generationPrerequisiteConditions = new List<NodeData.SelectionCondition>();
            if (generationPrerequisites != null)
            {
                n.generationPrerequisiteConditions = new List<NodeData.SelectionCondition>(generationPrerequisites);
            }
            n.forceGenerationConditions = new List<NodeData.SelectionCondition>();
            if (forceGenerationConditions != null)
            {
                n.forceGenerationConditions = new List<NodeData.SelectionCondition>(forceGenerationConditions);
            }
            n.nodeSequenceHandlerType = nodeSequenceHandlerType;
            NewNodes.Add(n);
        }

        public static void New<T>(string name, MapNodeType type, List<Texture2D> mapAnimationFrames, List<NodeData.SelectionCondition> generationPrerequisites = null,
            List<NodeData.SelectionCondition> forceGenerationConditions = null) where T : INodeSequencer
        {
            New<T>(name, new List<MapNodeType> { type }, mapAnimationFrames, generationPrerequisites, forceGenerationConditions);
        }

        public static void New<T>(string name, List<MapNodeType> type, List<Texture2D> mapAnimationFrames, List<NodeData.SelectionCondition> generationPrerequisites = null,
            List<NodeData.SelectionCondition> forceGenerationConditions = null) where T : INodeSequencer
        {
            New(name, type, mapAnimationFrames, typeof(T), generationPrerequisites, forceGenerationConditions);
        }

        [HarmonyPatch(typeof(MapDataReader), "SpawnAndPlaceElement")]
        [HarmonyPostfix]
        public static void SpawnAndPlaceElement(MapElementData data, GameObject __result)
        {
            if (data is CustomSpecialNodeData && (data as CustomSpecialNodeData).Node != null && NewNodes.Contains((data as CustomSpecialNodeData).Node))
            {
                __result.GetComponentInChildren<AnimatingSprite>().SetTexture((data as CustomSpecialNodeData).Node.mapAnimationFrames[0]);
                __result.GetComponentInChildren<AnimatingSprite>().textureFrames = new List<Texture2D>((data as CustomSpecialNodeData).Node.mapAnimationFrames);
            }
            else if (data is CustomSpecialNodeData && ((data as CustomSpecialNodeData).Node == null || !NewNodes.Contains((data as CustomSpecialNodeData).Node)))
            {
                __result.GetComponentInChildren<AnimatingSprite>().SetTexture(MAP_EVENT_MISSING[0]);
                __result.GetComponentInChildren<AnimatingSprite>().textureFrames = new List<Texture2D>(MAP_EVENT_MISSING);
            }
        }

        [HarmonyPatch(typeof(MapGenerator), "ChooseSpecialNodeFromPossibilities")]
        [HarmonyPrefix]
        public static void ChooseSpecialNodeFromPossibilities(ref List<NodeData> possibilities)
        {
            MapNodeType type = MapNodeType.None;
            if (possibilities.Count > 3 && possibilities[0] is BuyPeltsNodeData && possibilities[1] is TradePeltsNodeData && possibilities[2] is DeckTrialNodeData && possibilities[3] is BoulderChoiceNodeData)
            {
                type = MapNodeType.SpecialEvent;
            }
            else if (possibilities.Count > 6 && possibilities[0] is CardMergeNodeData && possibilities[1] is GainConsumablesNodeData && possibilities[2] is BuildTotemNodeData && possibilities[3] is DuplicateMergeNodeData &&
                possibilities[4] is CardRemoveNodeData && possibilities[5] is CardStatBoostNodeData && possibilities[6] is CopyCardNodeData)
            {
                type = MapNodeType.Other;
            }
            if (type != MapNodeType.None)
            {
                List<FullNode> nodesOftype = NewNodes.FindAll((x) => x.type != null && x.type.Contains(type));
                if (nodesOftype.Count > 0)
                {
                    possibilities.AddRange(nodesOftype.ConvertAll(delegate (FullNode nn)
                    {
                        NodeData node = new CustomSpecialNodeData();
                        if (node is CustomSpecialNodeData)
                        {
                            (node as CustomSpecialNodeData).nodeName = nn.name;
                            (node as CustomSpecialNodeData).nodeIndex = NewNodes.IndexOf(nn);
                        }
                        return node;
                    }));
                }
            }
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool StartSpecialNodeSequence(SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            INodeSequencer CreateOrFind(string name, Type typeToAdd)
            {
                Component preadded = __instance.GetComponentInChildren(typeToAdd);
                if (preadded != null)
                {
                    return (INodeSequencer)preadded;
                }
                return (INodeSequencer)new GameObject(name).AddComponent(typeToAdd);
            }
            if (nodeData is CustomSpecialNodeData && (nodeData as CustomSpecialNodeData).Node != null && (nodeData as CustomSpecialNodeData).Node.nodeSequencerPrefab != null &&
                (nodeData as CustomSpecialNodeData).Node.nodeSequencerPrefab.GetComponent<INodeSequencer>() != null)
            {
                GameObject sequencer = __instance.GetComponentsInChildren<NodeHolder>().ToList().Find(x => x.Node == (nodeData as CustomSpecialNodeData).Node)?.gameObject ??
                    UnityEngine.Object.Instantiate((nodeData as CustomSpecialNodeData).Node.nodeSequencerPrefab);
                NodeHolder holder = sequencer.GetComponent<NodeHolder>();
                if (holder == null)
                {
                    holder = sequencer.AddComponent<NodeHolder>();
                }
                holder.Node = (nodeData as CustomSpecialNodeData).Node;
                INodeSequencer nodeSequence = sequencer.GetComponent<INodeSequencer>();
                nodeSequence?.StartSequence(__instance);
                return false;
            }
            else if (nodeData is CustomSpecialNodeData && (nodeData as CustomSpecialNodeData).Node != null && NodeTools.SequencerTypeValid((nodeData as CustomSpecialNodeData).Node.nodeSequenceHandlerType))
            {
                INodeSequencer nodeSequence = CreateOrFind((nodeData as CustomSpecialNodeData).Node.name + " Custom Sequencer", (nodeData as CustomSpecialNodeData).Node.nodeSequenceHandlerType);
                nodeSequence.StartSequence(__instance);
                return false;
            }
            else if (nodeData is CustomSpecialNodeData && (nodeData as CustomSpecialNodeData).Node != null && ((nodeData as CustomSpecialNodeData).Node.nodeSequenceHandlerType == null ||
                !(nodeData as CustomSpecialNodeData).Node.nodeSequenceHandlerType.IsSubclassOf(typeof(CustomSpecialNodeSequencer))))
            {
                INodeSequencer nodeSequence = CreateOrFind("MISSING_SEQUENCE Custom Sequencer", typeof(MissingSequenceSequencer));
                nodeSequence.StartSequence(__instance);
                return false;
            }
            else if (nodeData is CustomSpecialNodeData && (nodeData as CustomSpecialNodeData).Node == null)
            {
                INodeSequencer nodeSequence = CreateOrFind("MISSING_NODE Custom Sequencer", typeof(MissingEventSequencer));
                nodeSequence.StartSequence(__instance);
                return false;
            }
            return true;
        }

        private static void LinkNodes(List<NodeData> nodes, int startY, List<NodeData> previousNodes)
        {
            int i = 0;
            while (i < nodes.Count)
            {
                if (!nodes[i].MapGenerationPrequisitesMet(startY + i, i == 0 ? previousNodes : previousNodes.Concat(nodes.Take(i)).ToList()))
                {
                    nodes.RemoveAt(i);
                    continue;
                }

                nodes[i].gridY = startY + i;
                nodes[i].gridX = 1;
                nodes[i].id = MapGenerator.GetNewID();

                if (i > 0)
                {
                    if (nodes[i - 1].connectedNodes == null)
                        nodes[i - 1].connectedNodes = new();

                    nodes[i - 1].connectedNodes.Add(nodes[i]);
                }

                i++;
            }
        }

        private static NodeData GetLastNodeBeforeCustomFront(List<NodeData> nodes)
        {
            if (MapGenerator.ForceFirstNodeTraderForAscension(1))
                return nodes.First(n => n.gridY == 1);
            else
                return nodes.First(n => n.gridY == 0);
        }

        [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateNodes))]
        [HarmonyPostfix]
        private static void AddStartEndCustomNodes(ref List<NodeData> __result)
        {
            // We need to create a list of all of the new nodes that go at the front
            int frontStart = MapGenerator.ForceFirstNodeTraderForAscension(1) ? 2 : 1;
            List<NodeData> frontNodes = new();
            foreach (FullNode info in NewNodes.Where(x => x.type.Contains(MapNodeType.RegionStart)))
            {
                NodeData node = new CustomSpecialNodeData();
                if (node is CustomSpecialNodeData)
                {
                    (node as CustomSpecialNodeData).nodeName = info.name;
                    (node as CustomSpecialNodeData).nodeIndex = NewNodes.IndexOf(info);
                }
                frontNodes.Add(node);
            }

            // This filters out nodes that shouldn't be there and links them in order
            LinkNodes(frontNodes, frontStart, new());

            // This takes the nodes and injects them into the beginning of the map (extending the map in the process)
            if (frontNodes.Count > 0)
            {
                foreach (NodeData node in __result.Where(n => n.gridY >= frontStart).ToList())
                    node.gridY += frontNodes.Count;

                NodeData lastFrontNode = GetLastNodeBeforeCustomFront(__result);
                lastFrontNode.connectedNodes.Clear();
                lastFrontNode.connectedNodes.Add(frontNodes[0]);

                foreach (NodeData firstNextNode in __result.Where(n => n.gridY == frontStart + frontNodes.Count))
                    frontNodes[frontNodes.Count - 1].connectedNodes.Add(firstNextNode);

                __result = __result.Where(n => n.gridY < frontStart).Concat(frontNodes).Concat(__result.Where(n => n.gridY >= frontStart)).ToList();
            }

            // These nodes go before the boss
            List<NodeData> preBossNodes = new();
            foreach (FullNode info in NewNodes.Where(x => x.type.Contains(MapNodeType.PreBoss)))
            {
                NodeData node = new CustomSpecialNodeData();
                if (node is CustomSpecialNodeData)
                {
                    (node as CustomSpecialNodeData).nodeName = info.name;
                    (node as CustomSpecialNodeData).nodeIndex = NewNodes.IndexOf(info);
                }
                preBossNodes.Add(node);
            }

            int bossY = __result.Select(n => n.gridY).Max();
            NodeData bossBattleNode = __result.First(n => n.gridY == bossY);
            LinkNodes(preBossNodes, bossY, new(__result.Where(n => n.gridY < bossY)));

            if (preBossNodes.Count > 0)
            {
                List<NodeData> originalPreBossDataNodes = __result.Where(n => n.connectedNodes.Contains(bossBattleNode)).ToList();

                foreach (NodeData node in originalPreBossDataNodes)
                {
                    node.connectedNodes.Clear();
                    node.connectedNodes.Add(preBossNodes[0]);
                }

                bossBattleNode.gridY = preBossNodes[preBossNodes.Count - 1].gridY + 1;
                preBossNodes[preBossNodes.Count - 1].connectedNodes.Add(bossBattleNode);

                __result = __result.Where(n => n != bossBattleNode).Concat(preBossNodes).ToList();
                __result.Add(bossBattleNode);
            }

            // These nodes go after the boss
            List<NodeData> postBossNodes = new();
            foreach (FullNode info in NewNodes.Where(x => x.type.Contains(MapNodeType.PostBoss)))
            {
                NodeData node = new CustomSpecialNodeData();
                if (node is CustomSpecialNodeData)
                {
                    (node as CustomSpecialNodeData).nodeName = info.name;
                    (node as CustomSpecialNodeData).nodeIndex = NewNodes.IndexOf(info);
                }
                postBossNodes.Add(node);
            }

            LinkNodes(postBossNodes, bossBattleNode.gridY + 1, new(__result));

            if (postBossNodes.Count > 0)
            {
                bossBattleNode.connectedNodes = new();
                bossBattleNode.connectedNodes.Add(postBossNodes[0]);
                __result = __result.Concat(postBossNodes).ToList();
            }
        }

        [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
        [HarmonyPostfix]
        private static void FixMapLength(ref MapData __result, int gridWidth, RegionData region)
        {
            if (region.predefinedNodes == null)
            {
                __result.gridLength = __result.nodeData.Select(n => n.gridY).Max() + 1;
                __result.mapLength = (float)__result.gridLength * 0.185f;
                MapGenerator.PositionNodes(__result.nodeData, __result.mapLength, gridWidth, __result.gridLength);
            }
        }
    }

    public enum MapNodeType
    {
        None,
        CardBattle,
        SpecialCardBattle,
        CardChoice,
        SpecialEvent,
        RegionStart,
        PreBoss,
        PostBoss,
        Other
    }

    public class FullNode
    {
        public Type nodeSequenceHandlerType;
        public GameObject nodeSequencerPrefab;
        public List<Texture2D> mapAnimationFrames;
        public List<MapNodeType> type;
        public string name;
        public List<NodeData.SelectionCondition> generationPrerequisiteConditions;
        public List<NodeData.SelectionCondition> forceGenerationConditions;
    }
}
