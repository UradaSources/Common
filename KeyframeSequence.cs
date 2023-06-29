using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UradaCommon.SpriteFrameAnimation
{
	[System.Serializable]
	public struct Keyframe
	{
		public Sprite sprite;
		public int length;

		public int offset;

		public Keyframe(Sprite sprite, int length = 1)
		{
			this.sprite = sprite;
			this.length = length;

			this.offset = -1;
		}
	}

	[System.Serializable]
	public class KeyframeSequence
	{
		public static implicit operator bool(KeyframeSequence obj) => obj == null;
		
		public static KeyframeSequence CreateFromeSprites(IEnumerable<Sprite> sp, int sample, int eachFrameLength = 1)
		{
			var sequence = new KeyframeSequence();

			sequence.sample = sample;
			sequence.setKeyframeArray(sp);

			for (int i = 0; i < sequence.m_keyframeArray.Length; i++)
				sequence.m_keyframeArray[i].length = eachFrameLength;

			sequence.applyKeyframe();

			return sequence;
		}

		public static bool IsNotNullOrEmpty(KeyframeSequence sequence)
			=> sequence != null && sequence.isNotEmpty;

		[SerializeField]
		private Keyframe[] m_keyframeArray;

		[SerializeField]
		private int m_sample = 24;

		[SerializeField, HideInInspector]
		private int m_totalFrameCount;

		public int sample
		{
			set => m_sample = System.Math.Max(1, value);
			get => m_sample;
		}

		public int totalFrameCount => m_totalFrameCount;

		public int keyframeCount => m_keyframeArray.Length;

		// 采样时每帧的增量时间
		public float frameDeltaTime => 1.0f / this.m_sample;

		// 该动画的持续时长
		public float duration => this.totalFrameCount * this.frameDeltaTime;

		public bool isNotEmpty => m_keyframeArray.Length > 0;

		public Keyframe[] keyframeArray => m_keyframeArray;

		public void applyKeyframe()
		{
			m_totalFrameCount = 0;
			for (int i = 0; i < this.keyframeArray.Length; i++)
			{
				this.keyframeArray[i].offset = m_totalFrameCount;
				m_totalFrameCount += this.keyframeArray[i].length;
			}
		}

		public void setKeyframeArray(IEnumerable<Keyframe> keyframes)
		{
			var tmp = new List<Keyframe>(keyframes);
			m_keyframeArray = tmp.ToArray();
		}
		public void setKeyframeArray(IEnumerable<Sprite> sprites)
		{
			var tmp = new List<Keyframe>();
			foreach (var sp in sprites)
				tmp.Add(new Keyframe(sp));

			m_keyframeArray = tmp.ToArray();
		}

		public Keyframe getKeyframe(int index)
			=> this.keyframeArray[index];

		public bool tryGetKeyframe(int index, out Keyframe result)
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
