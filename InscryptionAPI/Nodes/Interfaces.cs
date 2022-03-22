using DiskCardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Nodes
{
    public interface INodeSequencer
    {
        IEnumerator DoCustomSequence();
        void Inherit();
    }

    public interface DestroyOnEnd
    {
    }

    public interface DoNotReturnToMapOnEnd
    {
    }
}
