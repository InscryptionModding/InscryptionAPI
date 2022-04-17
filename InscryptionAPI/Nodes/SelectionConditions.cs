using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public class Not : NodeData.SelectionCondition
    {
        public NodeData.SelectionCondition condition;

        public Not(NodeData.SelectionCondition selectionCondition)
        {
            condition = selectionCondition;
        }

        public override bool Satisfied(int gridY, List<NodeData> previousNodes)
        {
            return !condition.Satisfied(gridY, previousNodes);
        }
    }

    public class And : NodeData.SelectionCondition
    {
        public NodeData.SelectionCondition condition1;
        public NodeData.SelectionCondition condition2;

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

    public class True : NodeData.SelectionCondition
    {
        public override bool Satisfied(int gridY, List<NodeData> previousNodes)
        {
            return true;
        }
    }

    public class False : NodeData.SelectionCondition
    {
        public override bool Satisfied(int gridY, List<NodeData> previousNodes)
        {
            return false;
        }
    }

    public class CustomPreviousNodesContent : NodeData.SelectionCondition
    {
        public string name;
        public string guid;
        public bool doesContain;

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

    public class CustomPreviousRowContent : NodeData.SelectionCondition
    {
        public string name;
        public string guid;
        public bool doesContain;

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
}
