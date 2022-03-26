using System.Collections;
using System.Text;
using DiskCardGame;
using HarmonyLib;

namespace InscryptionAPI.Helpers.Extensions;

/// <summary>
/// <para>
///     This class is meant to make it easier to create dialogue with TextDisplayer.
///     1. Making a piece of text a specific color
///     2. Wait between words
///     3. Alter emotion at any point
///     4. Alter pitch at any point
///     5. Insert text of a specific size
///
///     For TalkingCards:
///     1. Play a specific animation on the talking card.
///     2. Shake the card itself, lightly or strongly, at a specific point in the dialogue. 
///
///     <example>
///     Adding color:
///     ```csharp
///         yield return DialogueBuilder
///         .Builder
///         .AddText("LET'S TURN UP THE ")
///         .AddText("HEAT!", DialogueEnums.Colors.Red) // will make the word 'HEAT!' appear red.
///         .ShowUntilInput();
///     ```
///
///     Adding size:
///     ```csharp
///         yield return DialogueBuilder
///         .Builder
///         .AddText("LET'S TURN UP THE ")
///         .AddText("HEAT!", DialogueEnums.Size.Giant) // will make the word 'HEAT!' appear as the largest possible size the game allows.
///         .ShowUntilInput();
///     ```
///
///     Adding wait times:
///     Original: `"An' because ye bested me...[w:0.4] with ye,[w:0.3] I'll share me secret."`
///     Builder version:
///     ```csharp
///         yield return DialogueBuilder
///         .Builder
///         .AddText("An' because ye bested me...")
///         .ThenWait(0.4f)
///         .AddText(" with ye,")
///         .ThenWait(0.3f)
///         .AddText(" I'll share me secret")
///         .ShowUntilInput();
///     ```
///     </example>
/// </para>
/// </summary>
public class DialogueBuilder
{
    private static readonly Dictionary<Enum, string> DialogueColorsByEnum = new()
    {
        { DialogueEnums.Colors.Gold, "G" },
        { DialogueEnums.Colors.BrightGold, "bG" },
        { DialogueEnums.Colors.Blue, "B" },
        { DialogueEnums.Colors.BrightBlue, "bB" },
        { DialogueEnums.Colors.DarkBlue, "dB" },
        { DialogueEnums.Colors.Orange, "O" },
        { DialogueEnums.Colors.BrownOrange, "brnO" },
        { DialogueEnums.Colors.Red, "R" },
        { DialogueEnums.Colors.BrightRed, "bR" },
        { DialogueEnums.Colors.LimeGreen, "lGr" },
        { DialogueEnums.Colors.BrightLightGreen, "blGr" },
        { DialogueEnums.Colors.DarkLimeGreen, "dlGr" },
        { DialogueEnums.Colors.DarkSeafoam, "dSG" },
        { DialogueEnums.Colors.GlowSeafoam, "bSG" },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InscryptionAPI.Helpers.Extensions.DialogueBuilder"/> class.
    /// </summary>
    public static DialogueBuilder Builder => new();

    /// <summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder"></see> class.</summary>
    private readonly StringBuilder _builder = new();

    public string Build(bool endLineWithoutNeedingInput = false)
    {
        if (endLineWithoutNeedingInput)
        {
            _builder.Insert(_builder.Length, "[end]");
        }
        return _builder.ToString();
    }

    /// <summary>
    /// A helper method to immediately call ShowUntilInput from <see cref="DiskCardGame.TextDisplayer"/> class without having to wrap the Build call.
    /// </summary>
    /// <param name="endLineWithoutNeedingInput">(ONLY WORKS IN ACT 2) if true, will immediately go to the next line in the dialogue without player input.</param>
    /// <returns>The text being displayed</returns>
    public IEnumerator ShowUntilInput(bool endLineWithoutNeedingInput = false)
    {
        yield return TextDisplayer.Instance.ShowUntilInput(Build(endLineWithoutNeedingInput));
    }

    /// <summary>
    /// A helper method to immediately call ShowThenClear from <see cref="DiskCardGame.TextDisplayer"/> class without having to wrap the Build call.
    /// </summary>
    /// <param name="lengthToShowMessage">How long the message to stay on screen for</param>
    /// <returns>The text being displayed</returns>
    public IEnumerator ShowThenClear(float lengthToShowMessage)
    {
        yield return TextDisplayer.Instance.ShowThenClear(Build(), lengthToShowMessage);
    }

    /// <summary>
    /// A helper method to immediately call ShowMessage from <see cref="DiskCardGame.TextDisplayer"/> class without having to wrap the Build call.
    /// </summary>
    /// <returns>The text being displayed</returns>
    public IEnumerator ShowMessage()
    {
        yield return TextDisplayer.Instance.ShowMessage(Build());
    }

    /// <summary>
    /// Add text at this point in the builder.
    ///
    /// You may also adjust the size and/or color of the text as well.
    /// </summary>
    /// <param name="text">The text to add to the dialogue.</param>
    /// <param name="color">The color the text is displayed in.</param>
    /// <param name="size">How large the text is displayed at. Max is 5, Giant.</param>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder AddText(
        string               text,
        DialogueEnums.Colors color = DialogueEnums.Colors.None,
        DialogueEnums.Size   size  = DialogueEnums.Size.Normal
    )
    {
        Action doAfterTextAppend = null;

        if (size != DialogueEnums.Size.Normal)
        {
            _builder.Append($"[size:{(int)size}]");
            doAfterTextAppend += () => _builder.Append("[size:]");
        }

        if (color != DialogueEnums.Colors.None)
        {
            _builder.Append($"[c:{DialogueColorsByEnum.GetValueSafe(color)}]");
            doAfterTextAppend += () => _builder.Append("[c:]");
        }
        _builder.Append(text);
        doAfterTextAppend?.Invoke();
        return this;
    }

