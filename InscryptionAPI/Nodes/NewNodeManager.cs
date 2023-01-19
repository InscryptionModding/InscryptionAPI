using DiskCardGame;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InscryptionAPI.Nodes;

/// <summary>
/// The new version of NodeManager, used for more advanced node work.
/// </summary>
public static class NewNodeManager
{
    /// <summary>
    /// Represents a new node added by the API.
    /// </summary>
    public class FullNode
    {
        #region Fields
        /// <summary>
        /// Prerequisites for the node generating. If at least one of them is not met, the node won't be generated.
        /// </summary>
        public List<NodeData.SelectionCondition> generationPrerequisites = new();
        /// <summary>
        /// Prerequisites for forcing the node generation. If all generation prerequisites and force generation conditions are met, this node will always generate in place of other nodes of the same generation type.
        /// </summary>
        public List<NodeData.SelectionCondition> forceGenerationConditions = new();
        /// <summary>
        /// Triggers when the node gets created, but before the actual node objects gets added to the map. Can be used for writing runtime information.
        /// </summary>
        public Action<CustomSpecialNodeData> onPreNodeGeneration;
        /// <summary>
        /// Triggers after the node object has been added to the map. Can be used to modify it.
        /// </summary>
        public Action<CustomSpecialNodeData, MapNode2D> onPostNodeGeneration;
        /// <summary>
        /// Prefab for the node sequencer. If null, an empty object with the sequencer type will be generated instead.
        /// </summary>
        public GameObject sequencerPrefab;
        /// <summary>
        /// Prefab for the node object. If null, a default node object will be created and modified instead.
        /// </summary>
        public GameObject nodePrefab;
        /// <summary>
        /// Type for the node sequencer. If null, it will try to get the sequencer type from the sequencer prefab instead.
        /// </summary>
        public Type nodeSequencerType;
        /// <summary>
        /// Animation frames for the node object on the map. If null or empty, the animation frames of the map object won't be affected.
        /// </summary>
        public List<Texture2D> nodeAnimation = new();
        /// <summary>
        /// Flags that affect the way the node generates.
        /// </summary>
        public GenerationType generationType;
        /// <summary>
        /// Guid of the mod this node came from.
        /// </summary>
        public string guid;
        /// <summary>
        /// Name of the node.
        /// </summary>
        public string name;
        #endregion

        #region Chain Setters
        /// <summary>
        /// Sets this node's name.
        /// </summary>
        /// <param name="newName">The new name for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetName(string newName)
        {
            name = newName;
            return this;
        }

        /// <summary>
        /// Sets the mod guid for this node.
        /// </summary>
        /// <param name="newGuid">The new mod guid for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetGuid(string newGuid)
        {
            guid = newGuid;
            return this;
        }

        /// <summary>
        /// Sets the generation type for this node.
        /// </summary>
        /// <param name="newGenerationType">The new generation type for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetGenerationType(GenerationType newGenerationType)
        {
            generationType = newGenerationType;
            return this;
        }

        /// <summary>
        /// Sets the animation frames for this node.
        /// </summary>
        /// <param name="newNodeAnimation">The new animation frames for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetNodeAnimation(List<Texture2D> newNodeAnimation)
        {
            nodeAnimation = new(newNodeAnimation);
            return this;
        }

        /// <summary>
        /// Sets the sequencer type for this node.
        /// </summary>
        /// <param name="newNodeSequencerType">The new sequencer type for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetNodeSequencerType(Type newNodeSequencerType)
        {
            nodeSequencerType = newNodeSequencerType;
            return this;
        }

        /// <summary>
        /// Sets the sequencer type for this node.
        /// </summary>
        /// <typeparam name="T">The new sequencer type for this node.</typeparam>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetNodeSequencerType<T>() where T : ICustomNodeSequencer
        {
            nodeSequencerType = typeof(T);
            return this;
        }

        /// <summary>
        /// Sets the node prefab for this node.
        /// </summary>
        /// <param name="newNodePrefab">The new node prefab for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetNodePrefab(GameObject newNodePrefab)
        {
            nodePrefab = newNodePrefab;
            return this;
        }

        /// <summary>
        /// Sets the sequencer prefab for this node.
        /// </summary>
        /// <param name="newSequencerPrefab">The new sequencer prefab for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetSequencerPrefab(GameObject newSequencerPrefab)
        {
            sequencerPrefab = newSequencerPrefab;
            return this;
        }

