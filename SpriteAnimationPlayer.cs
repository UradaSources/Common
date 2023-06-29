using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UradaCommon.SpriteFrameAnimation
{ 
	// 关键帧动画的运行上下文
	[System.Serializable]
	public class KeyframeAnimationContext
	{
		[SerializeField]
		private bool _loop = true;

		[SerializeField]
		private float _speedScale = 1;

		// 当前关键帧索引
		private int m_keyframeIndex;

		// 当前关键帧的帧计数器
		private int m_frameId;

		// 累积2帧之间的时间误差
		private float m_cumulativeDeltaTime;

		private KeyframeSequence m_sequence;

		public KeyframeSequence sequence
		{
			set
			{
				m_sequence = value;

				// 清空帧计数器并归零误差值
				if (m_sequence != null)
				{
					m_frameId = 0;
					m_cumulativeDeltaTime = 0;
				}
			}
			get => m_sequence;
		}

		public bool loop
		{
			set => _loop = value;
			get => _loop;
		}
		public float speedScale
		{
			set => _speedScale = value;
			get => _speedScale;
		}

		public int curKeyframeIndex => m_keyframeIndex;

		public int frameId => m_frameId;

		public bool inLastFrame
		{
			get
			{
				int dir = (int)Mathf.Sign(this.speedScale);
				int lastIndex = dir > 0 ? m_sequence.keyframeCount - 1 : 0;

				return m_keyframeIndex == lastIndex;
			}
		}

		public bool isFinished
		{
			get
			{
				// 检查当前帧是否完毕
				var frameFinished = m_cumulativeDeltaTime > m_sequence.frameDeltaTime;
				return this.isFinished && frameFinished;
			}
		}

		public Keyframe? curKeyframe
		{
			get
			{
				if(this.sequence && this.sequence.tryGetKeyframe(m_keyframeIndex, out var kf))
					return kf;
				return null;
			}
		}

		public int calculatesFrameCount()
		{
			int count = this.frameId;
			for (int i = 0; i < this.curKeyframeIndex; i++)
			{
				this.sequence.tryGetKeyframe(i, out var kf);
				count += kf.length;
			}
			return count;
		}

		// 设置当前关键帧
		public bool trySetCurKeyform(int index)
		{
			if (m_sequence.tryGetKeyframe(index, out _))
			{
				m_keyframeIndex = index;

				// 清空帧计数器并重新设置时间戳
				m_frameId = 0;
				m_cumulativeDeltaTime = 0;

				return true;
			}
			return false;
		}

		// 在完成时返回true
		public void UpdateFrame(float deltaTime)
		{
			if (KeyframeSequence.IsNotNullOrEmpty(m_sequence))
			{
				// 一帧最小持续的时间
				float minDt = m_sequence.frameDeltaTime;

				// 用abs避免倒放时timer为负数
				if (Mathf.Abs(m_cumulativeDeltaTime) >= minDt)
				{
					// 计算递进帧数
					// 此处应该向0取整
					int skip = Mathf.RoundToInt(m_cumulativeDeltaTime / minDt);

					// 递进帧数
					m_frameId += Mathf.Abs(skip);

					// 递进方向
					int dir = (int)Mathf.Sign(skip);

					// 剔除计时器中完整的帧时间
					m_cumulativeDeltaTime -= skip * minDt;

					// 检查帧数是否超过当前的关键帧长度
					var kf = this.curKeyframe;
					if (!this.curKeyframe.HasValue)
					{
						Debug.LogError("Invalid keyframe, should be applied after modifying the keyframe");

						this.trySetCurKeyform(0);
						return;
					}

					var curKf = kf.Value;
					while (m_frameId >= curKf.length)
					{
						int last_index = dir > 0 ? m_sequence.keyframeCount - 1 : 0;
						if (m_keyframeIndex == last_index && !_loop)
							break; // 播放完毕
						else
						{
							// 剔除关键帧残留的帧数
							m_frameId -= curKf.length;
							// 递进关键帧索引
							m_keyframeIndex = MathUtils.LoopIndex(m_keyframeIndex, m_sequence.keyframeCount, dir);
						}
					}
				}

				// 更新计时器
				m_cumulativeDeltaTime += this.speedScale * deltaTime;
			}
		}
	}

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
		private KeyframeAnimationContext m_frameUpadter; // 当前动画

		[SerializeField]
		private bool m_playOnAwake = true;

		private PlayState _state = PlayState.Prepare;

		public KeyframeAnimationContext frameUpadter => m_frameUpadter;

		public PlayState State
		{
			private set
			{
				if (_state == value) return;

				var orig = _state;
				_state = value;

				this.onStartChanged?.Invoke(this, orig);
			}
			get => _state;
		}

		public SpriteRenderer spriteRenderer => m_renderer;

		public bool isFinished
		{
			get => this.State == PlayState.Finish;
		}

		public bool lastFrame
		{
			get
			{
				var sequence = m_frameUpadter.sequence;
				if (sequence == null) return false;

				if (sequence.tryGetKeyframe(sequence.keyframeCount - 1, out var last))
				{
					return == m_frameUpadter.KeyframeCount - 1 && m_frameCounter == last.length - 1;
				}
				return false;
			}
		}

		// 清理并重置运行时状态
		[ContextMenu("Editor Runtime Test/Prepare")]
		public void Prepare()
		{
			// 设置当前所使用的精灵
			

			if (m_frameUpadter != null && m_frameUpadter.sequence.tryGetKeyframe(0, out var kf))
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

		private void Awake()
		{
			this.Prepare();

			if (m_playOnAwake)
				this.Play();
		}

		private void Update()
		{
			if (this.State == PlayState.Play)
			{
				this.UpdateFrame(Time.deltaTime);
			}
		}

#if UNITY_EDITOR
		public void CreateAnimFromSelectedSprites()
		{
			var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Create", "new SpriteAnimation", "asset", "");
			if (!string.IsNullOrEmpty(path))
			{
				var selectedSprites = MiscUtils.GetSelectedObjectByOrder<Sprite>();
				var asset = ScriptableObject.CreateInstance<KeyframeSequence>();

				asset.SetKeyframe(selectedSprites.Process((Sprite sp) => new KeyframeSequence.Keyframe(sp)));

				UnityEditor.AssetDatabase.CreateAsset(asset, path);
				UnityEditor.AssetDatabase.SaveAssets();
			}
		}

		private void Reset()
		{
			m_renderer = this.GetComponent<SpriteRenderer>();
		}

		private void OnValidate()
		{
			if (m_renderer == null) return;

			if (m_frameUpadter != null && m_frameUpadter.TryGetKeyframe(0, out var kf))
				m_renderer.sprite = kf.sprite;
			else
				m_renderer.sprite = null;
		}
#endif
	}

}


