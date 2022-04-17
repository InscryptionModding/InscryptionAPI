using DiskCardGame;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Nodes
{
    public static class NewNodeManager
    {
        public class FullNode
        {
            #region Fields
            public List<NodeData.SelectionCondition> generationPrerequisites = new();
            public List<NodeData.SelectionCondition> forceGenerationConditions = new();
            public Action<CustomSpecialNodeData> onPreNodeGeneration;
            public Action<CustomSpecialNodeData, MapNode2D> onPostNodeGeneration;
            public GameObject sequencerPrefab;
            public GameObject nodePrefab;
            public Type nodeSequencerType;
            public List<Texture2D> nodeAnimation = new();
            public GenerationType generationType;
            public string guid;
            public string name;
            #endregion

            #region Chain Setters
            public FullNode SetName(string newName)
            {
                name = newName;
                return this;
            }

            public FullNode SetGuid(string newGuid)
            {
                guid = newGuid;
                return this;
            }

            public FullNode SetGenerationType(GenerationType newGenerationType)
            {
                generationType = newGenerationType;
                return this;
            }

            public FullNode SetNodeAnimation(List<Texture2D> newNodeAnimation)
            {
                nodeAnimation = new(newNodeAnimation);
                return this;
            }

            public FullNode SetNodeSequencerType(Type newNodeSequencerType)
            {
                nodeSequencerType = newNodeSequencerType;
                return this;
            }

            public FullNode SetNodeSequencerType<T>() where T : ICustomNodeSequencer
            {
                nodeSequencerType = typeof(T);
                return this;
            }

            public FullNode SetNodePrefab(GameObject newNodePrefab)
            {
                nodePrefab = newNodePrefab;
                return this;
            }

            public FullNode SetSequencerPrefab(GameObject newSequencerPrefab)
            {
                sequencerPrefab = newSequencerPrefab;
                return this;
            }

            public FullNode SetOnPostNodeGeneration(Action<CustomSpecialNodeData, MapNode2D> newOnPostNodeGeneration)
            {
                onPostNodeGeneration = newOnPostNodeGeneration;
                return this;
            }

            public FullNode SetOnPreNodeGeneration(Action<CustomSpecialNodeData> newOnPreNodeGeneration)
            {
                onPreNodeGeneration = newOnPreNodeGeneration;
                return this;
            }

            public FullNode SetGenerationPrerequisites(List<NodeData.SelectionCondition> newGenerationPrerequisites)
            {
                generationPrerequisites = newGenerationPrerequisites;
                return this;
            }

            public FullNode SetForceGenerationConditions(List<NodeData.SelectionCondition> newForceGenerationConditions)
            {
                forceGenerationConditions = newForceGenerationConditions;
                return this;
            }
            #endregion

            public Type GetSequencerType()
            {
                return nodeSequencerType ?? nodePrefab?.GetComponent<ICustomNodeSequencer>()?.GetType();
            }

            public bool IsValidSequencerType()
            {
                Type type = GetSequencerType();
                return type != null && !type.IsAbstract && type.IsSubclassOf(typeof(Component)) && type.GetInterfaces().Contains(typeof(ICustomNodeSequencer));
            }
        }

        public readonly static List<FullNode> NewNodes = new();

        public static FullNode New<T>(string guid, string name, GenerationType generationType = GenerationType.None, List<Texture2D> nodeAnimation = null, List<NodeData.SelectionCondition> generationPrerequisites = null, 
            List<NodeData.SelectionCondition> forceGenerationConditions = null, Action<CustomSpecialNodeData> onPreGeneration = null, Action<CustomSpecialNodeData, MapNode2D>
            onPostGeneration = null, GameObject sequencerPrefab = null, GameObject nodePrefab = null) where T : ICustomNodeSequencer
        {
            return New(guid, name, generationType, typeof(T), nodeAnimation, generationPrerequisites, forceGenerationConditions, onPreGeneration, onPostGeneration, sequencerPrefab, nodePrefab);
        }

        public static FullNode New(string guid, string name, GenerationType generationType = GenerationType.None, Type nodeSequencerType = null, List<Texture2D> nodeAnimation = null, 
            List<NodeData.SelectionCondition> generationPrerequisites = null, List<NodeData.SelectionCondition> forceGenerationConditions = null, Action<CustomSpecialNodeData> onPreGeneration = null, 
            Action<CustomSpecialNodeData, MapNode2D> onPostGeneration = null, GameObject sequencerPrefab = null, GameObject nodePrefab = null)
        {
            FullNode fn = new();
            fn.guid = guid;
            fn.name = name;
            fn.nodeSequencerType = nodeSequencerType;
            fn.generationType = generationType;
            if(nodeAnimation != null)
            {
                fn.nodeAnimation = new(nodeAnimation);
            }
            if (generationPrerequisites != null)
            {
                fn.generationPrerequisites = new(generationPrerequisites);
            }
            if(forceGenerationConditions != null)
            {
                fn.forceGenerationConditions = new(forceGenerationConditions);
            }
            fn.onPreNodeGeneration = onPreGeneration;
            fn.onPostNodeGeneration = onPostGeneration;
            fn.sequencerPrefab = sequencerPrefab;
            fn.nodePrefab = nodePrefab;
            return fn;
        }

        public static IEnumerator CustomNodeSequence(ICustomNodeSequencer sequencer, CustomSpecialNodeData node)
        {
            yield return sequencer.DoCustomSequence(node);
            if(sequencer is IDestroyOnEnd destroy && destroy.ShouldDestroyOnEnd(node))
            {
                UnityObject.Destroy((destroy as Component).gameObject);
            }
            if(sequencer is IDoNotReturnToMapOnEnd donotreturn && donotreturn.ShouldNotReturnToMapOnEnd(node))
            {
                yield break;
            }
            Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map, null);
            yield break;
        }

        internal static void DoMissingSequenceSequence(this SpecialNodeHandler hand)
        {
            List<Transform> children = new();
            for (int i = 0; i < hand.transform.childCount; i++)
            {
                Transform child = hand.transform.GetChild(i);
                if (child != null)
                {
                    children.Add(child);
                }
            }
            MissingSequenceSequencer existing = children.Find(x => x.GetComponent<MissingSequenceSequencer>() != null)?.GetComponent<MissingSequenceSequencer>();
            if (existing != null)
            {
                hand.StartCoroutine(CustomNodeSequence(existing, null));
            }
            else
            {
                GameObject sequencerObject = new("MissingSequenceSequencer");
                sequencerObject.transform.parent = hand.transform;
                sequencerObject.transform.localPosition = Vector3.zero;
                existing = sequencerObject.AddComponent<MissingSequenceSequencer>();
                if (existing != null)
                {
                    hand.StartCoroutine(CustomNodeSequence(existing, null));
                }
            }
        }

        internal static void DoMissingNodeSequence(this SpecialNodeHandler hand)
        {
            List<Transform> children = new();
            for (int i = 0; i < hand.transform.childCount; i++)
            {
                Transform child = hand.transform.GetChild(i);
                if (child != null)
                {
                    children.Add(child);
                }
            }
            MissingNodeSequencer existing = children.Find(x => x.GetComponent<MissingNodeSequencer>() != null)?.GetComponent<MissingNodeSequencer>();
            if (existing != null)
            {
                hand.StartCoroutine(CustomNodeSequence(existing, null));
            }
            else
            {
                GameObject sequencerObject = new("MissingNodeSequencer");
                sequencerObject.transform.parent = hand.transform;
                sequencerObject.transform.localPosition = Vector3.zero;
                existing = sequencerObject.AddComponent<MissingNodeSequencer>();
                if (existing != null)
                {
                    hand.StartCoroutine(CustomNodeSequence(existing, null));
                }
            }
        }
    }

    public enum GenerationType
    {
        None = 0,
        SpecialCardChoice = 1,
        SpecialEvent = 2,
        RegionStart = 4,
        PreBoss = 8,
        PostBoss = 16
    }
}
