using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

[HarmonyPatch]
public static class Part3CardCostRender
{
    // This patches the way card costs are rendered in Act 1 (Leshy's cabin)
    // It allows mixed card costs to display correctly (i.e., 2 blood, 1 bone)
    // And allows gem cost and energy cost to render on the card at all.

    /// <summary>
    /// Contains rendering information for a specific custom cost
    /// </summary>
    public class CustomCostRenderInfo
    {
        /// <summary>
        /// Unique ID for this cost
        /// </summary>
        public string CostId { get; private set; }

        /// <summary>
        /// The display texture for the custom cost
        /// </summary>
        public Texture2D MainCostTexture { get; set; }

        /// <summary>
        /// The emission texture for the custom cost
        /// </summary>
        public Texture2D EmissionTexture { get; set; }

        /// <summary>
        /// The game object that holds the custom cost
        /// </summary>
        public GameObject CostContainer { get; internal set; }

        internal void UpdateDisplayedTextures()
        {
            if (CostContainer != null)
                CostContainer.DisplayCostOnContainer(MainCostTexture, EmissionTexture);
        }

        /// <summary>
        /// Creates a blank custom cost render information object
        /// </summary>
        /// <param name="costId">A unique ID for this cost. Use this to identify the render info at a later step</param>
        /// <remarks>You should only use this constructor if you plan to build a custom gameobject for your cost.</remarks>
        public CustomCostRenderInfo(string costId)
        {
            CostId = costId;
        }

        /// <summary>
        /// Creates a custom cost render information object with a texture displayer
        /// </summary>
        /// <param name="costId">A unique ID for this cost. Use this to identify the render info at a later step</param>
        /// <param name="mainCostTexture">The standard (albedo) texture for your custom cost</param>
        /// <param name="emissionTexture">The emissive texture for your custom cost</param>
        /// <remarks>Use this constructor if you have built your own set of textures to display custom costs</remarks>
        public CustomCostRenderInfo(string costId, Texture2D mainCostTexture, Texture2D emissionTexture) : this(costId)
        {
            MainCostTexture = mainCostTexture;
            EmissionTexture = emissionTexture;
        }

        /// <summary>
        /// Creates a custom cost render information object with a texture displayer
        /// </summary>
        /// <param name="costId">A unique ID for this cost. Use this to identify the render info at a later step</param>
        /// <param name="textures">A pair of standard and emissive textures. This is the return type of the GetIconifiedCostTexture helper method.</param>
        /// <remarks>Use this constructor if you are using the GetIconifiedCostTexture helper to generate a cost texture for you</remarks>
        public CustomCostRenderInfo(string costId, Tuple<Texture2D, Texture2D> textures) : this(costId, textures.Item1, textures.Item2)
        {
        }
    }

    /// <summary>
    /// Hook into this event to be able to add a new custom cost to a card. In your delegate, create a new CustomCostRenderInfo object with the appropriate textures (or no textures if you will use the UpdateCardCostComplex event as well).
    /// </summary>
    public static event Action<CardInfo, List<CustomCostRenderInfo>> UpdateCardCostSimple;

    /// <summary>
    /// Hook into this event to directly modify the game object that is created to hold your custom cost. Do this ONLY if you want to do something more complex than displaying a label/texture.
    /// </summary>
    public static event Action<CardInfo, List<CustomCostRenderInfo>> UpdateCardCostComplex;

    private const float CONTAINER_Z_SPACING = 0.145f;
    private const float INITIAL_CONTAINER_Z_SPACING = 0.14f;
    private const int ICON_SPACING = 18;

