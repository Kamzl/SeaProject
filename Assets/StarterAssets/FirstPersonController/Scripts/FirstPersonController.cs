using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
		

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float WaterLevel = 0;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;
		
		[Header("Player Swimming")]
		[Tooltip("Move speed of the character in m/s")]
		public float SwimSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SwimSprintSpeed = 6.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SwimSpeedChangeRate = 10.0f;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		private float _moveSpeed = 4.0f;
		private float _sprintSpeed = 6.0f;
		private bool _isMovementRestricted = false;
		
		private Vector2 _clampDirection;
		private float _topClamp = 90.0f;
		private float _bottomClamp = -90.0f;
		private float _horizontalClamp;
		private bool _isCamClamped = false;

		// cinemachine
		private float _cinemachineTargetPitchX;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		private float _startingGravity = 15f;
		private bool _isSwimming = false;
		private Vector2 _nextTimeInWaterPos = Vector2.zero;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			
			// init move speed
			if (!_isMovementRestricted)
			{
				_moveSpeed = MoveSpeed;
				_sprintSpeed = SprintSpeed;
			}
			
			// init cam clamp
			if (!_isCamClamped)
			{
				_topClamp = TopClamp;
				_bottomClamp = BottomClamp;
			}

			_startingGravity = Gravity;
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			InputAction jumpAction = _playerInput.actions.FindAction("Jump", false);
			jumpAction.canceled += context =>
			{
				_input.jump = false;
			};
			InputAction crouchAction = _playerInput.actions.FindAction("Crouch", false);
			crouchAction.canceled += context =>
			{
				_input.crouch = false;
			};
			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
			SwimmingCheck();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}
		
		private void SwimmingCheck()
		{
			if (_isSwimming && CinemachineCameraTarget.transform.position.y >= WaterLevel)
			{
				Gravity = _startingGravity;
			}

			if (!_isSwimming && CinemachineCameraTarget.transform.position.y < WaterLevel)
			{
				Gravity = 0;
				_verticalVelocity = 0;
				// _controller.Move(new Vector3(0, -0.1f, 0));
				if (_nextTimeInWaterPos != Vector2.zero)
				{
					_controller.Move(new Vector3(_nextTimeInWaterPos.x - transform.position.x, 0, _nextTimeInWaterPos.y - transform.position.z));
					_nextTimeInWaterPos = Vector2.zero;
				}
			}
			
			_isSwimming = CinemachineCameraTarget.transform.position.y < WaterLevel;
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitchX += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitchX = ClampAngle(_cinemachineTargetPitchX, _bottomClamp, _topClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitchX, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
				if (_isCamClamped)
				{
					float playerAngle = Mathf.Abs(transform.localRotation.eulerAngles.y) > 180
						? -(360 - Mathf.Abs(transform.localRotation.eulerAngles.y))
						: Mathf.Abs(transform.localRotation.eulerAngles.y);
					float clampAngle = playerAngle - _clampDirection.y;
					if (Mathf.Abs(clampAngle) > _horizontalClamp)
					{
						transform.Rotate(Vector3.up * ((Mathf.Abs(clampAngle) - _horizontalClamp) * (_input.look.x < 0 ? 1 : -1)));
					}
				}
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			if(_isMovementRestricted) return;
			float targetSpeed = _input.sprint ? (_isSwimming ? SwimSprintSpeed : _sprintSpeed) : (_isSwimming ? SwimSpeed : _moveSpeed);

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero && !(_isSwimming && (_input.jump || _input.crouch))) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			if (_isSwimming)
			{
				currentHorizontalSpeed = new Vector3(_controller.velocity.x, _controller.velocity.y, _controller.velocity.z).magnitude;
			}

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * (_isSwimming ? SwimSpeedChangeRate : SpeedChangeRate));

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero || (_isSwimming && (_input.jump || _input.crouch)))
			{
				// move
				if (_isSwimming)
				{
					inputDirection = transform.right * _input.move.x + CinemachineCameraTarget.transform.forward * _input.move.y + (_input.jump ? transform.up : Vector3.zero) + (_input.crouch ? -transform.up : Vector3.zero);
				}
				else
				{
					inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
				}
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded and not swimming, do not jump
				if (!_isSwimming)
				{
					_input.jump = false;
				}
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		public void RestrictCamera(Vector2 direction, float verticalLimit, float horizontalLimit)
		{
			_isCamClamped = true;
			_clampDirection = direction;
			_topClamp = direction.x + verticalLimit / 2;
			_bottomClamp = direction.x - verticalLimit / 2;
			_horizontalClamp = horizontalLimit / 2;
		}
		
		public void UnrestrictCamera()
		{
			_isCamClamped = false;
			_topClamp = TopClamp;
			_bottomClamp = BottomClamp;
		}

		public void RestrictMovement()
		{
			_isMovementRestricted = true;
			_moveSpeed = 0;
			_sprintSpeed = 0;
		}

		public void UnrestrictMovement()
		{
			_isMovementRestricted = false;
			_moveSpeed = MoveSpeed;
			_sprintSpeed = SprintSpeed;
		}
		
		public void SetNextTimeInWaterPos(Vector2 pos)
		{
			_nextTimeInWaterPos = pos;
		}
	}
}