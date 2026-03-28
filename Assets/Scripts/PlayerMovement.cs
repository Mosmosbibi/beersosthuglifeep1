using PurrNet.Prediction;
using UnityEngine;

public class PlayerMovement : PredictedIdentity<PlayerMovement.MoveInput, PlayerMovement.MoveState>
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float planerDamping = 10f;

    [SerializeField] private InputReader inputReader;
    [SerializeField] private FirstPersonCamera firstPersonCamera;
    [SerializeField] private PredictedRigidbody predictedRigidbody;

    protected override void LateAwake()
    {
        if (isOwner)
        {
            firstPersonCamera.Init();
        }
    }

    protected override void Simulate(MoveInput input, ref MoveState state, float delta)
    {
        Vector3 targetVelocity = (transform.forward * input.moveDirection.y + transform.right * input.moveDirection.x) * moveSpeed;
        predictedRigidbody.AddForce(targetVelocity * acceleration);

        Vector3 horizontal = new Vector3(predictedRigidbody.velocity.x, 0, predictedRigidbody.velocity.z);
        predictedRigidbody.AddForce(-horizontal * planerDamping);
        if (horizontal.magnitude > moveSpeed)
        {
            predictedRigidbody.velocity = new Vector3(targetVelocity.x, predictedRigidbody.velocity.y, targetVelocity.z);
        }
        if (input.cameraForward.HasValue)
        {
            Vector3 camForward = input.cameraForward.Value;
            camForward.y = 0;
            if (camForward.sqrMagnitude > 0.0001)
            {
                predictedRigidbody.MoveRotation(Quaternion.LookRotation(camForward.normalized));
            }
        }
    }

    protected override void GetFinalInput(ref MoveInput input)
    {
        input.moveDirection = inputReader.MovementInput;
        input.cameraForward = firstPersonCamera.forward;
    }

    protected override void SanitizeInput(ref MoveInput input)
    {
        if (input.moveDirection.magnitude > 1)
        {
            input.moveDirection.Normalize();
        }
    }

    public struct MoveInput : IPredictedData
    {
        public Vector2 moveDirection;
        public Vector3? cameraForward;

        public void Dispose() { }
    }

    public struct MoveState : IPredictedData<MoveState>
    {
        public void Dispose() { }
    }
}



