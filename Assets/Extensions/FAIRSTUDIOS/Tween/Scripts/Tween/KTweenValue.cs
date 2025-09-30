namespace FAIRSTUDIOS.Tools
{
  public class KTweenValue : KTweener
  {
		public float from;
		public float to;

		float mValue;

    public virtual float value
    {
			get { return mValue;}
			set { mValue = value; }
		}

		virtual protected void ValueUpdate(float value, bool isFinished) { }

		protected override void OnUpdate (float factor, bool isFinished)
    {
			value = from + factor * (to - from);
			ValueUpdate(value, isFinished);		
		}
	}
}
