using DiskCardGame;
using InscryptionAPI.Triggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Card
{
    public interface IActivateWhenFacedown
    {
        public bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs);
        public bool ShouldCustomTriggerFaceDown<T>();
    }
}