    public const string ENERGY_LIGHTS = "Anim/CardBase/Top/EnergyCostLights";
    public const string UPPER_GEM_CONTAINER = "Anim/CardBase/Top/Gems";
    public const string BLUE_GEM_ORIGINAL = "Anim/CardBase/Top/Gems/Gem_Blue";
    public const string ORANGE_GEM_ORIGINAL = "Anim/CardBase/Bottom/Gems/Gem_Orange";
    public const string GREEN_GEM_ORIGINAL = "Anim/CardBase/Bottom/Gems/Gem_Green";
    public const string BLUE_GEM_COST = "Gem_Cost_Blue";
    public const string ORANGE_GEM_COST = "Gem_Cost_Orange";
    public const string GREEN_GEM_COST = "Gem_Cost_Green";
    public const string INITIAL_ALTERNATE_COST = "Anim/CardBase/Top/Gems/TextureDisplayer";

    private static Color BoneColor => Color.white;
    private static Color BloodColor => GameColors.Instance.glowRed;
    
    /// <summary>
    /// Gets a specific child GameObject instance of a particular card
    /// </summary>
    /// <param name="key">The Unity path to the object you want.</param>
    /// <remarks>The Part3CardCostRender class contains a number of constants that point to important parts of the card.</remarks>
    public static GameObject GetPiece(this DiskCardGame.Card card, string key)
    {
        Transform t = card.gameObject.transform.Find(key);
        return t?.gameObject;
    }

    private static ConditionalWeakTable<RenderStatsLayer, DiskCardGame.Card> CardRenderReverseLookup = new ();

    /// <summary>
    /// Gets a reverse reference from an instance of RenderStatsLayer back to the Card it belongs to.
    /// </summary>
    public static DiskCardGame.Card GetCard(this RenderStatsLayer layer)
    {
        if (CardRenderReverseLookup.TryGetValue(layer, out DiskCardGame.Card retval))
            return retval;

        retval = layer.gameObject.GetComponentInParent<DiskCardGame.Card>();
        CardRenderReverseLookup.Add(layer, retval);
        return retval;
    }

    private static Texture2D _bgTexture = null;
    private static Texture2D BackgroundTexture
    {
        get
        {
            if (_bgTexture == null)
                _bgTexture = TextureHelper.GetImageAsTexture($"CostTextureBackground.png", typeof(Part3CardCostRender).Assembly);

            return _bgTexture;
        }
    }

    private static List<Texture2D> _sevenSegments = null;
    private static List<Texture2D> SevenSegmentDisplay
    {
        get
        {
            if (_sevenSegments == null)
            {
                _sevenSegments = new();
                for (int i = 0; i <= 9; i++)
                {
                    Texture2D tex = TextureHelper.GetImageAsTexture($"Display_{i}_small.png", typeof(Part3CardCostRender).Assembly);
                    tex.name = $"Display_{i}";
                    _sevenSegments.Add(tex);
                }

                Texture2D texx = TextureHelper.GetImageAsTexture($"Display_x_small.png", typeof(Part3CardCostRender).Assembly);
                texx.name = $"Display_x";
                _sevenSegments.Add(texx);
            }
            return _sevenSegments;
        }
    }

    private static Texture2D _costCubeBackground = null;
    private static Texture2D CostCubeBackground
    {
        get
        {
            if (_costCubeBackground == null)
            {
                _costCubeBackground = TextureHelper.GetImageAsTexture("Act3CostCubeBackground.png", typeof(Part3CardCostRender).Assembly);
                _costCubeBackground.name = "CostCubeBackground";
            }
            return _costCubeBackground;                
        }
    }

    private static Texture2D _boneIcon = null;
    internal static Texture2D BoneCostIcon
    {
        get
        {
            if (_boneIcon == null)
            {
                _boneIcon = TextureHelper.GetImageAsTexture("BoneCostIcon_small.png", typeof(Part3CardCostRender).Assembly);
                _boneIcon.name = "BoneCostIcon";
            }
            return _boneIcon;                
        }
    }

