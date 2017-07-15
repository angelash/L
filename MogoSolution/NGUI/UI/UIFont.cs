//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// UIFont contains everything needed to be able to print text.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public partial class UIFont : MonoBehaviour
{
	//UNISIP
	
    //store a list of all dynamic fonts used, in order to trigger callbacks when a font texture is rebuilt
    static List<Font> _fontList = new List<Font>();

    public static void RegisterFont(UIFont font)
    {
        if (font.UseDynamicFont && !_fontList.Contains(font.dynamicFont))
        {
            _fontList.Add(font.dynamicFont);
            font.dynamicFont.textureRebuildCallback += OnFontRebuilt;
        }
    }

    static void OnFontRebuilt()
    {
        //rebuild everyone (the callback doesn't tell us which font it is, so we can't filter any better)
        UILabel[] list = GameObject.FindObjectsOfType(typeof(UILabel)) as UILabel[];
        foreach (UILabel label in list)
            label.MarkAsChanged();

        EmojiUILabel[] list1 = GameObject.FindObjectsOfType(typeof(EmojiUILabel)) as EmojiUILabel[];
        foreach (EmojiUILabel label in list1)
            label.MarkAsChanged();
    }

    public static void OnFontRebuilt(UIFont font)
    {
        UILabel[] list = GameObject.FindObjectsOfType(typeof(UILabel)) as UILabel[];
        foreach (UILabel label in list)
        {
            if (label.font == font)
                label.MarkAsChanged();
        }

        EmojiUILabel[] list1 = GameObject.FindObjectsOfType(typeof(EmojiUILabel)) as EmojiUILabel[];
        foreach (EmojiUILabel label in list1)
        {
            if (label.font == font)
                label.MarkAsChanged();
        }
    }


    //UNISIP - ADDITIONAL FIELDS FOR DYNAMIC FONTS
    public Font dynamicFont;
    public Material dynamicFontMaterial;
    public int dynamicFontSize = 12;
    public FontStyle dynamicFontStyle;

    public bool UseDynamicFont { get { return (dynamicFont != null); } }


	public enum Alignment
	{
		Left,
		Center,
		Right,
	}

	public enum SymbolStyle
	{
		None,
		Uncolored,
		Colored,
	}

	[HideInInspector][SerializeField] Material mMat;
	[HideInInspector][SerializeField] Rect mUVRect = new Rect(0f, 0f, 1f, 1f);
	[HideInInspector][SerializeField] BMFont mFont = new BMFont();
	[HideInInspector][SerializeField] UIAtlas mAtlas;
	[HideInInspector][SerializeField] UIFont mReplacement;
	[HideInInspector][SerializeField] float mPixelSize = 1f;

	// List of symbols, such as emoticons like ":)", ":(", etc
	[HideInInspector][SerializeField] List<BMSymbol> mSymbols = new List<BMSymbol>();

	// Cached value
	UIAtlas.Sprite mSprite = null;
	int mPMA = -1;

	// BUG: There is a bug in Unity 3.4.2 and all the way up to 3.5 b7 -- when instantiating from prefabs,
	// for some strange reason classes get initialized with default values. So for example, 'mSprite' above
	// gets initialized as if it was created with 'new UIAtlas.Sprite()' instead of 'null'. Fun, huh?
	// EDIT: It seems this value is SAVED as well. Dafuq...

	bool mSpriteSet = false;

	// I'd use a Stack here, but then Flash export wouldn't work as it doesn't support it
	List<Color> mColors = new List<Color>();

	/// <summary>
	/// Access to the BMFont class directly.
	/// </summary>

	public BMFont bmFont { get { return (mReplacement != null) ? mReplacement.bmFont : mFont; } }

	/// <summary>
	/// Original width of the font's texture in pixels.
	/// </summary>

	public int texWidth { get { return (mReplacement != null) ? mReplacement.texWidth : ((mFont != null) ? mFont.texWidth : 1); } }

	/// <summary>
	/// Original height of the font's texture in pixels.
	/// </summary>

	public int texHeight { get { return (mReplacement != null) ? mReplacement.texHeight : ((mFont != null) ? mFont.texHeight : 1); } }

	/// <summary>
	/// Whether the font has any symbols defined.
	/// </summary>

	public bool hasSymbols { get { return (mReplacement != null) ? mReplacement.hasSymbols : mSymbols.Count != 0; } }

	/// <summary>
	/// List of symbols within the font.
	/// </summary>

	public List<BMSymbol> symbols { get { return (mReplacement != null) ? mReplacement.symbols : mSymbols; } }

	/// <summary>
	/// Atlas used by the font, if any.
	/// </summary>

	public UIAtlas atlas
	{
		get
		{
			return (mReplacement != null) ? mReplacement.atlas : mAtlas;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.atlas = value;
			}
			else if (mAtlas != value)
			{
				if (value == null)
				{
					if (mAtlas != null) mMat = mAtlas.spriteMaterial;
					if (sprite != null) mUVRect = uvRect;
				}

				mPMA = -1;
				mAtlas = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Get or set the material used by this font.
	/// </summary>

	public Material material
	{
		get
		{
            //UNISIP
            if (UseDynamicFont)
            {
                if (dynamicFontMaterial != null)
                {
                    dynamicFontMaterial.mainTexture = dynamicFont.material.mainTexture;
                    return dynamicFontMaterial;

                }
                return dynamicFont.material;
            }

			if (mReplacement != null) return mReplacement.material;
			return (mAtlas != null) ? mAtlas.spriteMaterial : mMat;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.material = value;
			}
			else if (mAtlas == null && mMat != value)
			{
				mPMA = -1;
				mMat = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Pixel size is a multiplier applied to label dimensions when performing MakePixelPerfect() pixel correction.
	/// Most obvious use would be on retina screen displays. The resolution doubles, but with UIRoot staying the same
	/// for layout purposes, you can still get extra sharpness by switching to an HD font that has pixel size set to 0.5.
	/// </summary>

	public float pixelSize
	{
		get
		{
			if (mReplacement != null) return mReplacement.pixelSize;
			if (mAtlas != null) return mAtlas.pixelSize;
			return mPixelSize;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.pixelSize = value;
			}
			else if (mAtlas != null)
			{
				mAtlas.pixelSize = value;
			}
			else
			{
				float val = Mathf.Clamp(value, 0.25f, 4f);

				if (mPixelSize != val)
				{
					mPixelSize = val;
					MarkAsDirty();
				}
			}
		}
	}

	/// <summary>
	/// Whether the font is using a premultiplied alpha material.
	/// </summary>

	public bool premultipliedAlpha
	{
		get
		{
			if (mReplacement != null) return mReplacement.premultipliedAlpha;

			if (mAtlas != null) return mAtlas.premultipliedAlpha;

			if (mPMA == -1)
			{
				Material mat = material;
				mPMA = (mat != null && mat.shader != null && mat.shader.name.Contains("Premultiplied")) ? 1 : 0;
			}
			return (mPMA == 1);
		}
	}

	/// <summary>
	/// Convenience function that returns the texture used by the font.
	/// </summary>

	public Texture2D texture
	{
		get
		{
			if (mReplacement != null) return mReplacement.texture;
			Material mat = material;
			return (mat != null) ? mat.mainTexture as Texture2D : null;
		}
	}

	/// <summary>
	/// Offset and scale applied to all UV coordinates.
	/// </summary>

	public Rect uvRect
	{
		get
		{
			if (mReplacement != null) return mReplacement.uvRect;

			if (mAtlas != null && (mSprite == null && sprite != null))
			{
				Texture tex = mAtlas.texture;

				if (tex != null)
				{
					mUVRect = mSprite.outer;

					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						mUVRect = NGUIMath.ConvertToTexCoords(mUVRect, tex.width, tex.height);
					}

					// Account for trimmed sprites
					if (mSprite.hasPadding)
					{
						Rect rect = mUVRect;
						mUVRect.xMin = rect.xMin - mSprite.paddingLeft * rect.width;
						mUVRect.yMin = rect.yMin - mSprite.paddingBottom * rect.height;
						mUVRect.xMax = rect.xMax + mSprite.paddingRight * rect.width;
						mUVRect.yMax = rect.yMax + mSprite.paddingTop * rect.height;
					}
#if UNITY_EDITOR
					// The font should always use the original texture size
					if (mFont != null)
					{
						float tw = (float)mFont.texWidth / tex.width;
						float th = (float)mFont.texHeight / tex.height;

						if (tw != mUVRect.width || th != mUVRect.height)
						{
							//Debug.LogWarning("Font sprite size doesn't match the expected font texture size.\n" +
							//	"Did you use the 'inner padding' setting on the Texture Packer? It must remain at '0'.", this);
							mUVRect.width = tw;
							mUVRect.height = th;
						}
					}
#endif
					// Trimmed sprite? Trim the glyphs
					if (mSprite.hasPadding) Trim();
				}
			}
			return mUVRect;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.uvRect = value;
			}
			else if (sprite == null && mUVRect != value)
			{
				mUVRect = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Sprite used by the font, if any.
	/// </summary>

	public string spriteName
	{
		get
		{
			return (mReplacement != null) ? mReplacement.spriteName : mFont.spriteName;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.spriteName = value;
			}
			else if (mFont.spriteName != value)
			{
				mFont.spriteName = value;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Pixel-perfect size of this font.
	/// </summary>

	public int size { get { return (mReplacement != null) ? mReplacement.size : charSize; } }

	/// <summary>
	/// Retrieves the sprite used by the font, if any.
	/// </summary>

	public UIAtlas.Sprite sprite
	{
		get
		{
			if (mReplacement != null) return mReplacement.sprite;

			if (!mSpriteSet) mSprite = null;

			if (mSprite == null)
			{
				if (mAtlas != null && !string.IsNullOrEmpty(mFont.spriteName))
				{
					mSprite = mAtlas.GetSprite(mFont.spriteName);

					if (mSprite == null) mSprite = mAtlas.GetSprite(name);

					mSpriteSet = true;

					if (mSprite == null) mFont.spriteName = null;
				}

				for (int i = 0, imax = mSymbols.Count; i < imax; ++i)
					symbols[i].MarkAsDirty();
			}
			return mSprite;
		}
	}

	/// <summary>
	/// Setting a replacement atlas value will cause everything using this font to use the replacement font instead.
	/// Suggested use: set up all your widgets to use a dummy font that points to the real font. Switching that font to
	/// another one (for example an eastern language one) is then a simple matter of setting this field on your dummy font.
	/// </summary>

	public UIFont replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIFont rep = value;
			if (rep == this) rep = null;

			if (mReplacement != rep)
			{
				if (rep != null && rep.replacement == this) rep.replacement = null;
				if (mReplacement != null) MarkAsDirty();
				mReplacement = rep;
				MarkAsDirty();
			}
		}
	}

	/// <summary>
	/// Trim the glyphs, making sure they never go past the trimmed texture bounds.
	/// </summary>

	void Trim ()
	{
		Texture tex = mAtlas.texture;

		if (tex != null && mSprite != null)
		{
			Rect full = NGUIMath.ConvertToPixels(mUVRect, texture.width, texture.height, true);
			Rect trimmed = (mAtlas.coordinates == UIAtlas.Coordinates.TexCoords) ?
				NGUIMath.ConvertToPixels(mSprite.outer, tex.width, tex.height, true) : mSprite.outer;

			int xMin = Mathf.RoundToInt(trimmed.xMin - full.xMin);
			int yMin = Mathf.RoundToInt(trimmed.yMin - full.yMin);
			int xMax = Mathf.RoundToInt(trimmed.xMax - full.xMin);
			int yMax = Mathf.RoundToInt(trimmed.yMax - full.yMin);

			mFont.Trim(xMin, yMin, xMax, yMax);
		}
	}

	/// <summary>
	/// Helper function that determines whether the font uses the specified one, taking replacements into account.
	/// </summary>

	bool References (UIFont font)
	{
		if (font == null) return false;
		if (font == this) return true;
		return (mReplacement != null) ? mReplacement.References(font) : false;
	}

	/// <summary>
	/// Helper function that determines whether the two atlases are related.
	/// </summary>

	static public bool CheckIfRelated (UIFont a, UIFont b)
	{
		if (a == null || b == null) return false;
		return a == b || a.References(b) || b.References(a);
	}

	/// <summary>
	/// Refresh all labels that use this font.
	/// </summary>

	public void MarkAsDirty ()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
		if (mReplacement != null) mReplacement.MarkAsDirty();

		mSprite = null;
		UILabel[] labels = NGUITools.FindActive<UILabel>();

		for (int i = 0, imax = labels.Length; i < imax; ++i)
		{
			UILabel lbl = labels[i];
			
			if (lbl.enabled && NGUITools.GetActive(lbl.gameObject) && CheckIfRelated(this, lbl.font))
			{
				UIFont fnt = lbl.font;
				lbl.font = null;
				lbl.font = fnt;
			}
		}

		// Clear all symbols
		for (int i = 0, imax = mSymbols.Count; i < imax; ++i)
			symbols[i].MarkAsDirty();
	}

	/// <summary>
	/// Get the printed size of the specified string. The returned value is in local coordinates. Multiply by transform's scale to get pixels.
	/// </summary>

    public Vector2 CalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle, int spacingX = 0, int spacingY = 0,bool tranlateReturn = true)
	{
        if (mReplacement != null) return mReplacement.CalculatePrintedSize(text, encoding, symbolStyle, spacingX, spacingY);

		Vector2 v = Vector2.zero;

		if (UseDynamicFont || (mFont != null && mFont.isValid && !string.IsNullOrEmpty(text)))
		{
			if (encoding) text = NGUITools.StripSymbols(text, tranlateReturn);

            if (UseDynamicFont)
                dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);
            CharacterInfo charInfo;

			int length = text.Length;
			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
            int lineHeight = (charSize + spacingY);
			bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols;

			for (int i = 0; i < length; ++i)
			{
				char c = text[i];

				// Start a new line
				if (c == '\n')
				{
					if (x > maxX) maxX = x;
					x = 0;
					y += lineHeight;
					prev = 0;
					continue;
				}

				// Skip invalid characters
				if (c < ' ') { prev = 0; continue; }

				if (UseDynamicFont)
                {
                    if (dynamicFont.GetCharacterInfo(c, out charInfo, dynamicFontSize, dynamicFontStyle))
                    {
                        x += (int)(spacingX + charInfo.width);
                    }
                    else
                    {
                    }
                }
                else
                {
					// See if there is a symbol matching this text
					BMSymbol symbol = useSymbols ? MatchSymbol(text, i, length) : null;

					if (symbol == null)
					{
						// Get the glyph for this character
						BMGlyph glyph = mFont.GetGlyph(c);

						if (glyph != null)
						{
                            x += spacingX + ((prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance);
							prev = c;
						}
					}
					else
					{
						// Symbol found -- use it
                        x += spacingX + symbol.advance;
						i += symbol.length - 1;
						prev = 0;
					}
				}
			}

			// Convert from pixel coordinates to local coordinates
			float scale = (charSize > 0) ? 1f / charSize : 1f;
			v.x = scale * ((x > maxX) ? x : maxX);
			v.y = scale * (y + lineHeight);
		}
		return v;
	}

	//UNISIP
    int charSize
    {
        get
        {
            return (UseDynamicFont ? dynamicFontSize : mFont.charSize);
        }
    }

	/// <summary>
	/// Convenience function that ends the line by either appending a new line character or replacing a space with one.
	/// </summary>

	static void EndLine (ref StringBuilder s)
	{
		int i = s.Length - 1;
		if (i > 0 && s[i] == ' ') s[i] = '\n';
		else s.Append('\n');
	}

	/// <summary>
	/// Different line wrapping functionality -- contributed by MightyM.
	/// http://www.tasharen.com/forum/index.php?topic=1049.0
	/// </summary>

    public string GetEndOfLineThatFits(string text, float maxWidth, bool encoding, SymbolStyle symbolStyle, int spacingX = 0)
	{
        if (mReplacement != null) return mReplacement.GetEndOfLineThatFits(text, maxWidth, encoding, symbolStyle, spacingX);

		int lineWidth = Mathf.RoundToInt(maxWidth * size);
		if (lineWidth < 1) return text;

		int textLength = text.Length;
		int remainingWidth = lineWidth;
		BMGlyph followingGlyph = null;
		int currentCharacterIndex = textLength;
		bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols;
		if (UseDynamicFont)
            dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);
        CharacterInfo charInfo;

		while (currentCharacterIndex > 0 && remainingWidth > 0)
		{
			char currentCharacter = text[--currentCharacterIndex];

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? MatchSymbol(text, currentCharacterIndex, textLength) : null;

			// Calculate how wide this symbol or character is going to be
            int glyphWidth = spacingX;

			if (UseDynamicFont)
            {
                if (dynamicFont.GetCharacterInfo(currentCharacter, out charInfo, dynamicFontSize, dynamicFontStyle))
                {
                    glyphWidth += (int)charInfo.width;
                }
            }
			else
			{
				if	 (symbol != null)
				{
					glyphWidth += symbol.advance;
				}
				else
				{
					// Find the glyph for this character
					BMGlyph glyph = mFont.GetGlyph(currentCharacter);

					if (glyph != null)
					{
						glyphWidth += glyph.advance + ((followingGlyph == null) ? 0 : followingGlyph.GetKerning(currentCharacter));
						followingGlyph = glyph;
					}
					else
					{
						followingGlyph = null;
						continue;
					}
				}
			}

			// Remaining width after this glyph gets printed
			remainingWidth -= glyphWidth;
		}
		if (remainingWidth < 0) ++currentCharacterIndex;
		return text.Substring(currentCharacterIndex, textLength - currentCharacterIndex);
	}


	/// <summary>
	/// Text wrapping functionality. The 'maxWidth' should be in local coordinates (take pixels and divide them by transform's scale).
	/// </summary>

    public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle, int spacingX = 0)
	{
        if (mReplacement != null) return mReplacement.WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, spacingX);

		// Width of the line in pixels
		int lineWidth = Mathf.RoundToInt(maxWidth * size);
		if (lineWidth < 1) return text;

		StringBuilder sb = new StringBuilder();
		int textLength = text.Length;
		int remainingWidth = lineWidth;
		int previousChar = 0;
		int start = 0;
		int offset = 0;
		bool lineIsEmpty = true;
		bool multiline = (maxLineCount != 1);
		int lineCount = 1;
		bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols;
		 if (UseDynamicFont)
            dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);
        CharacterInfo charInfo;


		// Run through all characters
		for (; offset < textLength; ++offset)
		{
			char ch = text[offset];

			// New line character -- start a new line
			if (ch == '\n')
			{
				if (!multiline || lineCount == maxLineCount ) break;
				remainingWidth = lineWidth;

				// Add the previous word to the final string
				if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
				else sb.Append(ch);

				lineIsEmpty = true;
				++lineCount;
				start = offset + 1;
				previousChar = 0;
				continue;
			}

			// If this marks the end of a word, add it to the final string.
			if (ch == ' ' && previousChar != ' ' && start < offset)
            {
                sb.Append(text.Substring(start, offset - start + 1));
                //lineIsEmpty = false; -----MaiFeo 原来是有滴现在被我注释掉了。。。
                start = offset + 1;
                previousChar = ch;
			}

			// When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
			if (encoding && ch == '[') 
			{
				if (offset + 2 < textLength)
				{
					if (text[offset + 1] == '-' && text[offset + 2] == ']')  
					{
						offset += 2;
						continue;
					}
					else if (offset + 7 < textLength && text[offset + 7] == ']') 
					{
						if (NGUITools.EncodeColor(NGUITools.ParseColor(text, offset + 1)) == text.Substring(offset + 1, 6).ToUpper())
						{
							offset += 7;
							continue;
						}
					}
                    else if (text[offset + 1] == 'u' && text[offset + 2] == ']')
                    {
                        offset += 2;
                        continue;
                    }
                    else if (offset + 3 < textLength && text[offset + 1] == '/' && text[offset + 2] == 'u' && text[offset + 3] == ']')
                    {
                        offset += 3;
                        continue;

                    }
                }
			}

			// See if there is a symbol matching this text
			BMSymbol symbol = useSymbols ? MatchSymbol(text, offset, textLength) : null;

			// Calculate how wide this symbol or character is going to be
            int glyphWidth = spacingX;

			if (UseDynamicFont)
            {
                if (dynamicFont.GetCharacterInfo(ch, out charInfo, dynamicFontSize, dynamicFontStyle))
                {
                    glyphWidth += (int)charInfo.width;
                }
            }
			else
			{
				if (symbol != null)
				{
					glyphWidth += symbol.advance;
				}
				else
				{
					// Find the glyph for this character
					BMGlyph glyph = (symbol == null) ? mFont.GetGlyph(ch) : null;

					if (glyph != null)
					{
						glyphWidth += (previousChar != 0) ? glyph.advance + glyph.GetKerning(previousChar) : glyph.advance;
					}
					else continue;
				}
			}

			// Remaining width after this glyph gets printed
			remainingWidth -= glyphWidth;

			// Doesn't fit?
			if (remainingWidth < 0)
			{
				// Can't start a new line
				if (lineIsEmpty || !multiline || lineCount == maxLineCount)
				{
					// This is the first word on the line -- add it up to the character that fits
					sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));

					if (!multiline || lineCount == maxLineCount)
					{
						start = offset;
						break;
					}
					EndLine(ref sb);

					// Start a brand-new line
					lineIsEmpty = true;
					++lineCount;

					if (ch == ' ')
					{
						start = offset + 1;
						remainingWidth = lineWidth;
					}
					else
					{
						start = offset;
						remainingWidth = lineWidth - glyphWidth;
					}
					previousChar = 0;
				}
				else
				{
					// Skip all spaces before the word
					while (start < textLength && text[start] == ' ') ++start;

					// Revert the position to the beginning of the word and reset the line
					lineIsEmpty = true;
					remainingWidth = lineWidth;
					offset = start - 1;
					previousChar = 0;
					if (!multiline || lineCount == maxLineCount) break;
					++lineCount;
					EndLine(ref sb);
					continue;
				}
			}
			else previousChar = ch;

			// Advance the offset past the symbol
			if (!UseDynamicFont && symbol != null)
			{
				offset += symbol.length - 1;
				previousChar = 0;
			}
		}

		if (start < offset) sb.Append(text.Substring(start, offset - start));
		return sb.ToString();
	}

	/// <summary>
	/// Text wrapping functionality. Legacy compatibility function.
	/// </summary>

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding) { return WrapText(text, maxWidth, maxLineCount, encoding, SymbolStyle.None); }

	/// <summary>
	/// Text wrapping functionality. Legacy compatibility function.
	/// </summary>

	public string WrapText(string text, float maxWidth, int maxLineCount) { return WrapText(text, maxWidth, maxLineCount, false, SymbolStyle.None); }

	/// <summary>
	/// Align the vertices to be right or center-aligned given the specified line width.
	/// </summary>

	void Align (BetterList<Vector3> verts, int indexOffset, Alignment alignment, int x, int lineWidth)
	{
		if (alignment != Alignment.Left && charSize > 0)
		{
			float offset = (alignment == Alignment.Right) ? lineWidth - x : (lineWidth - x) * 0.5f;
			offset = Mathf.RoundToInt(offset);
			if (offset < 0f) offset = 0f;
			offset /= charSize;

			Vector3 temp;
			for (int i = indexOffset; i < verts.size; ++i)
			{
				temp = verts.buffer[i];
				temp.x += offset;
				verts.buffer[i] = temp;
			}
		}
	}

	/// <summary>
	/// Print the specified text into the buffers.
	/// Note: 'lineWidth' parameter should be in pixels.
	/// </summary>

	public void Print (string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols,
        bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply, int spacingX = 0, int spacingY = 0)
	{
		if (mReplacement != null)
		{
            mReplacement.Print(text, color, verts, uvs, cols, encoding, symbolStyle, alignment, lineWidth, premultiply);
		}
		else if (mFont != null && text != null)
		{
			if (!mFont.isValid && !UseDynamicFont)
			{
				Debug.LogError("Attempting to print using an invalid font!");
				return;
			}

            //JUDIVA, include symbols in font
            RegisterFont(this);
            CharacterInfo unlineInfo;
            unlineInfo.uv = new Rect();
            unlineInfo.vert = new Rect();
            unlineInfo.flipped = true;

            bool has_unline_info = false;
            if (UseDynamicFont)
            {
                dynamicFont.RequestCharactersInTexture("_" + text, dynamicFontSize, dynamicFontStyle);

                if (dynamicFont.GetCharacterInfo('_', out unlineInfo, dynamicFontSize, dynamicFontStyle))
                {
                    has_unline_info = true;
                }
            }
                

			mColors.Clear();
			mColors.Add(color);

			Vector2 scale = charSize > 0 ? new Vector2(1f / charSize, 1f / charSize) : Vector2.one;

			int indexOffset = verts.size;
			int maxX = 0;
			int x = 0;
			int y = 0;
			int prev = 0;
			int lineHeight = (charSize + spacingY);
			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
			float invX = uvRect.width / mFont.texWidth;
			float invY = mUVRect.height / mFont.texHeight;
			int textLength = text.Length;
			bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols && sprite != null;

            bool underline = false;
            int min_under_x = 100000;
            int max_under_x = 0;

			for (int i = 0; i < textLength; ++i)
			{
				char c = text[i];

				if (c == '\n')
				{
                    if (underline && has_unline_info)
                    {
                        v0.x = scale.x * (min_under_x + unlineInfo.vert.xMin);
                        v0.y = scale.y * (-y + unlineInfo.vert.yMax - 0.5f);
                        v1.x = scale.x * max_under_x;
                        v1.y = scale.y * (-y + unlineInfo.vert.yMin - 1.0f);

                        u0.x = unlineInfo.uv.xMin + 0.3f * unlineInfo.uv.width;
                        u1.x = unlineInfo.uv.xMin + 0.7f * unlineInfo.uv.width;

                        for (int b = 0; b < 4; ++b) cols.Add(color);

                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMax));
                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMax));

                        verts.Add(new Vector3(v1.x, v0.y));
                        verts.Add(new Vector3(v0.x, v0.y));
                        verts.Add(new Vector3(v0.x, v1.y));
                        verts.Add(new Vector3(v1.x, v1.y));

                        min_under_x = 100000;
                    }

					if (x > maxX) maxX = x;

					if (alignment != Alignment.Left)
					{
						Align(verts, indexOffset, alignment, x, lineWidth);
						indexOffset = verts.size;
					}

					x = 0;
					y += lineHeight;
					prev = 0;
					continue;
				}

				if (c < ' ')
				{
					prev = 0;
					continue;
				}

                bool temp_under = underline;
                if (encoding && NGUITools.ParseSymbol(text, ref i, mColors, premultiply, ref underline))
				{
                    color = mColors[mColors.Count - 1];
                    --i;

                    if (temp_under && !underline && has_unline_info)
                    {
                        v0.x = scale.x * (min_under_x + unlineInfo.vert.xMin);
                        v0.y = scale.y * (-y + unlineInfo.vert.yMax - 0.5f);
                        v1.x = scale.x * max_under_x;
                        v1.y = scale.y * (-y + unlineInfo.vert.yMin - 1.0f);

                        u0.x = unlineInfo.uv.xMin + 0.3f * unlineInfo.uv.width;
                        u1.x = unlineInfo.uv.xMin + 0.7f * unlineInfo.uv.width;

                        for (int b = 0; b < 4; ++b) cols.Add(color);

                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMax));
                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMax));

                        verts.Add(new Vector3(v1.x, v0.y));
                        verts.Add(new Vector3(v0.x, v0.y));
                        verts.Add(new Vector3(v0.x, v1.y));
                        verts.Add(new Vector3(v1.x, v1.y));

                        min_under_x = 100000;
                    }

                    continue;
				}


				if(!UseDynamicFont)
				{				
				// See if there is a symbol matching this text
					BMSymbol symbol = useSymbols ? MatchSymbol(text, i, textLength) : null;

					if (symbol == null)
					{
						BMGlyph glyph = mFont.GetGlyph(c);
						if (glyph == null) continue;

						if (prev != 0) x += glyph.GetKerning(prev);

						if (c == ' ')
						{
                            x += spacingX + glyph.advance;
							prev = c;
							continue;
						}

						v0.x =  scale.x * (x + glyph.offsetX);
						v0.y = -scale.y * (y + glyph.offsetY);

						v1.x = v0.x + scale.x * glyph.width;
						v1.y = v0.y - scale.y * glyph.height;

						u0.x = mUVRect.xMin + invX * glyph.x;
						u0.y = mUVRect.yMax - invY * glyph.y;

						u1.x = u0.x + invX * glyph.width;
						u1.y = u0.y - invY * glyph.height;

                        x += spacingX + glyph.advance;
						prev = c;

						if (glyph.channel == 0 || glyph.channel == 15)
						{
							for (int b = 0; b < 4; ++b) cols.Add(color);
						}
						else
						{
							// Packed fonts come as alpha masks in each of the RGBA channels.
							// In order to use it we need to use a special shader.
							//
							// Limitations:
							// - Effects (drop shadow, outline) will not work.
							// - Should not be a part of the atlas (eastern fonts rarely are anyway).
							// - Lower color precision

							Color col = color;

							col *= 0.49f;

							switch (glyph.channel)
							{
								case 1: col.b += 0.51f; break;
								case 2: col.g += 0.51f; break;
								case 4: col.r += 0.51f; break;
								case 8: col.a += 0.51f; break;
							}

							for (int b = 0; b < 4; ++b) cols.Add(col);
						}
					}
					else
					{
						v0.x =  scale.x * (x + symbol.offsetX);
						v0.y = -scale.y * (y + symbol.offsetY);

						v1.x = v0.x + scale.x * symbol.width;
						v1.y = v0.y - scale.y * symbol.height;

						Rect uv = symbol.uvRect;

						u0.x = uv.xMin;
						u0.y = uv.yMax;
						u1.x = uv.xMax;
						u1.y = uv.yMin;

                        x += spacingX + symbol.advance;
						i += symbol.length - 1;
						prev = 0;

						if (symbolStyle == SymbolStyle.Colored)
						{
							for (int b = 0; b < 4; ++b) cols.Add(color); 
						}
						else
						{
							Color32 col = Color.white;
							col.a = color.a;
							for (int b = 0; b < 4; ++b) cols.Add(col);
						}
					}

					verts.Add(new Vector3(v1.x, v0.y));
					verts.Add(new Vector3(v1.x, v1.y));
					verts.Add(new Vector3(v0.x, v1.y));
					verts.Add(new Vector3(v0.x, v0.y));

					uvs.Add(new Vector2(u1.x, u0.y));
					uvs.Add(new Vector2(u1.x, u1.y));
					uvs.Add(new Vector2(u0.x, u1.y));
					uvs.Add(new Vector2(u0.x, u0.y));
			
		
				}
				else
                {
                    //v0 v1 are the two corners
                    CharacterInfo charInfo;
                    if (!dynamicFont.GetCharacterInfo(c, out charInfo, dynamicFontSize, dynamicFontStyle))
                    {
                        Debug.LogError("character not found in font:"+c);
                        continue;
                    }

                    v0.x = scale.x * (x + charInfo.vert.xMin);
                    v0.y = scale.x * (-y + charInfo.vert.yMax);
                    v1.x =  scale.y * (x + charInfo.vert.xMax);
                    v1.y = scale.y * (-y + charInfo.vert.yMin);

                    u0.x = charInfo.uv.xMin;
                    u0.y = charInfo.uv.yMin;
                    u1.x = charInfo.uv.xMax;
                    u1.y = charInfo.uv.yMax;

                    for (int b = 0; b < 4; ++b) cols.Add(color);

                    if (charInfo.flipped)
                    {
                        //swap entries
                        uvs.Add(new Vector2(u0.x, u1.y));
                        uvs.Add(new Vector2(u0.x, u0.y));
                        uvs.Add(new Vector2(u1.x, u0.y));
                        uvs.Add(new Vector2(u1.x, u1.y));
                    }
                    else
                    {
					
                        uvs.Add(new Vector2(u1.x, u0.y));
                        uvs.Add(new Vector2(u0.x, u0.y));
                        uvs.Add(new Vector2(u0.x, u1.y));
                        uvs.Add(new Vector2(u1.x, u1.y));
                    }

                    verts.Add(new Vector3(v1.x, v0.y));
                    verts.Add(new Vector3(v0.x, v0.y));
                    verts.Add(new Vector3(v0.x, v1.y));
                    verts.Add(new Vector3(v1.x, v1.y));

                    if (underline && has_unline_info)
                    {
                        if (min_under_x > x)
                        {
                            min_under_x = x;
                        }
                        max_under_x = x + (int)charInfo.vert.xMax;
                    }

                    x += spacingX + (int)charInfo.width;
                }
			}

			if (alignment != Alignment.Left && indexOffset < verts.size)
			{
				Align(verts, indexOffset, alignment, x, lineWidth);
				indexOffset = verts.size;
			}
		}
	}

	/// <summary>
	/// Retrieve the specified symbol, optionally creating it if it's missing.
	/// </summary>

	BMSymbol GetSymbol (string sequence, bool createIfMissing)
	{
		for (int i = 0, imax = mSymbols.Count; i < imax; ++i)
		{
			BMSymbol sym = mSymbols[i];
			if (sym.sequence == sequence) return sym;
		}

		if (createIfMissing)
		{
			BMSymbol sym = new BMSymbol();
			sym.sequence = sequence;
			mSymbols.Add(sym);
			return sym;
		}
		return null;
	}

	/// <summary>
	/// Retrieve the symbol at the beginning of the specified sequence, if a match is found.
	/// </summary>

	BMSymbol MatchSymbol (string text, int offset, int textLength)
	{
		// No symbols present
		int count = mSymbols.Count;
		if (count == 0) return null;
		textLength -= offset;

		// Run through all symbols
		for (int i = 0; i < count; ++i)
		{
			BMSymbol sym = mSymbols[i];

			// If the symbol's length is longer, move on
			int symbolLength = sym.length;
			if (symbolLength == 0 || textLength < symbolLength) continue;

			bool match = true;

			// Match the characters
			for (int c = 0; c < symbolLength; ++c)
			{
				if (text[offset + c] != sym.sequence[c])
				{
					match = false;
					break;
				}
			}

			// Match found
			if (match && sym.Validate(atlas)) return sym;
		}
		return null;
	}

	/// <summary>
	/// Add a new symbol to the font.
	/// </summary>

	public void AddSymbol (string sequence, string spriteName)
	{
		BMSymbol symbol = GetSymbol(sequence, true);
		symbol.spriteName = spriteName;
		MarkAsDirty();
	}

	/// <summary>
	/// Remove the specified symbol from the font.
	/// </summary>

	public void RemoveSymbol (string sequence)
	{
		BMSymbol symbol = GetSymbol(sequence, false);
		if (symbol != null) symbols.Remove(symbol);
		MarkAsDirty();
	}

	/// <summary>
	/// Change an existing symbol's sequence to the specified value.
	/// </summary>

	public void RenameSymbol (string before, string after)
	{
		BMSymbol symbol = GetSymbol(before, false);
		if (symbol != null) symbol.sequence = after;
		MarkAsDirty();
	}

	/// <summary>
	/// Whether the specified sprite is being used by the font.
	/// </summary>

	public bool UsesSprite (string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Equals(spriteName))
				return true;

			for (int i = 0, imax = symbols.Count; i < imax; ++i)
			{
				BMSymbol sym = symbols[i];
				if (s.Equals(sym.spriteName))
					return true;
			}
		}
		return false;
    }

    #region 过滤颜色

    /// <summary>
    /// 过滤颜色
    /// </summary> 
    public static string FilterColor(string text)
    {      
        StringBuilder sb = new StringBuilder();
        int textLength = text.Length;      
        int start = 0;
        int offset = 0;

        // Run through all characters
        for (; offset < textLength; ++offset)
        {
            char ch = text[offset];

            // When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
            if (ch == '[')
            {
                if (offset + 2 < textLength)
                {
                    if (text[offset + 1] == '-' && text[offset + 2] == ']')
                    {
                        offset += 2;
                        start = offset + 1;
                        continue;
                    }
                    else if (offset + 7 < textLength && text[offset + 7] == ']')
                    {
                        if (NGUITools.EncodeColor(NGUITools.ParseColor(text, offset + 1)) == text.Substring(offset + 1, 6).ToUpper())
                        {
                            offset += 7;
                            start = offset + 1;
                            continue;
                        }
                    }
                }
            }            

            sb.Append(text.Substring(start, offset - start + 1));
            start = offset + 1;
        }       

        if (start < offset) 
            sb.Append(text.Substring(start, offset - start));
        return sb.ToString();
    }

    #endregion
}

