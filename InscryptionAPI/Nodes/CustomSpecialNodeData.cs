using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public class CustomSpecialNodeData : SpecialNodeData
    {
        public sealed override string PrefabPath => "Prefabs/Map/MapNodesPart1/MapNode_BuyPelts";
        public override List<SelectionCondition> GenerationPrerequisiteConditions => Node != null && Node.generationPrerequisiteConditions != null ? Node.generationPrerequisiteConditions : new List<SelectionCondition>();
        public override List<SelectionCondition> ForceGenerationConditions => Node != null && Node.forceGenerationConditions != null ? Node.forceGenerationConditions : new List<SelectionCondition>();

        public static FullNode GetNodeFromNameAndIndex(string nodeName, int nodeIndex)
        {
            if (NodeManager.NewNodes.Exists((x) => x.name == nodeName && NodeManager.NewNodes.IndexOf(x) == nodeIndex))
            {
                return NodeManager.NewNodes.Find((x) => x.name == nodeName && NodeManager.NewNodes.IndexOf(x) == nodeIndex);
            }
            else if (NodeManager.NewNodes.Exists((x) => x.name == nodeName))
            {
                return NodeManager.NewNodes.Find((x) => x.name == nodeName);
            }
            else if (NodeManager.NewNodes.Count > nodeIndex)
            {
                return NodeManager.NewNodes[nodeIndex];
            }
            return null;
        }

        public FullNode Node
        {
            get
            {
                return GetNodeFromNameAndIndex(nodeName, nodeIndex);
            }
        }

        public string nodeName;
        public int nodeIndex;
    }
}
