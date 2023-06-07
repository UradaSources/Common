using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer)), DisallowMultipleComponent]
public class SpriteAnimationPlayer : MonoBehaviour
{
	public enum PlayState
	{
		Prepare,
		Play,
		Pause,
		Finish
	}

	public class StateChangedEvent : UnityEvent<SpriteAnimationPlayer, PlayState> { };
	public StateChangedEvent onStartChanged = new StateChangedEvent();

	// 所使用的的精灵渲染器
	[SerializeField]
	private SpriteRenderer m_renderer;

	[SerializeField]
	private SpriteAnimation m_anim; // 当前动画

	[SerializeField]
	private bool m_playOnAwake = true;

	[SerializeField]
	private bool m_loop = true; // 是否循环播放

	[SerializeField]
	private float m_speed = 1; // 播放速度

	private int m_curKeyframeIndex; // 当前关键帧索引
	private int m_frameCounter; // 当前关键帧的帧计数器

	private float m_timer; // 2个关键帧之间的计时器

	private PlayState _state = PlayState.Prepare;

	public SpriteAnimation Anim
	{
		set => m_anim = value;
		get => m_anim;
	}

	public bool Loop
	{
		set => m_loop = value;
		get => m_loop;
	}

	public float Speed
	{
		set => m_speed = value;
		get => m_speed;
	}

	public PlayState State
	{
		set
		{
			if (_state == value) return;

			var orig = _state;
			_state = value;

			this.onStartChanged?.Invoke(this, orig);
		}
		get => _state;
	}

	public SpriteRenderer Renderer 
	{ 
		get => m_renderer; 
	}

	public bool IsFinished 
	{
		get => this.State == PlayState.Finish; 
	}

	public int CurKeyframeIndex 
	{
		get => m_curKeyframeIndex;
	} 
	public int CurFrameCount 
	{
		get => m_frameCounter;
	}

	public SpriteAnimation.Keyframe? CurKeyframe
	{
		get
		{
			if (m_anim != null && m_anim.TryGetKeyframe(m_curKeyframeIndex, out var kf))
				return kf;
			return null;
		}
	}

	// 设置当前关键帧
	public bool TrySetCurKeyform(int index)
	{
		if (m_anim && m_anim.TryGetKeyframe(index, out _))
		{
			m_curKeyframeIndex = index;

			// 清空帧计数器并重新设置时间戳
			m_frameCounter = 0;
			m_timer = 0;

			return true;
		}
		return false;
	}

	// 清理并重置运行时状态
	[ContextMenu("Editor Runtime Test/Prepare")]
	public void Prepare()
	{
		// 重置运行时状态
		m_curKeyframeIndex = 0;
		m_frameCounter = 0;

		m_timer = 0;

		// 设置当前所使用的精灵
		if (m_anim != null && m_anim.TryGetKeyframe(0, out var kf))
			m_renderer.sprite = kf.sprite;

		this.State = PlayState.Prepare;
	}

	[ContextMenu("Editor Runtime Test/Play")]
	public void Play()
	{
		if (this.State == PlayState.Pause || this.State == PlayState.Prepare)
			this.State = PlayState.Play;
	}

	[ContextMenu("Editor Runtime Test/Pause")]
	public void Pause()
	{
		if (this.State == PlayState.Play || this.State == PlayState.Prepare)
			this.State = PlayState.Pause;
	}

	private void UpdateFrame(float dt)
	{
		// 一帧最小持续的时间
		float interval = m_anim.FrameDeltaTime;

		// 用abs避免倒放时timer为负数
		if (Mathf.Abs(m_timer) >= interval)
		{
			// 计算递进帧数
			int skip = Mathf.RoundToInt(m_timer / interval);

			// 递进帧数
			m_frameCounter += Mathf.Abs(skip);

			// 递进方向
			int dir = (int)Mathf.Sign(skip);

			// 剔除计时器中完整的帧时间
			m_timer -= skip * interval;

			// 检查帧数是否超过当前的关键帧长度
			var keyframe = this.CurKeyframe.Value;
			while (m_frameCounter >= keyframe.length)
			{
				int last_index = dir > 0 ? m_anim.KeyframeCount - 1 : 0;
				if (m_curKeyframeIndex == last_index && !m_loop)
				{
					// 停止播放
					this.State = PlayState.Finish;

					break;
				}
				else
				{ 
					// 剔除关键帧残留的帧数
					m_frameCounter -= keyframe.length;
					// 递进关键帧
					m_curKeyframeIndex = MathUtility.LoopIndex(m_curKeyframeIndex, m_anim.KeyframeCount, dir);
				}
			}

			// 更新当前关键帧对应的精灵
			m_anim.TryGetKeyframe(m_curKeyframeIndex, out var kf);
			m_renderer.sprite = kf.sprite;
		}

		// 更新计时器
		m_timer += this.Speed * dt;
	}

	private void Awake()
	{
		this.Prepare();

		if (m_playOnAwake)
			this.Play();
	}

	private void Update()
	{
		if (this.State == PlayState.Play && m_anim != null && m_anim.TryGetKeyframe(0, out _))
		{
			this.UpdateFrame(Time.deltaTime);
		}
	}

#if UNITY_EDITOR
	public void CreateAnimFromSelectedSprites()
	{
		var sps = MiscUtils.GetSelectedObjectByOrder<Sprite>();
		var asset = SpriteAnimation.CreateFromeSprites(sps, 24);

		var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Create", "new sequence", "asset", "");
		if (!string.IsNullOrEmpty(path))
		{
			
		}
	}

	private void Reset()
	{
		m_renderer = this.GetComponent<SpriteRenderer>();
	}

	private void OnValidate()
	{
		if (m_anim != null && m_anim.TryGetKeyframe(0, out var kf))
			m_renderer.sprite = kf.sprite;
		else
			m_renderer.sprite = null;
	}
#endif
}