public partial class UIFont
{
    //表情文字包裹
    public string EmojiWrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle,int spacingX = 0, Dictionary<uint, ChatEmoji> emojiMap = null)
    {
        if (mReplacement != null) return mReplacement.WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, spacingX);

        // Width of the line in pixels
        int lineWidth = Mathf.RoundToInt(maxWidth * size);
        if (lineWidth < 1) return text;

        StringBuilder sb = new StringBuilder();
        int textLength = text.Length;
        int remainingWidth = lineWidth;
        int previousChar = 0;
        int start = 0;
        int offset = 0;
        bool lineIsEmpty = true;
        bool multiline = (maxLineCount != 1);
        int lineCount = 1;
        bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols;
        if (UseDynamicFont)
            dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);
        CharacterInfo charInfo;

        // Run through all characters
        for (; offset < textLength; ++offset)
        {
            char ch = text[offset];
            // New line character -- start a new line
            if (ch == '\n')
            {
                if (!multiline || lineCount == maxLineCount) break;
                remainingWidth = lineWidth;

                // Add the previous word to the final string
                if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
                else sb.Append(ch);

                lineIsEmpty = true;
                ++lineCount;
                start = offset + 1;
                previousChar = 0;
                continue;
            }

            // If this marks the end of a word, add it to the final string.
            if (ch == ' ' && previousChar != ' ' && start < offset)
            {
                sb.Append(text.Substring(start, offset - start + 1));
                //lineIsEmpty = false; -----MaiFeo 原来是有滴现在被我注释掉了。。。
                start = offset + 1;
                previousChar = ch;
            }

            // When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
            if (encoding && ch == '[')
            {
                if (offset + 2 < textLength)
                {
                    if (text[offset + 1] == '-' && text[offset + 2] == ']')
                    {
                        offset += 2;
                        continue;
                    }
                    else if (offset + 7 < textLength && text[offset + 7] == ']')
                    {
                        if (NGUITools.EncodeColor(NGUITools.ParseColor(text, offset + 1)) == text.Substring(offset + 1, 6).ToUpper())
                        {
                            offset += 7;
                            continue;
                        }
                    }
                    else if (text[offset + 1] == 'u' && text[offset + 2] == ']')
                    {
                        offset += 2;
                        continue;
                    }
                    else if (offset + 3 < textLength && text[offset + 1] == '/' && text[offset + 2] == 'u' && text[offset + 3] == ']')
                    {
                        offset += 3;
                        continue;
                        
                    }
                }
            }

            //表情处理
            bool isEmoji = false;
            int emojiWidth = 0;
            if (ch == '#')
            {
                if (offset + 1 < textLength)
                {
                    if (text[offset + 1] >= '0' && text[offset + 1] <= '9')
                    {
                        StringBuilder emojiSB = new StringBuilder();
                        emojiSB.Append(text[offset + 1]);
                        for (int i = offset + 2; i < textLength; ++i)
                        {
                            if (text[i] >= '0' && text[i] <= '9')
                            {
                                emojiSB.Append(text[i]);
                                continue;;
                            }
                            break;
                        }

                        uint emojiID;
                        if (uint.TryParse(emojiSB.ToString(),out emojiID))
                        {
                            if (emojiMap.ContainsKey(emojiID))
                            {
                                isEmoji = true;
                                emojiWidth = emojiMap[emojiID].Width;
                                offset += emojiSB.Length;
                            }
                        }

                    }
                }
            }
            //end

            // See if there is a symbol matching this text
            BMSymbol symbol = useSymbols ? MatchSymbol(text, offset, textLength) : null;

            // Calculate how wide this symbol or character is going to be
            int glyphWidth = spacingX;

            if (UseDynamicFont)
            {
                if (isEmoji)
                {
                    glyphWidth += emojiWidth;
                }
                else if (dynamicFont.GetCharacterInfo(ch, out charInfo, dynamicFontSize, dynamicFontStyle))
                {
                    glyphWidth += (int)charInfo.width;
                }
            }
            else
            {
                if (symbol != null)
                {
                    glyphWidth += symbol.advance;
                }
                else
                {
                    // Find the glyph for this character
                    BMGlyph glyph = (symbol == null) ? mFont.GetGlyph(ch) : null;

                    if (glyph != null)
                    {
                        glyphWidth += (previousChar != 0) ? glyph.advance + glyph.GetKerning(previousChar) : glyph.advance;
                    }
                    else continue;
                }
            }

            // Remaining width after this glyph gets printed
            remainingWidth -= glyphWidth;

            // Doesn't fit?
            if (remainingWidth < 0)
            {
                // Can't start a new line
                if (lineIsEmpty || !multiline || lineCount == maxLineCount)
                {
                    //表情
                    if (isEmoji)
                    {
                        while (text[offset] != '#')
                        {
                            --offset;
                        }
                    }

                    // This is the first word on the line -- add it up to the character that fits
                    sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));

                    if (!multiline || lineCount == maxLineCount)
                    {
                        start = offset;
                        break;
                    }
                    EndLine(ref sb);

                    // Start a brand-new line
                    lineIsEmpty = true;
                    ++lineCount;

                    if (!isEmoji)
                    {
                        if (ch == ' ')
                        {
                            start = offset + 1;
                            remainingWidth = lineWidth;
                        }
                        else
                        {
                            start = offset;
                            remainingWidth = lineWidth - glyphWidth;
                        }
                    }
                    else
                    {
                        start = offset;
                        --offset;
                        remainingWidth = lineWidth;
                    }

                    previousChar = 0;
                }
                else
                {
                    // Skip all spaces before the word
                    while (start < textLength && text[start] == ' ') ++start;

                    // Revert the position to the beginning of the word and reset the line
                    lineIsEmpty = true;
                    remainingWidth = lineWidth;
                    offset = start - 1;
                    previousChar = 0;
                    if (!multiline || lineCount == maxLineCount) break;
                    ++lineCount;
                    EndLine(ref sb);
                    continue;
                }
            }
            else previousChar = ch;

            // Advance the offset past the symbol
            if (!UseDynamicFont && symbol != null)
            {
                offset += symbol.length - 1;
                previousChar = 0;
            }
        }

        if (start < offset) sb.Append(text.Substring(start, offset - start));
        return sb.ToString();
    }

    private int GetEmojiMaxHeightSingleLine(int start,int length,string text,int lineHeight,ref bool hasEmoji,Dictionary<uint, ChatEmoji> emojiMap = null)
    {
        for (int i = start; i < length; ++i)
        {
            char c = text[i];
            if (c == '\n')
            {
                break;
            }

            //表情
            if (c == '#')
            {
                if (i + 1 < length)
                {
                    if (text[i + 1] >= '0' && text[i + 1] <= '9')
                    {
                        StringBuilder emojiSB = new StringBuilder();
                        emojiSB.Append(text[i + 1]);
                        for (int index = i + 2; index < length; ++index)
                        {
                            if (text[index] >= '0' && text[index] <= '9')
                            {
                                emojiSB.Append(text[index]);
                                continue; ;
                            }
                            break;
                        }
 
                        uint emojiID;
                        if (uint.TryParse(emojiSB.ToString(), out emojiID))
                        {
                            if (emojiMap.ContainsKey(emojiID))
                            {
                                if (emojiMap[emojiID].Height > lineHeight)
                                {
                                    lineHeight = emojiMap[emojiID].Height;
                                }
                                i += emojiSB.Length;
                                hasEmoji = true;
                            }
                        }
                    }
                }
            }
        }
        return lineHeight;
    }

    public Vector2 EmojiCalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle, int spacingX = 0, int spacingY = 0, bool tranlateReturn = true,Dictionary<uint, ChatEmoji> emojiMap = null)
    {
        if (mReplacement != null) return mReplacement.CalculatePrintedSize(text, encoding, symbolStyle, spacingX, spacingY);

        Vector2 v = Vector2.zero;

        if (UseDynamicFont || (mFont != null && mFont.isValid && !string.IsNullOrEmpty(text)))
        {
            if (encoding) text = NGUITools.StripSymbols(text, tranlateReturn);

            if (UseDynamicFont)
                dynamicFont.RequestCharactersInTexture(text, dynamicFontSize, dynamicFontStyle);
            CharacterInfo charInfo;

            int length = text.Length;
            int maxX = 0;
            int x = 0;
            int y = 0;
            int prev = 0;
            int lineHeight = charSize;
            bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols;

            bool firstLineHasEmoji = false;
            lineHeight = GetEmojiMaxHeightSingleLine(0, length, text, lineHeight,ref firstLineHasEmoji,emojiMap) + spacingY;

            for (int i = 0; i < length; ++i)
            {
                char c = text[i];
                // Start a new line
                if (c == '\n')
                {
                    if (x > maxX) maxX = x;
                    x = 0;
                    y += lineHeight;

                    //表情
                    lineHeight = charSize;
                    bool hasEmoji = false;
                    lineHeight = GetEmojiMaxHeightSingleLine(i + 1, length, text, lineHeight,ref hasEmoji, emojiMap) + spacingY;

                    prev = 0;
                    continue;
                }

                // Skip invalid characters
                if (c < ' ') { prev = 0; continue; }


                //表情处理
                bool isEmoji = false;
                int emojiWidth = 0;
                if (c == '#')
                {
                    if (i + 1 < length)
                    {
                        if (text[i + 1] >= '0' && text[i + 1] <= '9')
                        {
                            StringBuilder emojiSB = new StringBuilder();
                            emojiSB.Append(text[i + 1]);
                            for (int index = i + 2; index < length; ++index)
                            {
                                if (text[index] >= '0' && text[index] <= '9')
                                {
                                    emojiSB.Append(text[index]);
                                    continue; ;
                                }
                                break;
                            }

                            uint emojiID;
                            if (uint.TryParse(emojiSB.ToString(), out emojiID))
                            {
                                if (emojiMap.ContainsKey(emojiID))
                                {
                                    isEmoji = true;
                                    emojiWidth = emojiMap[emojiID].Width;
                                    i += emojiSB.Length;
                                }
                            }
                        }
                    }
                }
                //end

                if (UseDynamicFont)
                {
                    if (isEmoji)
                    {
                        x += (int)(spacingX + emojiWidth);
                    }
                    else if (dynamicFont.GetCharacterInfo(c, out charInfo, dynamicFontSize, dynamicFontStyle))
                    {
                        x += (int)(spacingX + charInfo.width);
                    }
 
                }
                else
                {
                    // See if there is a symbol matching this text
                    BMSymbol symbol = useSymbols ? MatchSymbol(text, i, length) : null;

                    if (symbol == null)
                    {
                        // Get the glyph for this character
                        BMGlyph glyph = mFont.GetGlyph(c);

                        if (glyph != null)
                        {
                            x += spacingX + ((prev != 0) ? glyph.advance + glyph.GetKerning(prev) : glyph.advance);
                            prev = c;
                        }
                    }
                    else
                    {
                        // Symbol found -- use it
                        x += spacingX + symbol.advance;
                        i += symbol.length - 1;
                        prev = 0;
                    }
                }
            }

            // Convert from pixel coordinates to local coordinates
            float scale = (charSize > 0) ? 1f / charSize : 1f;
            v.x = scale * ((x > maxX) ? x : maxX);
            v.y = scale * (y + lineHeight);
        }
        return v;
    }


    /// <summary>
    /// Print the specified text into the buffers.
    /// Note: 'lineWidth' parameter should be in pixels.
    /// </summary>

    public void EmojiPrint(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols,
        bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply, int spacingX = 0, int spacingY = 0, Dictionary<uint, ChatEmoji> emojiMap = null, BetterList<ChatEmojiPos> emojiPosList = null)
    {
        if (mReplacement != null)
        {
            mReplacement.Print(text, color, verts, uvs, cols, encoding, symbolStyle, alignment, lineWidth, premultiply);
        }
        else if (mFont != null && text != null)
        {
            if (!mFont.isValid && !UseDynamicFont)
            {
                Debug.LogError("Attempting to print using an invalid font!");
                return;
            }

            //JUDIVA, include symbols in font
            RegisterFont(this);
            CharacterInfo unlineInfo;
            unlineInfo.uv = new Rect();
            unlineInfo.vert = new Rect();
            unlineInfo.flipped = true;

            bool has_unline_info = false;
            if (UseDynamicFont)
            {
                dynamicFont.RequestCharactersInTexture("_" + text, dynamicFontSize, dynamicFontStyle);

                if (dynamicFont.GetCharacterInfo('_', out unlineInfo, dynamicFontSize, dynamicFontStyle))
                {
                    has_unline_info = true;
                }
            }

            mColors.Clear();
            mColors.Add(color);

            Vector2 scale = charSize > 0 ? new Vector2(1f / charSize, 1f / charSize) : Vector2.one;

            int indexOffset = verts.size;
            int maxX = 0;
            int x = 0;
            int y = 0;
            int prev = 0;
            int lineHeight = charSize;
            Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
            Vector2 u0 = Vector2.zero, u1 = Vector2.zero;
            float invX = uvRect.width / mFont.texWidth;
            float invY = mUVRect.height / mFont.texHeight;
            int textLength = text.Length;
            bool useSymbols = encoding && symbolStyle != SymbolStyle.None && hasSymbols && sprite != null;

            bool underline = false;
            int min_under_x = 100000;
            int max_under_x = 0;

            bool firstLineHasEmoji = false;
            lineHeight = GetEmojiMaxHeightSingleLine(0, textLength, text, lineHeight,ref firstLineHasEmoji,emojiMap) + spacingY;

            BetterList<uint> currentLineEmojiList = new BetterList<uint>();

            for (int i = 0; i < textLength; ++i)
            {
                char c = text[i];
                if (c == '\n')
                {
                    if (underline && has_unline_info)
                    {
                        int tempY = y + lineHeight - (charSize + spacingY);
                        v0.x = scale.x * (min_under_x + unlineInfo.vert.xMin);
                        v0.y = scale.y * (-tempY + unlineInfo.vert.yMax - 0.5f);
                        v1.x = scale.x * max_under_x;
                        v1.y = scale.y * (-tempY + unlineInfo.vert.yMin - 1.0f);

                        u0.x = unlineInfo.uv.xMin + 0.3f * unlineInfo.uv.width;
                        u1.x = unlineInfo.uv.xMin + 0.7f * unlineInfo.uv.width;

                        for (int b = 0; b < 4; ++b) cols.Add(color);

                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMax));
                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMax));

                        verts.Add(new Vector3(v1.x, v0.y));
                        verts.Add(new Vector3(v0.x, v0.y));
                        verts.Add(new Vector3(v0.x, v1.y));
                        verts.Add(new Vector3(v1.x, v1.y));

                        min_under_x = 100000;
                    }

                    if (x > maxX) maxX = x;

                    if (alignment != Alignment.Left)
                    {
                        Align(verts, indexOffset, alignment, x, lineWidth);
                        indexOffset = verts.size;
                        AlignEmoji(emojiPosList, currentLineEmojiList, alignment, x, lineWidth);
                    }

                    x = 0;
                    y += lineHeight;

                    //表情
                    lineHeight = charSize ;
                    bool hasEmoji = false;
                    lineHeight = GetEmojiMaxHeightSingleLine(i + 1, textLength, text, lineHeight,ref hasEmoji,emojiMap) + spacingY;

                    prev = 0;
                    currentLineEmojiList.Clear();
                    continue;
                }

                if (c < ' ')
                {
                    prev = 0;
                    continue;
                }
                    
                bool temp_under = underline;
                if (encoding && NGUITools.ParseSymbol(text, ref i, mColors, premultiply, ref underline))
                {
                    color = mColors[mColors.Count - 1];
                    --i;

                    if (temp_under && !underline && has_unline_info)
                    {
                        int tempY = y + lineHeight - (charSize + spacingY);
                        v0.x = scale.x * (min_under_x + unlineInfo.vert.xMin);
                        v0.y = scale.y * (-tempY + unlineInfo.vert.yMax - 0.5f);
                        v1.x = scale.x * max_under_x;
                        v1.y = scale.y * (-tempY + unlineInfo.vert.yMin - 1.0f);

                        u0.x = unlineInfo.uv.xMin + 0.3f * unlineInfo.uv.width;
                        u1.x = unlineInfo.uv.xMin + 0.7f * unlineInfo.uv.width;

                        for (int b = 0; b < 4; ++b) cols.Add(color);

                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMax));
                        uvs.Add(new Vector2(u0.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMin));
                        uvs.Add(new Vector2(u1.x, unlineInfo.uv.yMax));

                        verts.Add(new Vector3(v1.x, v0.y));
                        verts.Add(new Vector3(v0.x, v0.y));
                        verts.Add(new Vector3(v0.x, v1.y));
                        verts.Add(new Vector3(v1.x, v1.y));

                        min_under_x = 100000;
                    }

                    continue;
                }


                if (!UseDynamicFont)
                {
                    // See if there is a symbol matching this text
                    BMSymbol symbol = useSymbols ? MatchSymbol(text, i, textLength) : null;

                    if (symbol == null)
                    {
                        BMGlyph glyph = mFont.GetGlyph(c);
                        if (glyph == null) continue;

                        if (prev != 0) x += glyph.GetKerning(prev);

                        if (c == ' ')
                        {
                            x += spacingX + glyph.advance;
                            prev = c;
                            continue;
                        }

                        v0.x = scale.x * (x + glyph.offsetX);
                        v0.y = -scale.y * (y + glyph.offsetY);

                        v1.x = v0.x + scale.x * glyph.width;
                        v1.y = v0.y - scale.y * glyph.height;

                        u0.x = mUVRect.xMin + invX * glyph.x;
                        u0.y = mUVRect.yMax - invY * glyph.y;

                        u1.x = u0.x + invX * glyph.width;
                        u1.y = u0.y - invY * glyph.height;

                        x += spacingX + glyph.advance;
                        prev = c;

                        if (glyph.channel == 0 || glyph.channel == 15)
                        {
                            for (int b = 0; b < 4; ++b) cols.Add(color);
                        }
                        else
                        {
                            // Packed fonts come as alpha masks in each of the RGBA channels.
                            // In order to use it we need to use a special shader.
                            //
                            // Limitations:
                            // - Effects (drop shadow, outline) will not work.
                            // - Should not be a part of the atlas (eastern fonts rarely are anyway).
                            // - Lower color precision

                            Color col = color;

                            col *= 0.49f;

                            switch (glyph.channel)
                            {
                                case 1: col.b += 0.51f; break;
                                case 2: col.g += 0.51f; break;
                                case 4: col.r += 0.51f; break;
                                case 8: col.a += 0.51f; break;
                            }

                            for (int b = 0; b < 4; ++b) cols.Add(col);
                        }
                    }
                    else
                    {
                        v0.x = scale.x * (x + symbol.offsetX);
                        v0.y = -scale.y * (y + symbol.offsetY);

                        v1.x = v0.x + scale.x * symbol.width;
                        v1.y = v0.y - scale.y * symbol.height;

                        Rect uv = symbol.uvRect;

                        u0.x = uv.xMin;
                        u0.y = uv.yMax;
                        u1.x = uv.xMax;
                        u1.y = uv.yMin;

                        x += spacingX + symbol.advance;
                        i += symbol.length - 1;
                        prev = 0;

                        if (symbolStyle == SymbolStyle.Colored)
                        {
                            for (int b = 0; b < 4; ++b) cols.Add(color);
                        }
                        else
                        {
                            Color32 col = Color.white;
                            col.a = color.a;
                            for (int b = 0; b < 4; ++b) cols.Add(col);
                        }
                    }

                    verts.Add(new Vector3(v1.x, v0.y));
                    verts.Add(new Vector3(v1.x, v1.y));
                    verts.Add(new Vector3(v0.x, v1.y));
                    verts.Add(new Vector3(v0.x, v0.y));

                    uvs.Add(new Vector2(u1.x, u0.y));
                    uvs.Add(new Vector2(u1.x, u1.y));
                    uvs.Add(new Vector2(u0.x, u1.y));
                    uvs.Add(new Vector2(u0.x, u0.y));


                }
                else
                {
                    //表情处理
                    int emojiWidth = 0;
                    if (c == '#')
                    {
                        if (i + 1 < textLength)
                        {
                            if (text[i + 1] >= '0' && text[i + 1] <= '9')
                            {
                                StringBuilder emojiSB = new StringBuilder();
                                emojiSB.Append(text[i + 1]);
                                for (int index = i + 2; index < textLength; ++index)
                                {
                                    if (text[index] >= '0' && text[index] <= '9')
                                    {
                                        emojiSB.Append(text[index]);
                                        continue; ;
                                    }
                                    break;
                                }

                                uint emojiID;
                                if (uint.TryParse(emojiSB.ToString(), out emojiID))
                                {
                                    if (emojiMap.ContainsKey(emojiID))
                                    {
                                        emojiWidth = emojiMap[emojiID].Width + spacingX;
                                        ChatEmojiPos chatEmojiPos = new ChatEmojiPos();
                                        chatEmojiPos.Id = emojiID;
                                        chatEmojiPos.Position = new Vector2(x + emojiMap[emojiID].Center.x,
                                            y + lineHeight - emojiMap[emojiID].Center.y - spacingY);
                                        emojiPosList.Add(chatEmojiPos);
                                        x += emojiWidth;
                                        i += emojiSB.Length;
                                        currentLineEmojiList.Add(emojiID);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    //end


                    //v0 v1 are the two corners
                    CharacterInfo charInfo;
                    if (!dynamicFont.GetCharacterInfo(c, out charInfo, dynamicFontSize, dynamicFontStyle))
                    {
                        Debug.LogError("character not found in font:" + c);
                        continue;
                    }

                    int tempY = y + lineHeight - (charSize + spacingY);

                    v0.x = scale.x * (x + charInfo.vert.xMin);
                    v0.y = scale.x * (-tempY + charInfo.vert.yMax);
                    v1.x = scale.y * (x + charInfo.vert.xMax);
                    v1.y = scale.y * (-tempY + charInfo.vert.yMin);

                    u0.x = charInfo.uv.xMin;
                    u0.y = charInfo.uv.yMin;
                    u1.x = charInfo.uv.xMax;
                    u1.y = charInfo.uv.yMax;

                    for (int b = 0; b < 4; ++b) cols.Add(color);

                    if (charInfo.flipped)
                    {
                        //swap entries
                        uvs.Add(new Vector2(u0.x, u1.y));
                        uvs.Add(new Vector2(u0.x, u0.y));
                        uvs.Add(new Vector2(u1.x, u0.y));
                        uvs.Add(new Vector2(u1.x, u1.y));
                    }
                    else
                    {

                        uvs.Add(new Vector2(u1.x, u0.y));
                        uvs.Add(new Vector2(u0.x, u0.y));
                        uvs.Add(new Vector2(u0.x, u1.y));
                        uvs.Add(new Vector2(u1.x, u1.y));
                    }

                    verts.Add(new Vector3(v1.x, v0.y));
                    verts.Add(new Vector3(v0.x, v0.y));
                    verts.Add(new Vector3(v0.x, v1.y));
                    verts.Add(new Vector3(v1.x, v1.y));

                    if (underline && has_unline_info)
                    {
                        if (min_under_x > x)
                        {
                            min_under_x = x;
                        }
                        max_under_x = x + (int)charInfo.vert.xMax;
                    }

                    x += spacingX + (int)charInfo.width;
                }
            }

            if (alignment != Alignment.Left && indexOffset < verts.size)
            {
                Align(verts, indexOffset, alignment, x, lineWidth);
                indexOffset = verts.size;
            }
        }
    }

    void AlignEmoji(BetterList<ChatEmojiPos> emojiPosList, BetterList<uint> emojiIndexList,Alignment alignment, int x, int lineWidth)
    {
        if (alignment != Alignment.Left)
        {
            float offset = (alignment == Alignment.Right) ? lineWidth - x : (lineWidth - x) * 0.5f;
            offset = Mathf.RoundToInt(offset);
            if (offset < 0f) offset = 0f;

            for (int i = 0; i < emojiIndexList.size;++i)
            {
                var pos = emojiPosList[(int)emojiIndexList[i]].Position;
                pos.x += offset;
                ChatEmojiPos chatEmojiPos = new ChatEmojiPos();
                chatEmojiPos.Id = emojiPosList[(int) emojiIndexList[i]].Id;
                chatEmojiPos.Position = new Vector2(pos.x,pos.y);
                emojiPosList[(int) emojiIndexList[i]] = chatEmojiPos;
            }
        }
    }
}

public struct ChatEmoji
{
    public uint Id { get; set; }
    public Vector2 Size { get; set; }
    public Vector4 Margin { get; set; }

    public int Width
    {
        get { return (int)(Margin.w + Size.x + Margin.y); }
    }

    public int Height
    {
        get { return (int)(Margin.x + Size.y + Margin.z); }
    }

    public Vector2 Center
    {
        get { return new Vector2(Margin.w + Size.x*0.5f, Margin.z + Size.y*0.5f); }
    }
}

public struct ChatEmojiPos
{
    public uint Id { get; set; }
    public Vector2 Position { get; set; }
}