using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public class True : NodeData.SelectionCondition
    {
        public override bool Satisfied(int gridY, List<NodeData> previousNodes) => true;
    }

    public class False : NodeData.SelectionCondition
    {
        public override bool Satisfied(int gridY, List<NodeData> previousNodes) => false;
    }

    public class And : NodeData.SelectionCondition
    {
        public And(NodeData.SelectionCondition l, NodeData.SelectionCondition r)
        {
            this.l = l;
            this.r = r;
        }

        public override bool Satisfied(int gridY, List<NodeData> previousNodes) => l.Satisfied(gridY, previousNodes) && r.Satisfied(gridY, previousNodes);

        public NodeData.SelectionCondition l;
        public NodeData.SelectionCondition r;
    }

    public class Not : NodeData.SelectionCondition
    {
        public Not(NodeData.SelectionCondition s)
        {
            this.s = s;
        }

        public override bool Satisfied(int gridY, List<NodeData> previousNodes) => !s.Satisfied(gridY, previousNodes);

        public NodeData.SelectionCondition s;
    }

    public class Delegate : NodeData.SelectionCondition
    {
        public override bool Satisfied(int gridY, List<NodeData> previousNodes) => (gate?.Invoke(gridY, previousNodes)).GetValueOrDefault();

        public SelectionConditionDelegate gate;
    }

    public delegate bool SelectionConditionDelegate(int gridY, List<NodeData> previousNodes);
}
