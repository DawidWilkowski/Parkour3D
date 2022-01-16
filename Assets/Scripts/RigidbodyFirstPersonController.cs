using System;
using UnityEngine;
using Random = UnityEngine.Random;
namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
      
        public AudioSource[] soundFX;
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   //Zmienne prêdkoœci
            public float BackwardSpeed = 4.0f;  
            public float StrafeSpeed = 4.0f;   
            public float SpeedInAir = 8.0f; 
            public float JumpForce = 5f;
           

            [HideInInspector] public float CurrentTargetSpeed = 8f;

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//do ty³u
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//do przodu
					CurrentTargetSpeed = ForwardSpeed;
				}

            }

        }


        public bool canrotate;
        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public Vector3 relativevelocity;

        public DetectObs detectGround;


        public bool Wallrunning;



        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private bool  m_IsGrounded;


        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        


        private void Awake()
        {
            canrotate = true;
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init (transform, cam.transform);
        }


        private void Update()
        {
            relativevelocity = transform.InverseTransformDirection(m_RigidBody.velocity);
            if (m_IsGrounded)
            {
                //skok
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    NormalJump();
                }

            }

        }


        private void LateUpdate()
        {
            if (canrotate)
            {
                RotateView();
            }
            else
            {
                mouseLook.LookOveride(transform, cam.transform);
            }
         

        }
        public void CamGoBack(float speed)
        {
            mouseLook.CamGoBack(transform, cam.transform, speed);

        }
        public void CamGoBackAll ()
        {
            mouseLook.CamGoBackAll(transform, cam.transform);

        }
        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            float h = input.x;
            float v = input.y;
            Vector3 inputVector = new Vector3(h, 0, v);
            inputVector = Vector3.ClampMagnitude(inputVector, 1);

            //predkosc na ziemi
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && m_IsGrounded && !Wallrunning)
            {
                if (Input.GetAxisRaw("Vertical") > 0.3f)
                {
                 
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * movementSettings.ForwardSpeed * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Vertical") < -0.3f)
                {

                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * -movementSettings.BackwardSpeed * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Horizontal") > 0.5f)
                {
                  
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * movementSettings.StrafeSpeed * Mathf.Abs(inputVector.x), 0, 0);
                }
                if (Input.GetAxisRaw("Horizontal") < -0.5f)
                {
                  
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * -movementSettings.StrafeSpeed * Mathf.Abs(inputVector.x), 0, 0);
                }

            }
            //predkosc w powietrzu
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && !m_IsGrounded  && !Wallrunning)
            {
                if (Input.GetAxisRaw("Vertical") > 0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * movementSettings.SpeedInAir * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Vertical") < -0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * -movementSettings.SpeedInAir * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Horizontal") > 0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * movementSettings.SpeedInAir * Mathf.Abs(inputVector.x), 0, 0);
                }
                if (Input.GetAxisRaw("Horizontal") < -0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * -movementSettings.SpeedInAir * Mathf.Abs(inputVector.x), 0, 0);
                }

            }

     
        }
        public void NormalJump()
        {
            soundFX[Random.Range(0,7)].Play();//DŸwiêk skoku ró¿ny
            m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
            m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
        }

  

      


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = Input.GetAxisRaw("Horizontal"),
                    y = Input.GetAxisRaw("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //jeœli gra zapauzowana to wy³¹cza myszke
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // ustawienie myszki zprzed pauzy
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, cam.transform);

       
        }


        /// sprawdzenie czy na ziemi
        private void GroundCheck()
        {
          if(detectGround.Obstruction)
            {
                m_IsGrounded = true;
            }
          else
            {
                m_IsGrounded = false;

            }
        }
    }
}
