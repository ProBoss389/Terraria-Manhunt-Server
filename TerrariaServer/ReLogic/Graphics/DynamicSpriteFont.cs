using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Text;

namespace ReLogic.Graphics;

public class DynamicSpriteFont : IFontMetrics
{
	private struct SpriteCharacterData
	{
		public readonly Texture2D Texture;
		public readonly Rectangle Glyph;
		public readonly Rectangle Padding;
		public readonly Vector3 Kerning;

		public SpriteCharacterData(Texture2D texture, Rectangle glyph, Rectangle padding, Vector3 kerning)
		{
			Texture = texture;
			Glyph = glyph;
			Padding = padding;
			Kerning = kerning;
		}

		public GlyphMetrics ToGlyphMetric() => GlyphMetrics.FromKerningData(Kerning.X, Kerning.Y, Kerning.Z);
	}

	private Dictionary<char, SpriteCharacterData> _spriteCharacters = new Dictionary<char, SpriteCharacterData>();
	private SpriteCharacterData _defaultCharacterData;
	public readonly char DefaultCharacter;
	private readonly float _characterSpacing;
	private readonly int _lineSpacing;

	public float CharacterSpacing => _characterSpacing;

	public int LineSpacing => _lineSpacing;

	public DynamicSpriteFont(float spacing, int lineSpacing, char defaultCharacter)
	{
		_characterSpacing = spacing;
		_lineSpacing = lineSpacing;
		DefaultCharacter = defaultCharacter;
	}

	public bool IsCharacterSupported(char character)
	{
		if (character != '\n' && character != '\r')
			return _spriteCharacters.ContainsKey(character);

		return true;
	}

	public bool AreCharactersSupported(IEnumerable<char> characters)
	{
		foreach (char character in characters) {
			if (!IsCharacterSupported(character))
				return false;
		}

		return true;
	}

	internal void SetPages(FontPage[] pages)
	{
		int num = 0;
		FontPage[] array = pages;
		foreach (FontPage fontPage in array) {
			num += fontPage.Characters.Count;
		}

		_spriteCharacters = new Dictionary<char, SpriteCharacterData>(num);
		array = pages;
		foreach (FontPage fontPage2 in array) {
			for (int j = 0; j < fontPage2.Characters.Count; j++) {
				_spriteCharacters.Add(fontPage2.Characters[j], new SpriteCharacterData(fontPage2.Texture, fontPage2.Glyphs[j], fontPage2.Padding[j], fontPage2.Kerning[j]));
				if (fontPage2.Characters[j] == DefaultCharacter)
					_defaultCharacterData = _spriteCharacters[fontPage2.Characters[j]];
			}
		}
	}

	internal void InternalDraw(string text, SpriteBatch spriteBatch, Vector2 startPosition, Color color, float rotation, Vector2 origin, ref Vector2 scale, SpriteEffects spriteEffects, float depth)
	{
		Matrix matrix = Matrix.CreateTranslation((0f - origin.X) * scale.X, (0f - origin.Y) * scale.Y, 0f) * Matrix.CreateRotationZ(rotation);
		Vector2 zero = Vector2.Zero;
		Vector2 one = Vector2.One;
		bool flag = true;
		float x = 0f;
		if (spriteEffects != 0) {
			Vector2 vector = MeasureString(text);
			if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally)) {
				x = vector.X * scale.X;
				one.X = -1f;
			}

			if (spriteEffects.HasFlag(SpriteEffects.FlipVertically)) {
				zero.Y = (vector.Y - (float)LineSpacing) * scale.Y;
				one.Y = -1f;
			}
		}

		zero.X = x;
		foreach (char c in text) {
			switch (c) {
				case '\n':
					zero.X = x;
					zero.Y += (float)LineSpacing * scale.Y * one.Y;
					flag = true;
					continue;
				case '\r':
					continue;
			}

			SpriteCharacterData characterData = GetCharacterData(c);
			Vector3 kerning = characterData.Kerning;
			Rectangle padding = characterData.Padding;
			if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
				padding.X -= padding.Width;

			if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
				padding.Y = LineSpacing - characterData.Glyph.Height - padding.Y;

			if (flag)
				kerning.X = Math.Max(kerning.X, 0f);
			else
				zero.X += CharacterSpacing * scale.X * one.X;

			zero.X += kerning.X * scale.X * one.X;
			Vector2 position = zero;
			position.X += (float)padding.X * scale.X;
			position.Y += (float)padding.Y * scale.Y;
			Vector2.Transform(ref position, ref matrix, out position);
			position += startPosition;
			spriteBatch.Draw(characterData.Texture, position, characterData.Glyph, color, rotation, Vector2.Zero, scale, spriteEffects, depth);
			zero.X += (kerning.Y + kerning.Z) * scale.X * one.X;
			flag = false;
		}
	}

	private SpriteCharacterData GetCharacterData(char character)
	{
		if (!_spriteCharacters.TryGetValue(character, out var value))
			return _defaultCharacterData;

		return value;
	}

	public Vector2 MeasureString(string text)
	{
		if (text.Length == 0)
			return Vector2.Zero;

		Vector2 zero = Vector2.Zero;
		zero.Y = LineSpacing;
		float val = 0f;
		int num = 0;
		float num2 = 0f;
		bool flag = true;
		foreach (char c in text) {
			switch (c) {
				case '\n':
					val = Math.Max(zero.X + Math.Max(num2, 0f), val);
					num2 = 0f;
					zero = Vector2.Zero;
					zero.Y = LineSpacing;
					flag = true;
					num++;
					continue;
				case '\r':
					continue;
			}

			SpriteCharacterData characterData = GetCharacterData(c);
			Vector3 kerning = characterData.Kerning;
			if (flag)
				kerning.X = Math.Max(kerning.X, 0f);
			else
				zero.X += CharacterSpacing + num2;

			zero.X += kerning.X + kerning.Y;
			num2 = kerning.Z;
			zero.Y = Math.Max(zero.Y, characterData.Padding.Height);
			flag = false;
		}

		zero.X += Math.Max(num2, 0f);
		zero.Y += num * LineSpacing;
		zero.X = Math.Max(zero.X, val);
		return zero;
	}

	public string CreateWrappedText(string text, float maxWidth) => CreateWrappedText(text, maxWidth, Thread.CurrentThread.CurrentCulture);

	public string CreateWrappedText(string text, float maxWidth, CultureInfo culture)
	{
		WrappedTextBuilder wrappedTextBuilder = new WrappedTextBuilder(this, maxWidth, culture);
		wrappedTextBuilder.Append(text);
		return wrappedTextBuilder.ToString();
	}

	public string CreateCroppedText(string text, float maxWidth)
	{
		Vector2 vector = MeasureString(text);
		Vector2 vector2 = MeasureString("…");
		maxWidth -= vector2.X;
		if (maxWidth <= vector2.X)
			return "…";

		if (vector.X > maxWidth) {
			int num = 200;
			while (vector.X > maxWidth && text.Length > 1) {
				num--;
				if (num <= 0)
					break;

				text = text.Substring(0, text.Length - 1);
				if (text.Length == 1) {
					text = "";
					break;
				}

				vector = MeasureString(text);
			}

			text += "…";
		}

		return text;
	}

	public GlyphMetrics GetCharacterMetrics(char character) => GetCharacterData(character).ToGlyphMetric();
}
