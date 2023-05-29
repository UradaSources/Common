using UnityEngine;

public class RotateTransform : MonoBehaviour
{
	[SerializeField, Tooltip("nullable, fallback to tr when null")]
	private Rigidbody2D m_rb;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_speedScale = 1;

	public float Speed
	{
		get => m_speed;
		set => m_speed = Mathf.Max(value, 0);
	}

	public float SpeedScale
	{
		get => m_speedScale;
		set => m_speedScale = Mathf.Max(value, 0);
	}

	private void Update()
	{
		if (m_rb)
		{
			var angle = m_rb.rotation;

			angle += m_speed * Time.deltaTime;
			angle = MathUtility.Loop(angle, 360.0f);

			m_rb.MoveRotation(angle);
		}
		else
		{
			var angle = this.transform.localEulerAngles;

			angle.z += m_speed * Time.deltaTime;
			angle.z = MathUtility.Loop(angle.z, 360.0f);

			this.transform.localEulerAngles = angle;
		}
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_rb = this.GetComponent<Rigidbody2D>();

		m_speedScale = 1.0f;
	}
#endif
}
