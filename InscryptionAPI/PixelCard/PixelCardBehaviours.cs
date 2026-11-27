using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.PixelCard;

/// <summary>
/// Subclass of <see cref="CardAppearanceBehaviour"/> that works in Act 2.
/// </summary>
public class PixelAppearanceBehaviour : CardAppearanceBehaviour
{
    /// <summary>
    /// How this appearance should be applied outside Act 2.
    /// </summary>
    public override void ApplyAppearance() { }
    /// <summary>
    /// The sprite this appearance behaviour will apply in Act 2.
    /// </summary>
    /// <returns></returns>
    public virtual Sprite PixelAppearance() { return null; }
    /// <summary>
    /// The sprite that will override the default portrait in Act 2.
    /// </summary>
    /// <returns></returns>
    public virtual Sprite OverridePixelPortrait() { return null; }
    /// <summary>
    /// The sprite that will override the default card background in Act 2.
    /// </summary>
    /// <returns></returns>
    public virtual Sprite OverrideBackground() { return null; }
    /// <summary>
    /// The sprite that will override the default card background in Act 2 when this card is in the queue.
    /// </summary>
    /// <returns></returns>
    public virtual Sprite OverrideQueuedBackground() { return null; }
    /// <summary>
    /// Logic for this apperance behaviour when applying its appearance in Act 2.
    /// </summary>
    public virtual void OnAppearanceApplied() { }
}

/// <summary>
/// Class that adds a border to Act 2 cards when they're Gemified that updates as gems are added and removed.
/// </summary>
public class PixelGemificationBorder : MonoBehaviour
{
    private void Start()
    {
        card = GetComponentInParent<DiskCardGame.Card>();
        UpdateGemGlow();
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "GBC_CardBattle")
            UpdateGemGlow();
    }
    private void UpdateGemGlow()
    {
        if (Singleton<ResourcesManager>.Instance)
        {
            if (SceneManager.GetActiveScene().name == "GBC_CardBattle" && (card as PlayableCard).OpponentCard)
            {
                if (Singleton<OpponentGemsManager>.Instance)
                {
                    OrangeGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Orange);
                    BlueGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Blue);
                    GreenGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<OpponentGemsManager>.Instance.HasGem(GemType.Green);
                }
            }
            else
            {
                OrangeGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<ResourcesManager>.Instance.HasGem(GemType.Orange);
                BlueGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<ResourcesManager>.Instance.HasGem(GemType.Blue);
                GreenGemLit.GetComponent<SpriteRenderer>().enabled = Singleton<ResourcesManager>.Instance.HasGem(GemType.Green);
            }
        }
        else
        {
            OrangeGemLit.GetComponent<SpriteRenderer>().enabled = false;
            BlueGemLit.GetComponent<SpriteRenderer>().enabled = false;
            GreenGemLit.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    /// <summary>
    /// The <see cref="DiskCardGame.Card"/> this border is attached to.
    /// </summary>
    public DiskCardGame.Card card;
    /// <summary>
    /// The <see cref="GameObject"/> for the lit orange gem.
    /// </summary>
    public GameObject OrangeGemLit;
    /// <summary>
    /// The <see cref="GameObject"/> for the lit green gem.
    /// </summary>
    public GameObject GreenGemLit;
    /// <summary>
    /// The <see cref="GameObject"/> for the lit blue gem.
    /// </summary>
    public GameObject BlueGemLit;
}