    private static Texture2D _bloodIcon = null;
    internal static Texture2D BloodCostIcon
    {
        get
        {
            if (_bloodIcon == null)
            {
                _bloodIcon = TextureHelper.GetImageAsTexture("BloodCostIcon_small.png", typeof(Part3CardCostRender).Assembly);
                _bloodIcon.name = "BloodCostIcon";
            }
            return _bloodIcon;                
        }
    }
    private static readonly Dictionary<string, Texture2D> AssembledTextures = new();

    private static GameObject CreateTextureDisplayer(GameObject container)
    {
        // Now a separate object for the texture displayer
        GameObject textureContainer = GameObject.CreatePrimitive(PrimitiveType.Quad);
        textureContainer.name = "TextureDisplayer";
        textureContainer.transform.SetParent(container.transform);

        GameObject.Destroy(textureContainer.GetComponent<MeshCollider>());

        Renderer textureRenderer = textureContainer.GetComponent<Renderer>();
        textureRenderer.material.EnableKeyword("_EMISSION");
        textureRenderer.material.SetTexture("_MainTex", BackgroundTexture);
        textureRenderer.material.SetTexture("_EmissionMap", BackgroundTexture);
        textureRenderer.material.SetColor("_EmissionColor", Color.white);

        textureContainer.transform.localPosition = new(0f, .501f, 0f);
        textureContainer.transform.localScale = new (0.95f, 0.95f, 0.95f);
        textureContainer.transform.localEulerAngles = new (90f, 180f, 0f);

        return textureContainer;
    }

    private static GameObject GenerateSingleAdditionalCostContainer(DiskCardGame.Card card, int index)
    {
        if (card.StatsLayer is not DiskRenderStatsLayer)
            throw new InvalidOperationException("You cannot generate an additional Part 3 cost container on anything other than a Disk Card!!");
        
        // Start with a cube
        GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
        container.name = $"Card_Cost_{index}";
        container.transform.SetParent(card.GetPiece(UPPER_GEM_CONTAINER).transform);
        container.transform.localPosition = new(.24f, .01f, 0f);
        container.transform.localScale = new (0.475f, 0.1f, 0.145f);
        container.transform.localEulerAngles = new (0f, 0f, 0f);

        GameObject.Destroy(container.GetComponent<BoxCollider>());

        // This bit allows the cube primitive to be textured differently on each side
        // https://discussions.unity.com/t/change-texture-of-cube-sides/82546/3
        Mesh mesh = container.GetComponent<MeshFilter>().mesh;
        Vector2[] UVs = new Vector2[mesh.vertices.Length];

        // Front
        UVs[0] = new Vector2(0.0f, 0.0f);
        UVs[1] = new Vector2(0.333f, 0.0f);
        UVs[2] = new Vector2(0.0f, 0.333f);
        UVs[3] = new Vector2(0.333f, 0.333f);

        // Top
        UVs[4] = new Vector2(0.334f, 0.333f);
        UVs[5] = new Vector2(0.666f, 0.333f);
        UVs[8] = new Vector2(0.334f, 0.0f);
        UVs[9] = new Vector2(0.666f, 0.0f);

        // Back
        UVs[6] = new Vector2(1.0f, 0.0f);
        UVs[7] = new Vector2(0.667f, 0.0f);
        UVs[10] = new Vector2(1.0f, 0.333f);
        UVs[11] = new Vector2(0.667f, 0.333f);

        // Bottom
        UVs[12] = new Vector2(0.0f, 0.334f);
        UVs[13] = new Vector2(0.0f, 0.666f);
        UVs[14] = new Vector2(0.333f, 0.666f);
        UVs[15] = new Vector2(0.333f, 0.334f);

        // Left
        UVs[16] = new Vector2(0.334f, 0.334f);
        UVs[17] = new Vector2(0.334f, 0.666f);
        UVs[18] = new Vector2(0.666f, 0.666f);
        UVs[19] = new Vector2(0.666f, 0.334f);

        // Right        
        UVs[20] = new Vector2(0.667f, 0.334f);
        UVs[21] = new Vector2(0.667f, 0.666f);
        UVs[22] = new Vector2(1.0f, 0.666f);
        UVs[23] = new Vector2(1.0f, 0.334f);

        mesh.uv = UVs;

        // Now we go ahead and set the texture
        Renderer renderer = container.GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", CostCubeBackground);

        // And after doing all that work, make it invisible if it's the first one ;)
        if (index == 0)
            renderer.enabled = false;

        // Now a separate object for the texture displayer
        CreateTextureDisplayer(container);

        return container;
    }

