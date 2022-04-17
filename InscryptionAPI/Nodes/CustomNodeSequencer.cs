using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public abstract class CustomNodeSequencer : ManagedBehaviour, ICustomNodeSequencer, IInherit, IDestroyOnEnd, IDoNotReturnToMapOnEnd
    {
        public abstract IEnumerator DoCustomSequence(CustomSpecialNodeData node);
        public virtual void Inherit(CustomSpecialNodeData node) { }
        public virtual bool ShouldDestroyOnEnd(CustomSpecialNodeData node) { return false; }
        public virtual bool ShouldNotReturnToMapOnEnd(CustomSpecialNodeData node) { return false; }
    }
}
