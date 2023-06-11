using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InscryptionAPI.PixelCard;

public class PixelAppearanceBehaviour : CardAppearanceBehaviour
{
    public override void ApplyAppearance() { }
    public virtual Sprite PixelAppearance() { return null; }
    public virtual Sprite OverrideBackground() { return null; }
    public virtual void OnAppearanceApplied() { }
}

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

    public DiskCardGame.Card card;
    public GameObject OrangeGemLit;
    public GameObject GreenGemLit;
    public GameObject BlueGemLit;
}