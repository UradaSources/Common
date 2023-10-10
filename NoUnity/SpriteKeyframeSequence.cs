using System.Collections;
using System.Collections.Generic;

namespace urd
{
	public class misc
	{

	}

	public struct rect { }

	public interface ISprite
	{
		rect texRect();
	}

	public struct SpriteKeyframe
	{
		public ISprite sprite;

		public int length;
		public int offset;

		public SpriteKeyframe(ISprite sprite, int length = 1)
		{
			this.sprite = sprite;
			this.length = length;

			this.offset = -1;
		}
	}

	[System.Serializable]
	public class SpriteKeyframeSequence
	{
		public static SpriteKeyframeSequence CreateFromeSpriteArray(IEnumerable<ISprite> sp, int sample, int eachFrameLength = 1)
		{
			var sequence = new SpriteKeyframeSequence();

			sequence.sample = sample;
			//sequence.setKeyframeArray(sp);

			for (int i = 0; i < sequence.m_keyframes.Length; i++)
				sequence.m_keyframes[i].length = eachFrameLength;

			sequence.applyKeyframe();

			return sequence;
		}
		public static bool IsNullOrEmpty(SpriteKeyframeSequence sequence)
			=> sequence == null || sequence.isEmpty;

		private SpriteKeyframe[] m_keyframes;

		private int m_sample = 24;

		private int m_totalFrameCount;

		public int sample
		{
			set => m_sample = System.Math.Max(1, value);
			get => m_sample;
		}

		public int totalFrameCount => m_totalFrameCount;

		public int keyframeCount => m_keyframes.Length;

		public float frameDeltaTime => 1.0f / m_sample; // 采样时每帧的增量时间

		public float duration => this.totalFrameCount * this.frameDeltaTime; // 该动画的持续时长

		public bool isEmpty => m_keyframes.Length > 0;

		public SpriteKeyframe[] keyframeArray => m_keyframes;

		public void applyKeyframe()
		{
			m_totalFrameCount = 0;
			for (int i = 0; i < this.keyframeArray.Length; i++)
			{
				this.keyframeArray[i].offset = m_totalFrameCount;
				m_totalFrameCount += this.keyframeArray[i].length;
			}
		}

		public void setKeyframeArray(IEnumerable<SpriteKeyframe> keyframes)
		{
			var tmp = new List<SpriteKeyframe>(keyframes);
			m_keyframes = tmp.ToArray();
		}
		//public void setKeyframeArray(IEnumerable<Sprite> sprites)
		//{
		//	var tmp = new List<SpriteKeyframe>();
		//	foreach (var sp in sprites)
		//		tmp.Add(new Keyframe(sp));

		//	m_keyframes = tmp.ToArray();
		//}

		public SpriteKeyframe getKeyframe(int index)
			=> this.keyframeArray[index];

		public bool tryGetKeyframe(int index, out SpriteKeyframe result)
		{
			if (index >= 0 && index < this.keyframeArray.Length)
			{
				result = this.getKeyframe(index);
				return true;
			}
			result = default;
			return false;
		}
	}
}
