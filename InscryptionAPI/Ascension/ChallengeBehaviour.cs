using DiskCardGame;
using System.Collections;

namespace InscryptionAPI.Ascension;

/// <summary>
/// Base class for all custom Challenge Handlers.
/// </summary>
public class ChallengeBehaviour : NonCardTriggerReceiver
{
    private static List<ChallengeBehaviour> instances;

    /// <summary>
    /// The unique id for this challenge.
    /// </summary>
    public ChallengeManager.FullChallenge challenge { get; internal set; }

    /// <summary>
    /// The instance number for this challenge once instantiated.
    /// </summary>
    public int instanceNumber { get; internal set; }

    /// <summary>
    /// Fires when the challenge behavior starts up.
    /// </summary>
    public void Start()
    {
        if (!Instances.Contains(this))
        {
            Instances.Add(this);
        }
    }

    /// <summary>
    /// Fires when the challenge behaviour is destroyed
    /// </summary>
    public new void OnDestroy()
    {
        Instances.Remove(this);
    }

    /// <summary>
    /// Gets all active challenge behaviours for the challenge boon type.
    /// </summary>
    /// <param name="type">The challenge type to search for.</param>
    /// <returns>A list of all challenge behaviours that are active in battle.</returns>
    public static List<ChallengeBehaviour> FindInstancesOfType(AscensionChallenge type)
    {
        return Instances.FindAll((x) => x.challenge.Challenge.challengeType == type);
    }

    /// <summary>
    /// Gets the count of active challenge behaviours for the given challenge type.
    /// </summary>
    /// <param name="type">The challenge type to search for.</param>
    /// <returns>The count of all challenge behaviours that are active in battle.</returns>
    public static int CountInstancesOfType(AscensionChallenge type)
    {
        return FindInstancesOfType(type).Count;
    }

    /// <summary>
    /// Indicates if there are any active challenge behaviours of the given type.
    /// </summary>
    /// <param name="type">The challenge type to search for.</param>
    /// <returns>True if there is at least one challenge of the given type; false otherwise.</returns>
    public static bool AnyInstancesOfType(AscensionChallenge type)
    {
        return CountInstancesOfType(type) > 0;
    }

    internal static List<ChallengeBehaviour> Instances
    {
        get
        {
            EnsureInstancesLoaded();
            return instances;
        }
    }

    internal static void DestroyAllInstances()
    {
        List<ChallengeBehaviour> instance = Instances;
        foreach (ChallengeBehaviour ins in instance)
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
            instances = new List<ChallengeBehaviour>();
        }
        instances.RemoveAll((x) => x == null || x.gameObject == null);
    }

    /// <summary>
    /// Plays the command line animation to show the challenge responsible for an action.
    /// </summary>
    protected void ShowActivation()
    {
        ChallengeActivationUI.TryShowActivation(challenge.Challenge.challengeType);
    }

    /// <summary>
    /// Override this to indicate if this challenge needs to take an action at the beginning of battle **before** all setup.
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPreBattleSetup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your challenge take an action at the beginning of battle **before** all setup.
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPreBattleSetup()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this challenge needs to take an action at the beginning of battle **after** all setup.
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPostBattleSetup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your challenge take an action at the beginning of battle **after** all setup
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPostBattleSetup()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this challenge needs to take an action at the end of battle **before** battle cleanup
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPreBattleCleanup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your challenge take an action at the end of battle **before** battle cleanup
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPreBattleCleanup()
    {
        yield break;
    }

    /// <summary>
    /// Override this to indicate if this challenge needs to take an action at the end of battle **after** battle cleanup
    /// </summary>
    /// <returns>True if the boon wants to respond, false otherwise</returns>
    public virtual bool RespondsToPostBattleCleanup()
    {
        return false;
    }

    /// <summary>
    /// Override this to have your challenge take an action at the end of battle **after** battle cleanup
    /// </summary>
    /// <returns>A sequence of Unity events containing those actions</returns>
    public virtual IEnumerator OnPostBattleCleanup()
    {
        yield break;
    }
}