    #region TalkingCardRelated

    /// <summary>
    /// Play the specific animation that is in the Animator object under the CharacterFace class for this card.
    ///
    /// Nothing will happen if the animation does not exist on the talking card.
    /// 
    /// For example, when the Action trigger is setup in the SequentialText class, it will default to the idle animation:
    ///
    /// ```csharp
    /// private void SetFaceAnimTrigger(string trigger)
    ///{
    ///    Face.Anim.SetTrigger(string.IsNullOrEmpty(trigger) ? "idle" : trigger);
    /// }
    /// ```
    ///
    /// The Stoat talking card has this line: "[anim:sly]It was the right play.[w:0.2]"
    /// To create that same string, we can do so like this:
    /// ```csharp
    /// string dialogue = DialogueBuilder.Builder
    /// .PlayAnim("sly")
    /// .AddText("It was the right play.")
    /// .ThenWait(0.2f)
    /// .Build()
    /// ```
    /// </summary>
    /// <param name="animToPlay">The name of the animation to play</param>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder PlayAnim(string animToPlay)
    {
        _builder.Append($"[anim:{animToPlay}]");
        return this;
    }

    /// <summary>
    /// Only works for talking cards or in Act 2.
    ///
    /// Play a light shaking animation for the card at a specific point in the dialogue.
    /// </summary>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder ShakeLightly()
    {
        _builder.Append("[shake:0.5]");
        return this;
    }

    /// <summary>
    /// Only works for talking cards or in Act 2.
    ///
    /// Play a strong shaking animation for the card at a specific point in the dialogue.
    /// </summary>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder ShakeStrongly()
    {
        _builder.Append("[shake:1]");
        return this;
    }

    #endregion

    /// <summary>
    /// Only works for talking cards or in Act 2.
    /// 
    /// Change the emotion that the dialogue will use or change to during the dialogue being shown.
    ///
    /// Possible values to use:
    /// Neutral
    /// Anger
    /// Curious
    /// Laughter
    /// Quiet
    /// Surprise
    /// None
    /// </summary>
    /// <param name="emotion">The emotion to want to change the speaker to use.</param>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder ChangeEmotionTo(Emotion emotion)
    {
        _builder.Append($"[e:{emotion.ToString()}]");
        return this;
    }

    /// <summary>
    /// Adjust the pitch of the speaker. Caps at 1.
    /// </summary>
    /// <param name="pitch">The number to increase the pitch.</param>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder ChangePitchTo(float pitch)
    {
        _builder.Append($"[p:{pitch}]");
        return this;
    }

    /// <summary>
    /// Adds a wait timer between words.
    /// </summary>
    /// <example>
    ///     ```csharp
    ///         yield return DialogueBuilder
    ///         .Builder
    ///         .AddText("An' because ye bested me...")
    ///         .ThenWait(0.4f)
    ///         .AddText(" with ye,")
    ///         .ThenWait(0.3f)
    ///         .AddText(" I'll share me secret")
    ///         .ShowUntilInput();
    ///     ```
    /// </example>
    /// <param name="wait">Seconds to wait before the next word is displayed.</param>
    /// <returns>A reference to this instance after appending has completed.</returns>
    public DialogueBuilder ThenWait(float wait)
    {
        _builder.Append($"[w:{wait}]");
        return this;
    }

}