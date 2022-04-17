using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public interface ICustomNodeSequencer
    {
        public IEnumerator DoCustomSequence(CustomSpecialNodeData nodeData);
    }

    public interface IDestroyOnEnd
    {
        public bool ShouldDestroyOnEnd(CustomSpecialNodeData nodeData);
    }

    public interface IDoNotReturnToMapOnEnd
    {
        public bool ShouldNotReturnToMapOnEnd(CustomSpecialNodeData nodeData);
    }

    public interface IInherit
    {
        public void Inherit(CustomSpecialNodeData nodeData);
    }
}
