using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(CharacterController))]
	public class FirstPersonController : MonoBehaviour
	{
		[SerializeField] private bool m_IsWalking;
		[SerializeField] private float m_WalkSpeed;
		public float m_RunSpeed;
		[SerializeField] private float m_JumpSpeed;
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_GravityMultiplier;
		[SerializeField] public MouseLook m_MouseLook;


		private Camera m_Camera;
		private bool m_Jump;
		private float m_YRotation;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private bool m_Jumping;

		public bool rotate = true;

		private void Start()
		{
			m_CharacterController = GetComponent<CharacterController>();
			m_Camera = Camera.main;
			m_Jumping = false;
			m_MouseLook.Init(transform, m_Camera.transform);
		}

		public void InitThing()
		{
			m_MouseLook.Init(transform, m_Camera.transform);
		}

		private void Update()
		{
			if (!m_Jump)
			{
				m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}

			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
			{
				m_MoveDir.y = 0f;
				m_Jumping = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
			{
				m_MoveDir.y = 0f;
			}

			m_PreviouslyGrounded = m_CharacterController.isGrounded;
		}

		private void FixedUpdate()
		{
			if (rotate)
				RotateView();

			float speed;
			GetInput(out speed);

			Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
							   m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x * speed;
			m_MoveDir.z = desiredMove.z * speed;


			if (m_CharacterController.isGrounded)
			{
				m_MoveDir.y = -m_StickToGroundForce;

				if (m_Jump)
				{
					m_MoveDir.y = m_JumpSpeed;
					m_Jump = false;
					m_Jumping = true;
				}
			}
			else
			{
				m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
			}
			m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

			m_MouseLook.UpdateCursorLock();
		}

		private void GetInput(out float speed)
		{
			float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			float vertical = CrossPlatformInputManager.GetAxis("Vertical");

			bool waswalking = m_IsWalking;

			speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			m_Input = new Vector2(horizontal, vertical);

			if (m_Input.sqrMagnitude > 1)
			{
				m_Input.Normalize();
			}

		}


		private void RotateView()
		{
			m_MouseLook.LookRotation(transform, m_Camera.transform);
		}


		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below)
			{
				return;
			}

			if (body == null || body.isKinematic)
			{
				return;
			}
			body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}

	}
}