    private static readonly ConditionalWeakTable<RenderStatsLayer, List<GameObject>> ExtraCostContainers = new();

    /// <summary>
    /// Gets the cached set of additional cost containers that have been built to accomodate costs beyond 1 type
    /// </summary>
    public static List<GameObject> GetCostContainers(this RenderStatsLayer layer) => ExtraCostContainers.GetOrCreateValue(layer);

    /// <summary>
    /// Gets the set of additional cost containers to hold costs beyond 1. If additional containers need to be built, this will buidl them.
    /// </summary>
    public static List<GameObject> GetCostContainers(this DiskCardGame.Card card, int numberOfContainers)
    {
        List<GameObject> retval = card.StatsLayer.GetCostContainers();
        if (card.StatsLayer is not DiskRenderStatsLayer || retval.Count >= numberOfContainers)
            return retval;
        
        // Cool - we need to make more cost containers
        while (retval.Count < numberOfContainers)
            retval.Add(GenerateSingleAdditionalCostContainer(card, retval.Count));

        return retval;
    }

    private static GameObject MakeCostContainer(DiskCardGame.Card card, string path, Color lightColor)
    {
        GameObject container = GameObject.CreatePrimitive(PrimitiveType.Quad);
        container.name = path.Split('/').Last();
        container.transform.SetParent(card.GetPiece(UPPER_GEM_CONTAINER).transform);

        GameObject.Destroy(container.GetComponent<MeshCollider>());

        Renderer renderer = container.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetTexture("_MainTex", BackgroundTexture);
        renderer.material.SetTexture("_EmissionMap", BackgroundTexture);

        container.transform.localPosition = new(.25f, .0565f, 0f);
        container.transform.localScale = new (0.43f, 0.12f, 0.08f);
        container.transform.localEulerAngles = new (90f, 245f, 65f);

        return container;
    }

    private static readonly Dictionary<string, Dictionary<int, Texture2D>> TextureCache = new();
    private static readonly Dictionary<string, Dictionary<int, Texture2D>> EmissionTextureCache = new();

