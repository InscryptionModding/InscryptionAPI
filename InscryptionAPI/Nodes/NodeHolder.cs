using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InscryptionAPI.Nodes
{
    internal class NodeHolder : MonoBehaviour
    {
        public FullNode Node
        {
            get
            {
                return CustomSpecialNodeData.GetNodeFromNameAndIndex(nodeName, nodeIndex);
            }
            set
            {
                if(value != null)
                {
                    nodeName = value.name;
                    nodeIndex = NodeManager.NewNodes.IndexOf(value);
                }
                else
                {
                    nodeName = "";
                    nodeIndex = -1;
                }
            }
        }

        public string nodeName = "";
        public int nodeIndex = -1;
    }
}
