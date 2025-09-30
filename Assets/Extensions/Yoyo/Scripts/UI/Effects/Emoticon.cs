using UnityEngine;
using UnityEngine.UI;

namespace Yoyo.UI
{
	public class Emoticon : Image
	{
		[SerializeField]
		private byte m_TopLeft;
		[SerializeField]
		private byte m_TopRight;
		[SerializeField]
		private byte m_BottomLeft;
		[SerializeField]
		private byte m_BottomRight;

		public void SetColorAlphas(byte topLeft, byte topRight, byte bottomLeft, byte bottomRight)
		{
			m_TopLeft = topLeft;
			m_TopRight = topRight;
			m_BottomLeft = bottomLeft;
			m_BottomRight = bottomRight;
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			base.OnPopulateMesh(toFill);

			Emoji.SetUIVertexColorAlpha(toFill, 0, m_BottomLeft);
			Emoji.SetUIVertexColorAlpha(toFill, 1, m_TopLeft);
			Emoji.SetUIVertexColorAlpha(toFill, 2, m_TopRight);
			Emoji.SetUIVertexColorAlpha(toFill, 3, m_BottomRight);
		}
	}
}