    /// <summary>
    /// Generates a texture that displays the cost of a specific resource, either by duplicating the icon or using a seven-segment display if there isn't enough space
    /// </summary>
    /// <param name="iconTexture">The icon for your resource</param>
    /// <param name="cost">The amount of that resource to display</param>
    /// <param name="forceDigitDisplay">Force the texture to render using the 7-segment display</param>
    /// <returns>A tuple of two textures. The first has the default background, the second does not. The first is best used as a default texture and the second as an emission texture.</returns>
    /// <remarks>If there is enough space in the texture to render multiple instances of the icon, this will do that.
    /// Otherwise, it will render a texture that shows [ICON] x Cost.
    /// In order to take advantage of texture caching, your icon texture must have a unique name.</remarks>
    public static Tuple<Texture2D, Texture2D> GetIconifiedCostTexture(Texture2D iconTexture, int cost, bool forceDigitDisplay = false)
    {
        if (!string.IsNullOrEmpty(iconTexture.name) && !iconTexture.name.Equals("untitled", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!TextureCache.Keys.Contains(iconTexture.name))
            {
                TextureCache.Add(iconTexture.name, new());
                EmissionTextureCache.Add(iconTexture.name, new());
            }

            if (TextureCache[iconTexture.name].ContainsKey(cost))
                return new(TextureCache[iconTexture.name][cost], EmissionTextureCache[iconTexture.name][cost]);
        }

        Texture2D newTexture = TextureHelper.DuplicateTexture(BackgroundTexture);
        newTexture.name = $"{iconTexture.name}_{cost}";

        Texture2D newEmission = new(newTexture.width, newTexture.height, newTexture.format, false);
        newEmission.name = $"{iconTexture.name}_{cost}_emission";

        Color[] emptyFill = new Color[newEmission.width * newEmission.height];
        Color empty = new(0f, 0f, 0f, 0f);
        for (int i = 0; i < emptyFill.Length; i++)
            emptyFill[i] = empty;

        newEmission.SetPixels(emptyFill);

        // Figure out the maximum number of icons we can fit
        // There is minimum 13 pixel border on either side and a 20 pixel gap between each icon.
        int maxCostIcons = Mathf.FloorToInt((BackgroundTexture.width - (13 * 2) + ICON_SPACING) / (ICON_SPACING + iconTexture.width));

        int rightPad = 13;

        List<Texture2D> texturesRightToLeft = new();
        if (cost <= maxCostIcons && !forceDigitDisplay)
        {
            for (int i = 0; i < cost; i++)
                texturesRightToLeft.Add(iconTexture);
            while (texturesRightToLeft.Count < maxCostIcons)
                texturesRightToLeft.Add(null); // This is a real hack - see below why i do this

            // Recalculate the right pad to center the things
            int width = (ICON_SPACING + iconTexture.width) * maxCostIcons - ICON_SPACING;
            rightPad = Mathf.FloorToInt((BackgroundTexture.width - width) / 2);
        }
        else
        {
            texturesRightToLeft.Add(SevenSegmentDisplay[cost % 10]);

            int tensDigit = Mathf.FloorToInt(cost / 10f) % 10;
            texturesRightToLeft.Add(SevenSegmentDisplay[tensDigit]);

            texturesRightToLeft.Add(SevenSegmentDisplay[10]);
            texturesRightToLeft.Add(iconTexture);
        }

        int leftWidthPad = 0;
        for (int d = 0; d < texturesRightToLeft.Count; d++)
        {
            // Here, 'null' is being used as an indicator that we need to show an "unlit" version of the icon
            float multiplier = 1.0f;
            if (texturesRightToLeft[d] == null)
            {
                texturesRightToLeft[d] = iconTexture;
                multiplier = 0.5f;
            }

            int ty = Mathf.FloorToInt((newTexture.height - texturesRightToLeft[d].height) / 2f);
            leftWidthPad += texturesRightToLeft[d].width;
            int tx = newTexture.width - rightPad - leftWidthPad - d * ICON_SPACING;
            for (int x = 0; x < texturesRightToLeft[d].width; x++)
            {
                for (int y = 0; y < texturesRightToLeft[d].height; y++)
                {
                    Color bc = texturesRightToLeft[d].GetPixel(x, y);
                    if (bc.a > 0f)
                    {                         
                        newTexture.SetPixel(tx + x, ty + y, bc * multiplier);
                        newEmission.SetPixel(tx + x, ty + y, bc * multiplier);
                    }
                }
            }
        }

        newTexture.Apply();
        newEmission.Apply();

        newTexture.filterMode = FilterMode.Point;
        newEmission.filterMode = FilterMode.Point;


        if (!String.IsNullOrEmpty(iconTexture.name) && !iconTexture.name.Equals("untitled", StringComparison.InvariantCultureIgnoreCase))
        {
            TextureCache[iconTexture.name].Add(cost, newTexture);
            EmissionTextureCache[iconTexture.name].Add(cost, newEmission);
        }

        return new (newTexture, newEmission);
    }

