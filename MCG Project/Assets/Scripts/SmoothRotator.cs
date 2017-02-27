using System;
using UnityEngine;

public class SmoothRotator
{
	public SmoothRotator ()
	{
	}

	public static Quaternion Rotate( Quaternion localRotation, ref Quaternion m_OriginalRotation,
		ref Vector3 m_TargetAngles,
		ref Vector3 m_FollowAngles, ref Vector3 m_FollowVelocity,
		Vector3 rotationRange, float rotationSpeed,
		float dampingTime, float inputH, float inputV)
	{
		// we make initial calculations from the original local rotation
		localRotation = m_OriginalRotation;


		// wrap values to avoid springing quickly the wrong way from positive to negative
		if (m_TargetAngles.y > 180)
		{
			m_TargetAngles.y -= 360;
			m_FollowAngles.y -= 360;
		}
		if (m_TargetAngles.x > 180)
		{
			m_TargetAngles.x -= 360;
			m_FollowAngles.x -= 360;
		}
		if (m_TargetAngles.z > 180)
		{
			m_TargetAngles.z -= 360;
			m_FollowAngles.z -= 360;
		}
		if (m_TargetAngles.y < -180)
		{
			m_TargetAngles.y += 360;
			m_FollowAngles.y += 360;
		}
		if (m_TargetAngles.x < -180)
		{
			m_TargetAngles.x += 360;
			m_FollowAngles.x += 360;
		}
		if (m_TargetAngles.z < -180)
		{
			m_TargetAngles.z += 360;
			m_FollowAngles.z += 360;
		}

		m_TargetAngles.y += inputH*rotationSpeed * Time.deltaTime;
		m_TargetAngles.x += inputV* rotationSpeed * Time.deltaTime;
		m_TargetAngles.z += inputH* rotationSpeed * Time.deltaTime;

		if (rotationRange.y < 360) {
			m_TargetAngles.y = Mathf.Clamp (m_TargetAngles.y, -rotationRange.y * 0.5f, rotationRange.y * 0.5f);
		}
		if (rotationRange.x < 360) {
			m_TargetAngles.x = Mathf.Clamp (m_TargetAngles.x, -rotationRange.x * 0.5f, rotationRange.x * 0.5f);
		}
		if (rotationRange.z < 360) {
			m_TargetAngles.z = Mathf.Clamp (m_TargetAngles.z, -rotationRange.z * 0.5f, rotationRange.z * 0.5f);
		}

				

		// smoothly interpolate current values to target angles
		m_FollowAngles = Vector3.SmoothDamp(m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);

			// update the actual gameobject's rotation
		localRotation = m_OriginalRotation*Quaternion.Euler(-m_FollowAngles.x, m_FollowAngles.y, m_FollowAngles.z);


		return localRotation;
	}

}


