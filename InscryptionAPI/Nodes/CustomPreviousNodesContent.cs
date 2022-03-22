using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public class CustomPreviousNodesContent : NodeData.SelectionCondition
	{
		public CustomPreviousNodesContent(string customNodeName, bool doesContain)
		{
			this.customNodeName = customNodeName;
			this.doesContain = doesContain;
		}

		public override bool Satisfied(int gridY, List<NodeData> previousNodes)
		{
			bool exists = previousNodes.Exists((NodeData x) => x is CustomSpecialNodeData && (x as CustomSpecialNodeData).Node != null && (x as CustomSpecialNodeData).Node.name == customNodeName);
			return doesContain == exists;
		}

		public string customNodeName;
		public bool doesContain;
    }
}
