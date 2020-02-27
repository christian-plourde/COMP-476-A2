using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//basic npc class
public abstract class NPC : MonoBehaviour
{
    protected float MAX_VELOCITY = 0.6f;
    float currentVelocity = 0.0f;
    float MAX_ANGULAR_VELOCITY = 40.0f;
    float currentAngularVelocity = 0.0f;
    float MAX_ANGULAR_ACCELERATION = 50.0f;
    float MAX_ACCELERATION = 1.0f;
    Vector3 steering_velocity;
    AlignedMovement movement; //the current movement type for the character

    public abstract float MaxVelocity
    {
        get; 
        set; 
    }

    public float MaxAngularVelocity
    {
        get { return MAX_ANGULAR_VELOCITY; }
    }

    public float MaxAngularAcceleration
    {
        get { return MAX_ANGULAR_ACCELERATION; }
    }

    public float MaxAcceleration
    {
        get { return MAX_ACCELERATION; }
    }

    public float Velocity
    {
        get { return currentVelocity; }
        set { currentVelocity = value; }
    }

    public Vector3 SteeringVelocity
    {
        get { return steering_velocity; }
        set { steering_velocity = value; }
    }

    public float AngularVelocity
    {
        get { return currentAngularVelocity; }
        set { currentAngularVelocity = value; }
    }

    public Vector3 Position
    {
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    public AlignedMovement Movement
    {
        get { return movement; }
        set { movement = value; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Movement = new SteeringArrive(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //move is called as long as the destination is not reached
        if (!Movement.HasArrived)
            Movement.Move();

    }
}
