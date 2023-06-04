using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new KeyframeSequence", menuName = "KeyframeSequence")]
public class KeyframeSequence : ScriptableObject
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

	public static KeyframeSequence CreateFromeSprites(IEnumerable<Sprite> sp, int sample, int unitFrameLength = 1)
	{ 
		var asset = ScriptableObject.CreateInstance<KeyframeSequence>();
		asset.Sample = sample;
		asset.SetKeyframe(sp.Process((Sprite sp)=>new Keyframe(sp, unitFrameLength)));

		return asset;
	}

	public static implicit operator bool(KeyframeSequence obj) => obj == null;

	[SerializeField] 
	private Keyframe[] m_keyframes;

	[SerializeField]
	private int m_sample = 24;

	[SerializeField, HideInInspector]
	private int m_totalFrameCount;

	public Keyframe[] Keyframes
	{
		set
		{
			m_keyframes = value;
			this.ApplyKeyframe();
		}
		get => m_keyframes;
	}
	public int Sample
	{ 
		set => m_sample = System.Math.Max(1, value);
		get => m_sample;
	}

	public int TotalFrameCount
	{
		get => m_totalFrameCount;
	}
	public int KeyframeCount 
	{ 
		get => m_keyframes.Length; 
	}
	
	public float FrameDeltaTime => 1.0f / this.m_sample; // 采样时每帧的增量时间

	public float Duration => this.TotalFrameCount * this.FrameDeltaTime; // 该动画的持续时长

	public void ApplyKeyframe()
	{
		m_totalFrameCount = 0;
		for (int i = 0; i < this.Keyframes.Length; i++)
		{
			this.Keyframes[i].offset = m_totalFrameCount;
			m_totalFrameCount += this.Keyframes[i].length;
		}
	}

	public void SetKeyframe(IEnumerable<Keyframe> keyframes)
	{
		var tmp = new List<Keyframe>(keyframes);
		m_keyframes = tmp.ToArray();

		this.ApplyKeyframe();
	}
	public bool TryGetKeyframe(int index, out Keyframe result)
	{ 
		if (index >= 0 && index < this.Keyframes.Length)
		{
			result = this.Keyframes[index];
			return true;
		}

		result = default;
		return false;
	}

	[ContextMenu("as")]
	private void foo()
	{ 
	
	}
}
