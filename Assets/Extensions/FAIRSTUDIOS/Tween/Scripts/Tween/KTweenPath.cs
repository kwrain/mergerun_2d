using UnityEngine;
using System.Collections.Generic;

namespace FAIRSTUDIOS.Tools
{
  public class KTweenPath : KTweenValue {

		public Transform target;
		public List<Vector3> paths;

		int mIndex = -1;
		int mPathsCount = 0;
		bool mCache = false;
    public bool mPause = false;

		void Cache () {
      if (null == paths || paths.Count == 0)
        return;

			mCache = true;
			if (paths.Count > 1) {
				mPathsCount = paths.Count - 1;
			}
			if (target == null) {
        target = GetComponent<Transform>();
			}
			from = 0;
			to = mPathsCount;

      mPause = false;
		}
	
		protected override void ValueUpdate (float _factor, bool _isFinished)
		{

			if (!mCache) { Cache();}
			pathIndex = Mathf.FloorToInt(_factor);
		}

    public void SetPath(List<Vector3> ltPath)
    {
      if (null == ltPath || ltPath.Count == 0)
        return;

      paths = ltPath;
    }


    public void Stop()
    {

    }


		int pathIndex {
			get { return mIndex;}
			set {
				if (mIndex != value) {
					mIndex = value;
          //Debug.Log(mIndex);
          //Debug.Log(paths[mIndex]);
					KTweenPosition.Begin(target.gameObject, target.localPosition, paths[mIndex], duration/paths.Count).loopStyle = LoopStyle.Once;
				}
			}
		}

	}
}