    /// <summary>
    /// Displays the given set of cost textures on the container object
    /// </summary>
    /// <param name="texture">The default texture</param>
    /// <param name="emission">The emission texture</param>
    /// <remarks>Does nothing if the game object is not the appropriate game object (i.e., it wasn't created in this helper)</remarks>
    public static void DisplayCostOnContainer(this GameObject gameObject, Texture2D texture, Texture2D emission)
    {
        Transform displayerTransform = gameObject.transform.Find("TextureDisplayer");
        if (displayerTransform == null)
            return;

        if (texture == null && emission == null)
        {
            displayerTransform.gameObject.SetActive(false);
            return;
        }

        displayerTransform.gameObject.SetActive(true);

        Renderer renderer = displayerTransform.gameObject.GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", texture);
        renderer.material.SetTexture("_EmissionMap", emission);
    }

    /// <summary>
    /// Displays the given set of cost textures on the container object
    /// </summary>
    /// <param name="costTextures">The cost textures (as generated by the GetIconifiedCostTexture helper)</param>
    /// <remarks>Does nothing if the game object is not the appropriate game object (i.e., it wasn't created in this helper)</remarks>
    public static void DisplayCostOnContainer(this GameObject gameObject, Tuple<Texture2D, Texture2D> costTextures)
    {
        gameObject.DisplayCostOnContainer(costTextures.Item1, costTextures.Item2);
    }

    private static readonly ConditionalWeakTable<RenderStatsLayer, Dictionary<GemType, Renderer>> GemContainerLookup = new ();

    /// <summary>
    /// Gets a reference to the renderers for the three possible gem costs for the card.
    /// </summary>
    /// <returns>A dictionary mapping GemType to Renderer. The renderers will be null if the container was never created.</returns>
    public static Dictionary<GemType, Renderer> GetGemCostContainer(this RenderStatsLayer layer) => GemContainerLookup.GetOrCreateValue(layer);

    /// <summary>
    /// Gets a reference to the renderers for the three possible gem costs for the card.
    /// </summary>
    /// <param name="force">If true, this will create a gem cost container if it doesn't already exist</param>
    /// <returns>A dictionary mapping GemType to Renderer. The renderers will be null if the container was never created.</returns>
    public static Dictionary<GemType, Renderer> GetGemCostContainer(this DiskCardGame.Card card, bool force = false, GameObject container = null)
    {
        Dictionary<GemType, Renderer> retval = card.StatsLayer.GetGemCostContainer();

        if (retval.ContainsKey(GemType.Blue))
            return retval;

        // First, see if it needs to be created
        GameObject gemContainer = container ?? card.GetPiece(UPPER_GEM_CONTAINER);
        Transform blueCostTransform = container.transform.Find(BLUE_GEM_COST);
        if (blueCostTransform == null && force)
        {
            // Start by making the backup lookup
            CardRenderReverseLookup.Add(card.StatsLayer, card);

            // Make each gem
            GameObject blueCost = GameObject.Instantiate(card.GetPiece(BLUE_GEM_ORIGINAL), gemContainer.transform);
            blueCost.name = BLUE_GEM_COST;
            blueCost.transform.localScale = new (650f, 200f, 450f);
            blueCost.transform.localPosition = new (-0.3f, 0.36f, 0f);
            blueCost.transform.localEulerAngles = new (270f, 90f, 0f);
            retval[GemType.Blue] = blueCost.GetComponent<Renderer>();

            GameObject greenCost = GameObject.Instantiate(card.GetPiece(GREEN_GEM_ORIGINAL), gemContainer.transform);
            greenCost.name = GREEN_GEM_COST;
            greenCost.transform.localScale = new (650f, 200f, 450f);
            greenCost.transform.localEulerAngles = new (270f, 90f, 0f);
            greenCost.transform.localPosition = new (0f, 0.39f, 0f);
            retval[GemType.Green] = greenCost.GetComponent<Renderer>();

            GameObject orangeCost = GameObject.Instantiate(card.GetPiece(ORANGE_GEM_ORIGINAL), gemContainer.transform);
            orangeCost.name = ORANGE_GEM_COST;
            orangeCost.transform.localScale = new (650f, 200f, 450f);
            orangeCost.transform.localEulerAngles = new (270f, 90f, 0f);
            orangeCost.transform.localPosition = new (0.3f, 0.41f, 0f);
            retval[GemType.Orange] = orangeCost.GetComponent<Renderer>();
        }
        else if (blueCostTransform == null)
        {
            retval[GemType.Blue] = null;
            retval[GemType.Green] = null;
            retval[GemType.Orange] = null;
        }
        else
        {
            retval[GemType.Blue] = gemContainer.transform.Find(BLUE_GEM_COST).gameObject.GetComponent<Renderer>();
            retval[GemType.Green] = gemContainer.transform.Find(GREEN_GEM_COST).gameObject.GetComponent<Renderer>();
            retval[GemType.Orange] = gemContainer.transform.Find(ORANGE_GEM_COST).gameObject.GetComponent<Renderer>();
        }

        return retval;
    }

