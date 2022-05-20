using DiskCardGame;

namespace InscryptionAPI.Card;

public interface IActivateWhenFacedown
{
    public bool ShouldTriggerWhenFaceDown(Trigger trigger, object[] otherArgs);
    public bool ShouldTriggerCustomWhenFaceDown(Type customTrigger);
}