        /// <summary>
        /// Sets the "On Post Node Generation" action for this node.
        /// </summary>
        /// <param name="newOnPostNodeGeneration">The new "OnPostNodeGeneration" action for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetOnPostNodeGeneration(Action<CustomSpecialNodeData, MapNode2D> newOnPostNodeGeneration)
        {
            onPostNodeGeneration = newOnPostNodeGeneration;
            return this;
        }

        /// <summary>
        /// Sets the "On Pre Node Generation" action for this node.
        /// </summary>
        /// <param name="newOnPreNodeGeneration">The new "On Pre Node Generation" action for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetOnPreNodeGeneration(Action<CustomSpecialNodeData> newOnPreNodeGeneration)
        {
            onPreNodeGeneration = newOnPreNodeGeneration;
            return this;
        }

        /// <summary>
        /// Sets the generation prerequisites for this node.
        /// </summary>
        /// <param name="newGenerationPrerequisites">New force generation conditions for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetGenerationPrerequisites(List<NodeData.SelectionCondition> newGenerationPrerequisites)
        {
            generationPrerequisites = newGenerationPrerequisites;
            return this;
        }

        /// <summary>
        /// Sets the force generation conditions for this node.
        /// </summary>
        /// <param name="newGenerationPrerequisites">New force generation conditions for this node.</param>
        /// <returns>This node, for method chaining purposes.</returns>
        public FullNode SetForceGenerationConditions(List<NodeData.SelectionCondition> newForceGenerationConditions)
        {
            forceGenerationConditions = newForceGenerationConditions;
            return this;
        }
        #endregion

        /// <summary>
        /// Returns true if type is a valid sequencer type (not null, not abstract, is subclass of Component and has the ICustomNodeSequencer interface).
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if type is a valid sequencer type (not null, not abstract, is subclass of Component and has the ICustomNodeSequencer interface).</returns>
        public static bool SequencerTypeIsValid(Type type)
        {
            return type != null && !type.IsAbstract && type.IsSubclassOf(typeof(Component)) && type.GetInterfaces().Contains(typeof(ICustomNodeSequencer));
        }

        /// <summary>
        /// Gets the sequencer type for this node.
        /// </summary>
        /// <returns>The sequencer type for this node.</returns>
        public Type GetSequencerType()
        {
            return SequencerTypeIsValid(nodeSequencerType) ? nodeSequencerType : nodePrefab?.GetComponent<ICustomNodeSequencer>()?.GetType();
        }

        /// <summary>
        /// Returns true if the sequencer type for this nod is a valid sequencer type (not null, not abstract, is subclass of Component and has the ICustomNodeSequencer interface).
        /// </summary>
        /// <returns>True if the sequencer type for this nod is a valid sequencer type (not null, not abstract, is subclass of Component and has the ICustomNodeSequencer interface).</returns>
        public bool IsValidSequencerType()
        {
            return SequencerTypeIsValid(GetSequencerType());
        }
    }

    internal readonly static List<FullNode> addedNodes = new();

