using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yoyo.UI
{
	public class Emoji : TextEffect
	{
		static private Dictionary<Emoji, float> s_RebuildQueue;

		static Emoji()
		{
			s_RebuildQueue = new Dictionary<Emoji, float>();
			Canvas.willRenderCanvases += () => {
				foreach (var entry in s_RebuildQueue) {
					entry.Key.Rebuild(entry.Value);
				}
				s_RebuildQueue.Clear();
			};
		}

		[SerializeField]
		private string m_SpriteGroupName;
		[SerializeField]
		private int m_SpriteBeginIndex;
		[SerializeField]
		private int m_SpriteEndIndex;
		[SerializeField]
		private float m_SpriteDuration;

		[SerializeField]
		private Emoticon m_Icon;

		private int m_SpriteCurrentIndex;
		private float m_SpriteDeltaTime;

		protected Emoji()
		{
		}

		protected override int priority
		{
			get
			{
				return base.priority;
			}

			set
			{
				base.priority = value + 50000;
			}
		}

		protected override void OnParameterRebuild()
		{
			if (parameter.IndexOf(",") >= 0) {
				var a = parameter.Split(',');
				m_SpriteGroupName = a[0];
				m_SpriteBeginIndex = ParseInt(a[1], 0);
				m_SpriteEndIndex = a.Length > 2 ? ParseInt(a[2], 0) : m_SpriteBeginIndex;
				m_SpriteDuration = a.Length > 3 ? ParseFloat(a[3], 0f) : 0f;
			} else {
				m_SpriteGroupName = parameter;
				m_SpriteBeginIndex = 0;
				m_SpriteEndIndex = 0;
				m_SpriteDuration = 0f;
			}
			m_SpriteDeltaTime = 0f;
			m_SpriteCurrentIndex = m_SpriteBeginIndex;
		}

		private void CreateEmoticon()
		{
			var go = new GameObject("emoticon");
			go.layer = gameObject.layer;
			go.SetActive(false);
			var t = go.transform;
			t.SetParent(transform);
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
			t.localPosition = Vector3.zero;
			m_Icon = go.AddComponent<Emoticon>();
			m_Icon.rectTransform.pivot = Vector2.zero;
			m_Icon.raycastTarget = false;
		}

		protected override void OnEnable()
		{
			if (m_Icon == null) {
				CreateEmoticon();
			}
#if YOYO_DEBUG
			m_Icon.gameObject.hideFlags = HideFlags.None;
#else
			m_Icon.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			s_RebuildQueue.Remove(this);
			if (m_Icon != null) {
				m_Icon.gameObject.SetActive(false);
			}
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			if (m_Icon != null) {
				if (Application.isPlaying) {
					GameObject.Destroy(m_Icon);
				} else {
					GameObject.DestroyImmediate(m_Icon);
				}
				m_Icon = null;
			}
			base.OnDestroy();
		}

		protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
		{
			Vector3 min, max;
			byte topLeft, topRight, bottomLeft, bottomRight;

			var k = startCharIdx * 4;
			UIVertex vertex = UIVertex.simpleVert;

			vh.PopulateUIVertex(ref vertex, k);
			topLeft = vertex.color.a;
			vertex.uv0 = Vector2.zero;
			vh.SetUIVertex(vertex, k);

			vh.PopulateUIVertex(ref vertex, k + 1);
			topRight = vertex.color.a;
			vertex.uv0 = Vector2.zero;
			vh.SetUIVertex(vertex, k + 1);

			vh.PopulateUIVertex(ref vertex, k + 2);
			max = vertex.position;
			bottomRight = vertex.color.a;
			vertex.uv0 = Vector2.zero;
			vh.SetUIVertex(vertex, k + 2);

			vh.PopulateUIVertex(ref vertex, k + 3);
			min = vertex.position;
			bottomLeft = vertex.color.a;
			vertex.uv0 = Vector2.zero;
			vh.SetUIVertex(vertex, k + 3);

			if (m_Icon != null) {
				m_Icon.rectTransform.localPosition = new Vector2(min.x, min.y - 2);
				m_Icon.SetColorAlphas(topLeft, topRight, bottomLeft, bottomRight);
			}
			s_RebuildQueue[this] = max.x - min.x;
		}

		private void Rebuild(float size)
		{
			if (m_Icon != null) {
				ShowSpriteIndex(m_SpriteCurrentIndex);
				m_Icon.rectTransform.sizeDelta = new Vector2(size, size);
				var go = m_Icon.gameObject;
				if (!go.activeSelf) {
					m_Icon.gameObject.SetActive(true);
				}
			}
		}

		private void ShowSpriteIndex(int index)
		{
			var group = richText.GetSpriteGroup(m_SpriteGroupName);
			if (group != null) {
				var sprites = group.sprites;
				if (sprites.Length > index) {
					m_Icon.sprite = sprites[index];
				}
			}
		}

		void Update()
		{
			if (m_SpriteBeginIndex < m_SpriteEndIndex && m_Icon != null) {
				m_SpriteDeltaTime += Time.deltaTime;
				if (m_SpriteDeltaTime >= m_SpriteDuration) {
					m_SpriteDeltaTime -= m_SpriteDuration;
					++m_SpriteCurrentIndex;
					if (m_SpriteCurrentIndex < m_SpriteBeginIndex || m_SpriteCurrentIndex > m_SpriteEndIndex) {
						m_SpriteCurrentIndex = m_SpriteBeginIndex;
					}
					if (!CanvasUpdateRegistry.IsRebuildingGraphics()) {
						ShowSpriteIndex(m_SpriteCurrentIndex);
					}
				}
			}
		}
	}
}
