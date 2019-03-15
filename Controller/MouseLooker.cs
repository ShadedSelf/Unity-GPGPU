using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System;

[Serializable]
public class MouseLooker
{
	public float XSensitivity = 2f;
	public float YSensitivity = 2f;

	private Quaternion m_CharacterTargetRot;
	private Quaternion m_CameraTargetRot;

	public void Init(Transform character, Transform camera)
	{
		m_CharacterTargetRot = character.localRotation;
		m_CameraTargetRot = camera.localRotation;
	}


	public void LookRotation(Transform character, Transform camera)
	{
		float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity * Time.deltaTime;
		float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity * Time.deltaTime;

		m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
		m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);


		character.localRotation = m_CharacterTargetRot;
		camera.localRotation = m_CameraTargetRot;

		Lock();
	}

	private void Lock()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
}