using DiskCardGame;

namespace InscryptionAPI.Nodes;

/// <summary>
/// Selection condition that is satisfied when the target condition given to it is not satisfied.
/// </summary>
public class Not : NodeData.SelectionCondition
{
    private readonly NodeData.SelectionCondition condition;

    /// <summary>
    /// Creates a "Not" selection condition and sets its target condition to selectionCondition.
    /// </summary>
    /// <param name="selectionCondition">The target condition for the created "Not" selection condition.</param>
    public Not(NodeData.SelectionCondition selectionCondition)
    {
        condition = selectionCondition;
    }

    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return !condition.Satisfied(gridY, previousNodes);
    }
}

/// <summary>
/// Selection condition that is satisfied when both of its target conditions are satisfied.
/// </summary>
public class And : NodeData.SelectionCondition
{
    private readonly NodeData.SelectionCondition condition1;
    private readonly NodeData.SelectionCondition condition2;

    /// <summary>
    /// Creates an "And" selection condition and sets its target conditions to selectionCondition1 and selectionCondition2.
    /// </summary>
    /// <param name="selectionCondition1">The target condition 1 for the created "And" selection condition.</param>
    /// <param name="selectionCondition2">The target condition 2 for the created "And" selection condition.</param>
    public And(NodeData.SelectionCondition selectionCondition1, NodeData.SelectionCondition selectionCondition2)
    {
        condition1 = selectionCondition1;
        condition2 = selectionCondition2;
    }

    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return condition1.Satisfied(gridY, previousNodes) && condition2.Satisfied(gridY, previousNodes);
    }
}

/// <summary>
/// Selection condition that is always satisfied.
/// </summary>
public class True : NodeData.SelectionCondition
{
    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return true;
    }
}

/// <summary>
/// Selection condition that is never satisfied.
/// </summary>
public class False : NodeData.SelectionCondition
{
    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return false;
    }
}

/// <summary>
/// Selection condition that is satisfied when the previous nodes either contain or don't contain a custom node with a specific name and guid.
/// </summary>
public class CustomPreviousNodesContent : NodeData.SelectionCondition
{
    private readonly string name;
    private readonly string guid;
    private bool doesContain;

    /// <summary>
    /// Creates an "CustomPreviousNodesContent" selection condition and sets its target guid, name and required value.
    /// </summary>
    /// <param name="guid">The mod guid of the node this selection condition will be looking for.</param>
    /// <param name="name">The name of the node this selection condition will be looking for.</param>
    /// <param name="doesContain">If true, this condition will be satisfied if the previous nodes do contain the node that this condition is looking for. If false, it will be satisfied if the previous nodes don't contain it.</param>
    public CustomPreviousNodesContent(string guid, string name, bool doesContain)
    {
        this.name = name;
        this.guid = guid;
        this.doesContain = doesContain;
    }

    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return previousNodes.Exists((x) => x != null && x is CustomSpecialNodeData && (x as CustomSpecialNodeData).name == name && (x as CustomSpecialNodeData).guid == guid) == doesContain;
    }
}

/// <summary>
/// Selection condition that is satisfied when the previous row of nodes either contains or doesn't contain a custom node with a specific name and guid.
/// </summary>
public class CustomPreviousRowContent : NodeData.SelectionCondition
{
    private readonly string name;
    private readonly string guid;
    private bool doesContain;

    /// <summary>
    /// Creates an "CustomPreviousRowContent" selection condition and sets its target guid, name and required value.
    /// </summary>
    /// <param name="guid">The mod guid of the node this selection condition will be looking for.</param>
    /// <param name="name">The name of the node this selection condition will be looking for.</param>
    /// <param name="doesContain">If true, this condition will be satisfied if the previous row of nodes does contain the node that this condition is looking for. If false, it will be satisfied if the previous row of nodes 
    /// doesn't contain it.</param>
    public CustomPreviousRowContent(string guid, string name, bool doesContain)
    {
        this.name = name;
        this.guid = guid;
        this.doesContain = doesContain;
    }

    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        return previousNodes.Exists((x) => x != null && x.gridY == gridY - 1 && x is CustomSpecialNodeData && (x as CustomSpecialNodeData).name == name && (x as CustomSpecialNodeData).guid == guid) == doesContain;
    }
}

/// <summary>
/// Selection condition that is satisfied when the delegate given to it returns true.
/// </summary>
public class Func : NodeData.SelectionCondition
{
    private readonly Func<int, List<NodeData>, bool> func;

    /// <summary>
    /// Creates a "Func" selection condition and sets its target delegate to del
    /// </summary>
    /// <param name="del">The target delegate for the condition.</param>
    public Func(Func<int, List<NodeData>, bool> del)
    {
        func = del;
    }

    public override bool Satisfied(int gridY, List<NodeData> previousNodes)
    {
        if (func == null)
        {
            return true;
        }
        return func(gridY, previousNodes);
    }
}