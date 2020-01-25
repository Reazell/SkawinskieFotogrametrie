using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson{

    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
	[RequireComponent(typeof (MouseLook))]

    public class FirstPersonController : MonoBehaviour{

        [SerializeField] bool m_IsWalking;
        [SerializeField] float m_WalkSpeed;
        [SerializeField] float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] float m_RunstepLenghten;
        [SerializeField] float m_JumpSpeed;
        [SerializeField] float m_StickToGroundForce;
        [SerializeField] float m_GravityMultiplier;
        [SerializeField] bool m_UseFovKick;
        [SerializeField] FOVKick m_FovKick = new FOVKick();
        [SerializeField] bool m_UseHeadBob;
        [SerializeField] CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] float m_StepInterval;
        [SerializeField] AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] AudioClip m_LandSound;           // the sound played when character touches back on ground.

        Camera m_Camera;
        bool m_Jump;
        float m_YRotation;
        Vector2 m_Input;
        Vector3 m_MoveDir = Vector3.zero;
        CharacterController m_CharacterController;
        CollisionFlags m_CollisionFlags;
        bool m_PreviouslyGrounded;
        Vector3 m_OriginalCameraPosition;
        float m_StepCycle;
        float m_NextStep;
        bool m_Jumping;
        AudioSource m_AudioSource;

		// Custom stuff for my own version of this stanard script
		[Header("Custom Flags")]
		[Tooltip("Toggle jump ability on and off")]
		public bool enableJump;
		[Tooltip("Toggle movement sounds on and off")]
		public bool enableMovementSounds;
		[Tooltip("Toggle movement on and off")]
		public bool enableMovement = true;

        void Start(){

            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().Init(transform , m_Camera.transform);

        }

        void Update(){

            RotateView();

			// the jump state needs to read here to make sure it is not missed
			if(enableJump && !m_Jump){
				m_Jump = Input.GetButtonDown("Jump");
			}

            if(!m_PreviouslyGrounded && m_CharacterController.isGrounded){
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if(!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded){
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

        }


        void PlayLandingSound(){

			if(enableMovementSounds){
				m_AudioSource.clip = m_LandSound;
				m_AudioSource.Play ();
				m_NextStep = m_StepCycle + .5f;
			}

        }


        void FixedUpdate(){

            float speed;
            GetInput(out speed);

			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = Vector3.zero;
			
			if(enableMovement){
				desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
			}

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,m_CharacterController.height/2f);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;

            if(m_CharacterController.isGrounded){

                m_MoveDir.y = -m_StickToGroundForce;

                if(m_Jump){

                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;

                }

            }else{
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

        }


        void PlayJumpSound(){

			if(enableMovementSounds){
				m_AudioSource.clip = m_JumpSound;
				m_AudioSource.Play ();
			}

        }


        void ProgressStepCycle(float speed){

            if(m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0)){
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*Time.fixedDeltaTime;
            }

            if(!(m_StepCycle > m_NextStep)){
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();

        }

        void PlayFootStepAudio(){

            if(!m_CharacterController.isGrounded){
                return;
            }

			if(enableMovementSounds){

				// pick & play a random footstep sound from the array,
				// excluding sound at index 0
				int n = Random.Range (1, m_FootstepSounds.Length);
				m_AudioSource.clip = m_FootstepSounds [n];
				m_AudioSource.PlayOneShot (m_AudioSource.clip);
				// move picked sound to index 0 so it's not picked next time
				m_FootstepSounds [n] = m_FootstepSounds [0];
				m_FootstepSounds [0] = m_AudioSource.clip;

			}

        }


        void UpdateCameraPosition(float speed){

            Vector3 newCameraPosition;

            if(!m_UseHeadBob){
                return;
            }

            if(m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded){

                m_Camera.transform.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();

            }else{

                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();

            }

            m_Camera.transform.localPosition = newCameraPosition;

        }


        void GetInput(out float speed){
            
			// Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
            bool waswalking = m_IsWalking;

			// Removing running completely
			//#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            //m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
			//#endif

            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if(m_Input.sqrMagnitude > 1){
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if(m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0){
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }

        }

        void RotateView(){
			gameObject.GetComponent<MouseLook>().LookRotation(transform, m_Camera.transform);
        }

        void OnControllerColliderHit(ControllerColliderHit hit){

            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if(m_CollisionFlags == CollisionFlags.Below){
                return;
            }

            if(body == null || body.isKinematic){
                return;
            }

            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);

        }

    }

}
