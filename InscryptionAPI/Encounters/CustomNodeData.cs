using DiskCardGame;
using UnityEngine;

namespace InscryptionAPI.Encounters;

/// <summary>
/// Represents a node on the map that has been placed by the Inscryption API
/// </summary>
public class CustomNodeData : SpecialNodeData
{
    /// <summary>
    /// This tells us whether or not the node can actually be generated on the map.
    /// Simply creating the node is not good enough: the node has to confirm that it can be generated
    /// </summary>
    /// <param name="gridY">How deep into the map the node will be. Higher numbers are further away from the start.</param>
    /// <param name="previousNodes">All nodes that have previously been confirmed to be added to the map.</param>
    /// <returns>TRUE if the node is able to be added to the map. FALSE if it is not.</returns>
    public delegate bool NodeGenerationCondition(int gridY, List<NodeData> previousNodes);

    private List<NodeData.SelectionCondition> prerequisiteConditions = new();

    /// <summary>
    /// All prerequisite conditions must return TRUE for the node to be eligible to be added to the map
    /// </summary>
    public override List<SelectionCondition> GenerationPrerequisiteConditions => prerequisiteConditions;

    private List<NodeData.SelectionCondition> forceGenerationConditions = new();

    /// <summary>
    /// If even a single one of these conditions returns TRUE, the node will be added to the map.
    /// </summary>
    public override List<SelectionCondition> ForceGenerationConditions => forceGenerationConditions;

    public CustomNodeData()
    {
        this.connectedNodes = new List<NodeData>();
    }

    /// <summary>
    /// Handles the initialization of the node. This is where conditions should be created.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Adds a simple prerequisite condition that does not depend on the current state of the map.
    /// </summary>
    /// <param name="condition">Returns TRUE if the map node can be generated</param>
    protected void AddGenerationPrerequisite(Func<bool> condition)
    {
        this.GenerationPrerequisiteConditions.Add(new DelegateCondition((y, nodes) => condition()));
    }

    /// <summary>
    /// Adds a prerequisite condition that depends on the current state of the map.
    /// </summary>
    /// <param name="condition">Returns TRUE if the map node can be generated</param>
    protected void AddGenerationPrerequisite(NodeGenerationCondition condition)
    {
        this.GenerationPrerequisiteConditions.Add(new DelegateCondition(condition));
    }

    /// <summary>
    /// Adds a simple forced generation condition that does not depend on the current state of the map.
    /// </summary>
    /// <param name="condition">Returns TRUE if the map node must be generated</param>
    protected void AddForceGenerationCondition(Func<bool> condition)
    {
        this.ForceGenerationConditions.Add(new DelegateCondition((y, nodes) => condition()));
    }

    /// <summary>
    /// Adds a forced generation condition that depends on the current state of the map.
    /// </summary>
    /// <param name="condition">Returns TRUE if the map node must be generated</param>
    protected void AddForceGenerationCondition(NodeGenerationCondition condition)
    {
        this.ForceGenerationConditions.Add(new DelegateCondition(condition));
    }

    [SerializeField]
    internal string guid;

    /// <summary>
    /// This prefab path has been specially formatted to work with the custom patches created by the API.
    /// </summary>
    public sealed override string PrefabPath
    {
        get
        {
            // This syntax works with our custom resource bank loaders
            return $"Prefabs/Map/MapNodesPart1/MapNode_BuyPelts@{guid}";
        }
    }

    private class DelegateCondition : NodeData.SelectionCondition
    {
        private NodeGenerationCondition internalDelegate;

        public DelegateCondition(NodeGenerationCondition condition)
        {
            internalDelegate = condition;
        }

        public override bool Satisfied(int gridY, List<NodeData> previousNodes)
        {
            return internalDelegate(gridY, previousNodes);
        }
    }
}