    private static void ResetBlueGemifyGem(DiskCardGame.Card card, bool originalPosition, bool active)
    {
        GameObject gem = card.GetPiece(BLUE_GEM_ORIGINAL);
        if (gem == null)
            return;

        gem.SetActive(active);

        if (originalPosition)
        {
            gem.transform.localPosition = new (0.0935f, 0f, 0.162f);
            gem.transform.localScale = new (122.6777f, 122.6777f, 100f);
        }
        else
        {
            gem.transform.localPosition = new(0.54f, 0f, 0f);
            gem.transform.localScale = new (80f, 80f, 100f);
        }
    }

    [HarmonyPatch(typeof(DiskCardGame.Card), nameof(DiskCardGame.Card.RenderCard))]
    [HarmonyPostfix]
    private static void DisplayAlternateCostSingle(DiskCardGame.Card __instance)
    {
        // This will render alternate costs to the cost window.
        // Note that right now this only support single alternate costs, not combined costs
        if (__instance.StatsLayer is not DiskRenderStatsLayer drsl)
            return;

        // Okay, first we need to figure out how many costs we actually have
        List<CustomCostRenderInfo> costDisplays = new();
        PlayableCard playableCard = __instance as PlayableCard;

        int energyCost = playableCard?.EnergyCost ?? __instance.Info.EnergyCost;
        if (energyCost > 0)
            costDisplays.Add(new("Energy"));

        List<GemType> gemsCost = playableCard?.GemsCost() ?? __instance.Info.GemsCost;
        if (gemsCost.Count > 0)
            costDisplays.Add(new("Gems"));

        int bonesCost = playableCard?.BonesCost() ?? __instance.Info.BonesCost;
        if (bonesCost > 0)
            costDisplays.Add(new("Bones", GetIconifiedCostTexture(BoneCostIcon, bonesCost, forceDigitDisplay: true)));
        
        int bloodCost = playableCard?.BloodCost() ?? __instance.Info.BloodCost;
        if (bloodCost > 0)
            costDisplays.Add(new("Blood", GetIconifiedCostTexture(BloodCostIcon, bloodCost)));

        // get a list of the custom costs we need textures for
        // check for PlayableCard to account for possible dynamic costs (no API support but who knows what modders do)
        List<CardCostManager.FullCardCost> customCosts;
        if (playableCard != null)
            customCosts = playableCard.GetCustomCardCosts().Select(x => CardCostManager.AllCustomCosts.Find(c => c.CostName == x.CostName)).ToList();
        else
            customCosts = __instance.Info.GetCustomCosts();

        // take the 'one' texture and use it as a base for creating the rest of the textures
        foreach (CardCostManager.FullCardCost fullCost in customCosts)
        {
            Texture2D costTex = null;
            string key = $"{fullCost.ModGUID}_{fullCost.CostName}_1_part3";
            if (CardCostRender.AssembledTextures.ContainsKey(key))
            {
                if (CardCostRender.AssembledTextures[key] != null)
                    costTex = CardCostRender.AssembledTextures[key];
                else
                    CardCostRender.AssembledTextures.Remove(key);
            }
            else
            {
                Texture2D oneTex = fullCost.CostTexture(1, __instance.Info, playableCard);
                if (oneTex != null)
                {
                    costTex = oneTex;
                    CardCostRender.AssembledTextures.Add(key, costTex);
                }
            }
            if (costTex != null)
            {
                int amount = playableCard?.GetCustomCost(fullCost) ?? __instance.Info.GetCustomCost(fullCost);
                costDisplays.Add(new(
                    fullCost.CostName,
                    GetIconifiedCostTexture(AssembledTextures[key], amount)
                    ));
            }
        }

        // Get the other mod costs
        UpdateCardCostSimple?.Invoke(__instance.Info, costDisplays);

        // Quick bailout if possible (for cards that only have energy cost)
        if (costDisplays.Count == 0 || (costDisplays.Count == 1 && energyCost > 0))
        {
            __instance.GetPiece(ENERGY_LIGHTS).SetActive(true);
            ResetBlueGemifyGem(__instance, true, __instance.Info.Gemified);
            return;
        }

        // Get all the cost containers
        List<GameObject> costContainers = __instance.GetCostContainers(costDisplays.Count);
        for (int i = 0; i < costContainers.Count; i++)
        {
            if (i < costDisplays.Count)
            {
                costContainers[i].SetActive(true);

                if (i > 0)
                {
                    costContainers[i].transform.localPosition = new (
                        costContainers[i].transform.localPosition.x,
                        costContainers[i].transform.localPosition.y,
                        INITIAL_CONTAINER_Z_SPACING + CONTAINER_Z_SPACING * (i - 1)
                    );
                }
                costDisplays[i].CostContainer = costContainers[i];
                costDisplays[i].UpdateDisplayedTextures();
            }
            else
            {
                costContainers[i].SetActive(false);
            }
        }

        // Run the complex update
        UpdateCardCostComplex?.Invoke(__instance.Info, costDisplays);

        // Handle our special case (the gems)
        if (gemsCost.Count > 0)
        {
            CustomCostRenderInfo gemRenderInfo = costDisplays.First(c => c.CostId.Equals("Gems", StringComparison.InvariantCultureIgnoreCase));
            var gemContainer = __instance.GetGemCostContainer(force: true, container: gemRenderInfo.CostContainer);
            foreach (var renderer in gemContainer.Values.Where(v => v != null))
                renderer.gameObject.SetActive(true);
        }

        // Handle the energy bars
        bool energyLightsActive = energyCost > 0 || costDisplays.Count == 0;
        __instance.GetPiece(ENERGY_LIGHTS).SetActive(energyLightsActive);
        ResetBlueGemifyGem(__instance, true, __instance.Info.Gemified && energyLightsActive);
    }

    [HarmonyPatch(typeof(DiskRenderStatsLayer), nameof(DiskRenderStatsLayer.ManagedUpdate))]
    [HarmonyPostfix]
    private static void UpdateGemsCost(DiskRenderStatsLayer __instance)
    {
        var gemContainer = __instance.GetGemCostContainer();
        foreach (var gem in gemContainer.Keys)
        {
            if (gemContainer[gem] == null || !gemContainer[gem].gameObject.activeInHierarchy)
                continue;

            DiskCardGame.Card card = __instance.GetCard();
            Color emissionColor = Color.black;
            if (card.Info.gemsCost.Contains(gem))
                emissionColor = !GameFlowManager.IsCardBattle || ResourcesManager.Instance.HasGem(gem) ? Color.white : Color.gray;
            gemContainer[gem].GetPropertyBlock(__instance.gemsPropertyBlock);
            __instance.gemsPropertyBlock.SetColor("_EmissionColor", emissionColor);
            gemContainer[gem].SetPropertyBlock(__instance.gemsPropertyBlock);
        }
    }
}