using DiskCardGame;
using System.Collections;

namespace InscryptionAPI.Boons;

/// <summary>
/// Base class for all custom Boons
/// </summary>
public class BoonBehaviour : NonCardTriggerReceiver
{
    private static List<BoonBehaviour> instances;

    /// <summary>
    /// The unique id for this boon
    /// </summary>
    public BoonManager.FullBoon boon { get; internal set; }

    /// <summary>
    /// The instance number for this boon once instantiated
    /// </summary>
    public int instanceNumber { get; internal set; }

    /// <summary>
    /// Fires when the boon behavior starts up
    /// </summary>
    public void Start()
    {
        if (!Instances.Contains(this))
        {
            Instances.Add(this);
        }
    }

    /// <summary>
    /// Fires when the boon behaviour is destroyed
    /// </summary>
    public new void OnDestroy()
    {
        Instances.Remove(this);
    }

    /// <summary>
    /// Gets all active boon behaviours for the given boon type
    /// </summary>
    /// <param name="type">The boon type to search for</param>
    /// <returns>A list of all boon behaviours that are active in battle</returns>
    public static List<BoonBehaviour> FindInstancesOfType(BoonData.Type type)
    {
        return Instances.FindAll((x) => x.boon.boon.type == type);
    }

    /// <summary>
    /// Gets the count of active boon behaviours for the given boon type
    /// </summary>
    /// <param name="type">The boon type to search for</param>
    /// <returns>The count of all boon behaviours that are active in battle</returns>
    public static int CountInstancesOfType(BoonData.Type type)
    {
        return FindInstancesOfType(type).Count;
    }

    /// <summary>
    /// Indicates if there are any active boon behaviours of the given type
    /// </summary>
    /// <param name="type">The boon type to search for</param>
    /// <returns>True if there is at least one boon of the given type; false otherwise.</returns>
    public static bool AnyInstancesOfType(BoonData.Type type)
    {
        return Instances.Any(x => x.boon.boon.type == type);
    }

    internal static List<BoonBehaviour> Instances
    {
        get
        {
            EnsureInstancesLoaded();
            return instances;
        }
    }

    internal static void DestroyAllInstances()
    {
        List<BoonBehaviour> instance = Instances;
        foreach (BoonBehaviour ins in instance)
        {
            if (ins != null && ins.gameObject != null)
            {
                Destroy(ins.gameObject);
            }
        }
        EnsureInstancesLoaded();
        Instances.Clear();
    }

    internal static void EnsureInstancesLoaded()
    {
        if (instances == null)
        {
            instances = new List<BoonBehaviour>();
        }
        instances.RemoveAll((x) => x == null || x.gameObject == null || x.GetComponent<DestroyingFlag>() != null);
    }

    /// <summary>
    /// Displays the boon card in battle to indicate to the player that the boon is responsible for the effect that is happening
    /// </summary>
    /// <returns>A sequence of Unity events that plays the boon card animation</returns>
    protected IEnumerator PlayBoonAnimation()
    {
        if (BoonsHandler.Instance != null)
        {
            yield return BoonsHandler.Instance.PlayBoonAnimation(boon.boon.type);
        }
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this boon needs to take an action at the beginning of battle **before** vanilla boons activate
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPreBoonActivation()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your boon take an action at the beginning of battle **before** vanilla boons activate
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPreBoonActivation()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this boon needs to take an action at the beginning of battle **after** vanilla boons activate
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPostBoonActivation()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your boon take an action at the beginning of battle **after** vanilla boons activate
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPostBoonActivation()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this boon needs to take an action at the end of battle **before** vanilla boons activate
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPreBattleCleanup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your boon take an action at the end of battle **before** vanilla boons activate
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPreBattleCleanup()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this boon needs to take an action at the end of battle **after** vanilla boons activate
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPostBattleCleanup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your boon take an action at the end of battle **after** vanilla boons activate
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPostBattleCleanup()
    {
        yield break;
    }
}