    /// <summary>
    /// A collection of all new nodes added using the API.
    /// </summary>
    public readonly static ReadOnlyCollection<FullNode> NewNodes = new(addedNodes);

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <typeparam name="T">The node sequencer type for the node.</typeparam>
    /// <param name="guid">The guid of the mod the node comes from.</param>
    /// <param name="name">The name of the node.</param>
    /// <param name="generationType">The generation flags for the node.</param>
    /// <param name="nodeAnimation">The animation frames for the node.</param>
    /// <param name="generationPrerequisites">The generation prerequisites for the node.</param>
    /// <param name="forceGenerationConditions">The force generation conditions for the node.</param>
    /// <param name="onPreGeneration">The action that triggers when the node is created, but before the node object is added to the map.</param>
    /// <param name="onPostGeneration">The action that triggers after the node object is added to the map.</param>
    /// <param name="sequencerPrefab">The sequencer prefab for the node.</param>
    /// <param name="nodePrefab">The node object prefab for the node.</param>
    /// <returns>The created node.</returns>
    public static FullNode New<T>(string guid, string name, GenerationType generationType = GenerationType.None, List<Texture2D> nodeAnimation = null, List<NodeData.SelectionCondition> generationPrerequisites = null,
        List<NodeData.SelectionCondition> forceGenerationConditions = null, Action<CustomSpecialNodeData> onPreGeneration = null, Action<CustomSpecialNodeData, MapNode2D>
            onPostGeneration = null, GameObject sequencerPrefab = null, GameObject nodePrefab = null) where T : ICustomNodeSequencer
    {
        return New(guid, name, generationType, typeof(T), nodeAnimation, generationPrerequisites, forceGenerationConditions, onPreGeneration, onPostGeneration, sequencerPrefab, nodePrefab);
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="guid">The guid of the mod the node comes from.</param>
    /// <param name="name">The name of the node.</param>
    /// <param name="generationType">The generation flags for the node.</param>
    /// <param name="nodeSequencerType">The node sequencer type for the node.</param>
    /// <param name="nodeAnimation">The animation frames for the node.</param>
    /// <param name="generationPrerequisites">The generation prerequisites for the node.</param>
    /// <param name="forceGenerationConditions">The force generation conditions for the node.</param>
    /// <param name="onPreGeneration">The action that triggers when the node is created, but before the node object is added to the map.</param>
    /// <param name="onPostGeneration">The action that triggers after the node object is added to the map.</param>
    /// <param name="sequencerPrefab">The sequencer prefab for the node.</param>
    /// <param name="nodePrefab">The node object prefab for the node.</param>
    /// <returns>The created node.</returns>
    public static FullNode New(string guid, string name, GenerationType generationType = GenerationType.None, Type nodeSequencerType = null, List<Texture2D> nodeAnimation = null,
        List<NodeData.SelectionCondition> generationPrerequisites = null, List<NodeData.SelectionCondition> forceGenerationConditions = null, Action<CustomSpecialNodeData> onPreGeneration = null,
        Action<CustomSpecialNodeData, MapNode2D> onPostGeneration = null, GameObject sequencerPrefab = null, GameObject nodePrefab = null)
    {
        FullNode fn = new();
        fn.guid = guid;
        fn.name = name;
        fn.nodeSequencerType = nodeSequencerType;
        fn.generationType = generationType;
        if (nodeAnimation != null)
        {
            fn.nodeAnimation = new(nodeAnimation);
        }
        if (generationPrerequisites != null)
        {
            fn.generationPrerequisites = new(generationPrerequisites);
        }
        if (forceGenerationConditions != null)
        {
            fn.forceGenerationConditions = new(forceGenerationConditions);
        }
        fn.onPreNodeGeneration = onPreGeneration;
        fn.onPostNodeGeneration = onPostGeneration;
        fn.sequencerPrefab = sequencerPrefab;
        fn.nodePrefab = nodePrefab;
        addedNodes.Add(fn);
        return fn;
    }

    internal static IEnumerator CustomNodeSequence(ICustomNodeSequencer sequencer, CustomSpecialNodeData node)
    {
        yield return sequencer.DoCustomSequence(node);
        if (sequencer is IDestroyOnEnd destroy && destroy.ShouldDestroyOnEnd(node))
        {
            UnityObject.Destroy((destroy as Component).gameObject);
        }
        if (sequencer is IDoNotReturnToMapOnEnd donotreturn && donotreturn.ShouldNotReturnToMapOnEnd(node))
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

/// <summary>
/// Flags for different types of generation.
/// </summary>
public enum GenerationType
{
    /// <summary>
    /// Nodes flagged with this will not generate.
    /// </summary>
    None = 0,
    /// <summary>
    /// Nodes flagged with this will be added to the pool of "special card choices" such as the Prospector, the Trader and deck trials.
    /// </summary>
    SpecialCardChoice = 1,
    /// <summary>
    /// Nodes flagged with this will be added to the pool of "special events" such as the campfire, the sacrifice stones and the Mycologists.
    /// </summary>
    SpecialEvent = 2,
    /// <summary>
    /// Nodes flagged with this will always appear at the start of each map if all of the generation prerequisites are satisfied.
    /// </summary>
    RegionStart = 4,
    /// <summary>
    /// Nodes flagged with this will always appear before the boss if all of the generation prerequisites are satisfied.
    /// </summary>
    PreBoss = 8,
    /// <summary>
    /// Nodes flagged with this will always appear after the boss if all of the generation prerequisites are satisfied.
    /// </summary>
    PostBoss = 16
}