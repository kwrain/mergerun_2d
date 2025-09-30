using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Yoyo.UI
{
	[AddComponentMenu("UI/Yoyo/RichText", 20)]
	public class RichText : Text, ISerializationCallbackReceiver
	{
		[SerializeField]
		[TextArea(3, 10)]
		private string m_Content;

		[SerializeField, HideInInspector]
		private List<TextEffect> m_UsedEffects = new List<TextEffect>();

		[SerializeField, HideInInspector]
		private List<TextEffect> m_UnusedEffects = new List<TextEffect>();

		[Serializable]
		public class SpriteGroup
		{
			public string name;
			public Sprite[] sprites;
		}

		[SerializeField]
		private List<SpriteGroup> m_SpriteGroups = new List<SpriteGroup>();

		private Dictionary<string, SpriteGroup> m_SpriteGroupDict = new Dictionary<string, SpriteGroup>();

		[SerializeField, FormerlySerializedAs("onLink")]
		private TextLinkEvent m_OnLink = new TextLinkEvent();

		public TextLinkEvent onLink
		{
			get
			{
				return m_OnLink;
			}
			set
			{
				m_OnLink = value;
			}
		}

		public override string text
		{
			get
			{
				return m_GeometryUpdating ? m_Text : m_Content;
			}

			set
			{
				if (string.IsNullOrEmpty(value)) {
					if (!string.IsNullOrEmpty(m_Content)) {
						m_Content = string.Empty;
						SetVerticesDirty();
						UpdateText();
					}
				} else if (m_Content != value) {
					m_Content = value;
					SetVerticesDirty();
					SetLayoutDirty();
					UpdateText();
				}
			}
		}

		public SpriteGroup GetSpriteGroup(string name)
		{
			SpriteGroup group = null;
			m_SpriteGroupDict.TryGetValue(name, out group);
			return group;
		}
		public void SetSpriteGroup(SpriteGroup group)
		{
			m_SpriteGroups.Add(group);
			m_SpriteGroupDict[group.name] = group;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			m_SpriteGroupDict.Clear();
			foreach (var group in m_SpriteGroups) {
				m_SpriteGroupDict[group.name] = group;
			}
		}

		private bool m_GeometryUpdating;

		protected override void UpdateGeometry()
		{
			m_GeometryUpdating = true;
			base.UpdateGeometry();
			m_GeometryUpdating = false;
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			base.OnPopulateMesh(toFill);
			if (font != null) {
				m_UsedEffects.Sort();
				foreach (var effect in m_UsedEffects) {
					if (effect != null) {
						effect.ModifyMesh(toFill);
					}
				}
			}
		}

		private void AddEffect(Type type, int begin, int end, string param, int priority)
		{
			TextEffect com = null;
			for (int i = m_UnusedEffects.Count - 1; i >= 0; i--) {
				var effect = m_UnusedEffects[i];
				if (effect != null && effect.GetType() == type) {
					effect.enabled = true;
					com = effect;
					m_UnusedEffects.RemoveAt(i);
					break;
				}
			}
			if (com == null) {
				com = (TextEffect)gameObject.AddComponent(type);
			}
			com.Set(begin, end, param, priority);
			m_UsedEffects.Add(com);
		}

		private void UpdateText()
		{
			foreach (var effect in m_UsedEffects) {
				if (effect != null) {
					m_UnusedEffects.Add(effect);
				}
			}
			m_UsedEffects.Clear();
			if (string.IsNullOrEmpty(m_Content)) {
				m_Text = string.Empty;
			} else {
				m_Text = TagParser.ParseText(this, m_Content);
			}
			foreach (var effect in m_UnusedEffects) {
				if (effect != null && effect.enabled) {
					effect.enabled = false;
				}
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			UpdateText();
			base.OnValidate();
		}
#endif

		[Serializable]
		public class TextLinkEvent : UnityEvent<string>
		{
		}

		/// <summary>
		/// Custom Tag Parser
		/// </summary>
		static class TagParser
		{
			class Tag
			{
				public string name;
				public Type type;
				public bool strict;
				public bool enclosed;
				public string replacement;
				public bool inbuilt
				{
					get
					{
						return type == null;
					}
				}
				public Tag(string name, bool strict, bool enclosed = true, Type type = null, string replacement = "")
				{
					this.name = name;
					this.type = type;
					this.strict = strict;
					this.enclosed = enclosed;
					this.replacement = replacement;
				}
			}

			struct Element
			{
				public Tag tag;
				public int begin;
				public int end;
				public string param;
			}

			static Dictionary<char, Tag[]> m_TagDefines = new Dictionary<char, Tag[]>() {
				{ 'a', new Tag[] { new Tag("a", true, true, typeof(Link)) } },
				{ 'b', new Tag[] { new Tag("b", false) } },
				{ 'c', new Tag[] { new Tag("color", true), new Tag("c", true, true, typeof(Colour)) } },
				{ 'g', new Tag[] { new Tag("g", true, true, typeof(Gradient)) } },
				{ 'i', new Tag[] { new Tag("img", true, false, typeof(Emoji), "<quad />"), new Tag("i", false) } },
				{ 'm', new Tag[] { new Tag("material", true) } },
				{ 'q', new Tag[] { new Tag("quad", true, false) } },
				{ 's', new Tag[] { new Tag("size", true), new Tag("s", true, true, typeof(Strike)) } },
				{ 'u', new Tag[] { new Tag("u", true, true, typeof(Underline)) } },
				{ 'w', new Tag[] { new Tag("w", true, true, typeof(Wobbly)) } },
			};

			static private List<Element> m_TagList = new List<Element>();
			static private List<Element> m_TagIndexes = new List<Element>();
			static private Stack<Element> m_TagStack = new Stack<Element>();

			static public string ParseText(RichText richText, string text)
			{
				m_TagList.Clear();
				m_TagStack.Clear();
				m_TagIndexes.Clear();

				Tag tag;
				Tag[] tags;
				var e = false;
				var a = text.ToCharArray();
				int i = 0, p = 0, k = 0, l = a.Length;
				while (p < l) {
					var c = a[p++];
					if (c != '<') {
						i++;
						continue;
					}

					if (p >= l)
						break;

					k = p - 1;
					c = a[p];
					if (m_TagDefines.TryGetValue(c, out tags)) {
						string param;
						if (CompleteTag(tags, a, ref p, out param, out tag)) {
							var element = new Element() { tag = tag, begin = i, param = param };
							if (tag.enclosed) {
								m_TagStack.Push(element);
							}
							if (!tag.inbuilt) {
								var rep = tag.replacement;
								if (!string.IsNullOrEmpty(rep)) {
									i += rep.Length;
								}
								m_TagIndexes.Add(new Element() { begin = k, end = p, param = rep });
								if (!tag.enclosed && element.begin < i) {
									element.end = i - 1;
									m_TagList.Add(element);
								}
								continue;
							}
						}
					} else if (c == '/') {
						if (++p >= l)
							break;

						c = a[p];
						if (m_TagDefines.TryGetValue(c, out tags) && CompareTag(tags, a, ref p, out tag) && a[p] == '>') {
							p++;
							if (m_TagStack.Count == 0 || m_TagStack.Peek().tag != tag) {
								e = true;
								break;
							}
							var t = m_TagStack.Pop();
							if (!tag.inbuilt) {
								m_TagIndexes.Add(new Element() { begin = k, end = p });
								if (t.begin < i) {
									t.end = i - 1;
									m_TagList.Add(t);
								}
								continue;
							}
						}
					}
					i += p - k;
				}

				if (richText.supportRichText = (!e && m_TagStack.Count == 0)) {
					if (m_TagIndexes.Count > 0) {
						int begin = 0;
						StringBuilder builder = new StringBuilder();
						for (int m = 0; m < m_TagIndexes.Count; m++) {
							var v = m_TagIndexes[m];
							if (begin != v.begin) {
								builder.Append(a, begin, v.begin - begin);
							}
							if (!string.IsNullOrEmpty(v.param)) {
								builder.Append(v.param);
							}
							begin = v.end;
						}
						if (begin < l) {
							builder.Append(a, begin, l - begin);
						}
						text = builder.ToString();
					}

					// AddEffects
					for (var m = m_TagList.Count - 1; m >= 0; m--) {
						var element = m_TagList[m];
						richText.AddEffect(element.tag.type, element.begin, element.end, element.param, m);
					}
				}

				return text;
			}

			private static bool CompareTag(Tag[] tags, char[] a, ref int p, out Tag tag)
			{
				tag = null;
				foreach (var value in tags) {
					var name = value.name;
					var len = name.Length;

					if (a.Length - p > len) {
						bool matched = true;
						for (int i = 1; i < len; i++) {
							if (a[p + i] != name[i]) {
								matched = false;
								break;
							}
						}
						if (matched) {
							p += len;
							tag = value;
							return true;
						}
					}
				}
				return false;
			}

			private static bool CompleteTag(Tag[] tags, char[] a, ref int p, out string param, out Tag tag)
			{
				param = string.Empty;
				if (CompareTag(tags, a, ref p, out tag)) {
					var c = a[p];
					if (c == '>') {
						p++;
						return true;
					} else if (c == '=') { // '=' or '>'
						p++;
						if (!tag.strict)
							return true;
						var k = p;
						var l = a.Length;
						while (p < l) {
							if (a[p++] == '>') {
								param = new string(a, k, p - k - 1);
								return true;
							}
						}
					}
				}

				return false;
			}
		}
	}
}
