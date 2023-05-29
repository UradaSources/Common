/*urada@foxmail.com 2023/5/29 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 摄像机的投影过渡动画
// 将摄像机从透视视角过渡到正交视角或是反向过渡回透视视角
public class CameraProjectionTransition : MonoBehaviour
{
	// 使用的摄像机
	[SerializeField]
	private Camera m_camera;

	// 动画过程中所要插入的animator, 可空
	// 使用该属性时在animation中用event调用该类的TryTransitionToPersView等函数
	// 在转换过程中m_insertedAnimator会被暂时禁用防止animator锁定transofm
	[SerializeField]
	private Animator m_insertedAnimator;

	// 焦点距离
	// 需要保证对象处在摄像机的焦点上, 否则随着fov增大对象将严重变形
	[SerializeField]
	private float m_focusDist = 0;

	// 过渡时长
	[SerializeField]
	public float m_duration = 0;

	[ContextMenu("TryTransitionToOrthView")]
	public void TryTransitionToOrthView()
		=> this.StartCoroutine(this.ToOrthViewProcess(m_duration, m_focusDist));

	[ContextMenu("TryTransitionToPersView")]
	public void TryTransitionToPersView()
		=> this.StartCoroutine(this.ToPersViewProcess(m_duration, m_focusDist));

	private bool _transitionProcessing = false;

	// 过渡到正交视图
	public IEnumerator ToOrthViewProcess(float duration, float focusDist, float fovTarget = 1)
	{
		if (_transitionProcessing || m_camera.orthographic) yield break;
		_transitionProcessing = true;

		// 暂停动画器的播放, 获取控制权
		if (m_insertedAnimator)
			m_insertedAnimator.enabled = false;

		// 记录原始的fov值
		var fovStart = m_camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = Mathf.Tan(halfFovAngle) * focusDist;

		var camTr = m_camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			m_camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越小, dist将越来越大来保持size不变
			var dist = MathUtility.Cot(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}

		// 在过渡动画完成后将视角切换为正交
		m_camera.orthographic = true;
		m_camera.orthographicSize = size;

		// 恢复动画器
		if (m_insertedAnimator)
			m_insertedAnimator.enabled = true;

		_transitionProcessing = false;
	}

	// 过渡到透视视图
	public IEnumerator ToPersViewProcess(float duration, float focusDist, float fovTarget = 60)
	{
		if (_transitionProcessing || !m_camera.orthographic) yield break;
		_transitionProcessing = true;

		// 暂停动画器的播放, 获取控制权
		if (m_insertedAnimator)
			m_insertedAnimator.enabled = false;

		// 记录原始的fov值
		var fovStart = m_camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = m_camera.orthographicSize;

		Debug.Assert(Mathf.Abs(halfFovAngle - 1.0f) < 10);

		var camTr = m_camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		// 初始化, 将相机移动到足够远的距离后设置为正交视角, 再逐渐拉近到正常位置
		var startDist = MathUtility.Cot(halfFovAngle) * size;

		var startPos = focusPoint - camTr.forward * startDist;
		camTr.position = startPos;

		// 设置回正交视角
		m_camera.orthographic = false;
		
		// 逐渐拉近
		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			m_camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越大, dist将越来越小来保持size不变
			var dist = MathUtility.Cot(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}

		// 恢复动画器
		if (m_insertedAnimator)
			m_insertedAnimator.enabled = true;

		_transitionProcessing = false;
	}

	public void OnDisable()
	{
		Debug.Assert(!_transitionProcessing, "Coroutine animation is forced to end");
		_transitionProcessing = false;
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_camera = this.GetComponent<Camera>();
		m_insertedAnimator = this.GetComponent<Animator>();
	}
	private void OnDrawGizmosSelected()
	{
		if (m_camera)
		{
			var camTr = m_camera.transform;

			var focusPoint = camTr.position + camTr.forward * m_focusDist;
			DebugUtility.DrawLine(camTr.position, focusPoint);
			DebugUtility.DrawMark(focusPoint, new DrawParam(size: 0.1f, color: Color.red));
		}
	}
#endif
}
