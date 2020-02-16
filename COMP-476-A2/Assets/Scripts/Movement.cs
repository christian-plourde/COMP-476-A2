using UnityEngine;
using System.Collections;

//base class for movement
public abstract class Movement
{
    private NPC character; //the npc character reference. We need this for its current position, speeds, etc.
    private bool has_arrived = false; //indicates if the character has arrived at his target or not
    private Vector3 target; //a vector 3 representing the target of the chracter's movement

    public bool HasArrived
    {
        get { return has_arrived; }
        set { has_arrived = value; }
    }

    public float DistanceToTarget
    {
        get { return (Target - Position).magnitude; } //how far away we are from the target
    }

    public Vector3 Target
    {
        get { return target; }
        set { target = value;
            has_arrived = false;
        }
    }

    public NPC NPC
    {
        get { return character; }
    }

    public Vector3 Position
    {
        get { return character.gameObject.transform.position; }
        set { character.gameObject.transform.position = value; }
    }

    public float MaxVelocity
    {
        get { return character.MaxVelocity; }
    }

    public float Velocity
    {
        get { return character.Velocity; }
        set { character.Velocity = value; }
    }

    public Movement(NPC npc)
    {
        this.character = npc;
    }

    public abstract void Move();
}

//movement type that first aligns to direction of motion then moves
public abstract class AlignedMovement : Movement
{
    protected const float time_to_target = 1.0f; 
    protected const float radius_of_satisfaction = 0.02f;
    protected const float angular_radius_of_satisfaction = 5.0f;
    protected const float angular_slow_down_radius = 10.0f;
    protected const float angular_time_to_target = 1.0f;
    protected const float cone_of_perception_distance = 1.0f;
    protected const float max_cone_radius = 45.0f;
    private bool aligned = false;

    public AlignedMovement(NPC npc) : base(npc) { }

    public bool TargetWithinCone
    {
        get {
            Vector3 dir = Target - Position;
            Quaternion target_rotation = Quaternion.LookRotation(dir, Vector3.up);
            float target_rotation_euler = target_rotation.eulerAngles.y;
            if (target_rotation_euler > 180)
                target_rotation_euler = target_rotation_euler - 360.0f;

            return Mathf.Abs((Orientation - target_rotation_euler)) < (ConeRadius / 2);
        }
    }

    public float ConeRadius
    {
        get { return (Velocity - MaxVelocity) / max_cone_radius; }
    }

    public float MaxAngularVelocity
    {
        get { return NPC.MaxAngularVelocity; }
    }

    public float AngularVelocity
    {
        get { return NPC.AngularVelocity; }
        set { NPC.AngularVelocity = value; }
    }

    public float MaxAngularAcceleration
    {
        get { return NPC.MaxAngularAcceleration; }
    }

    public bool Aligned
    {
        get { return aligned; }
        set { aligned = value; }
    }

    public float Orientation
    {
        get {
            if (NPC.gameObject.transform.rotation.eulerAngles.y > 180.0f)
                return NPC.gameObject.transform.rotation.eulerAngles.y - 360.0f;

            return NPC.gameObject.transform.rotation.eulerAngles.y; }

        set {
            Quaternion quat = new Quaternion();
            Vector3 euler_angs = new Vector3(0.0f, value, 0.0f);
            quat.eulerAngles = euler_angs;
            NPC.gameObject.transform.rotation = quat; }
    }

    protected void StopAlignAndMove()
    {
        if (!Aligned)
        {
            Align();
        }

        else
        {
            Aligned = false;
            move();
        }
    }

    protected void AlignAndMove()
    {
        Align();
        move();
    }

    protected abstract void move();

    protected virtual void Align()
    {
        //----------------------------- LOOK DIRECTION ---------------------------//

        //the first thing we need to do here is determine the target orientation
        //this is done by taking the difference in the orientation of the line connection the car and target and the current orientation
        //of the car
        //first lets find the orientation of that line
        //lets get the direction from the position of the car to the target
        Vector3 dir = Target - Position;

        //next lets create the target rotation, which is the orientation to the target position
        Quaternion target_rotation = Quaternion.LookRotation(dir, Vector3.up);
        float target_rotation_euler = target_rotation.eulerAngles.y;

        //since we want the target rotation to be signed, if it is larger than 180 degrees, we will make it negative from 0
        if (target_rotation_euler > 180)
            target_rotation_euler = target_rotation_euler - 360.0f;

        //we only allow the character to rotate around the y_axis, therefore we only need the angle around y axis that separates
        //the car and the target

        float rotation_diff = target_rotation_euler - Orientation;

        //------------------------------------- RADIUS OF SATISFACTION CHECK ---------------------------------//

        //we need to check if we are within the radius of satifaction, it which case, we should stop rotating the character
        //set its orientation to that of the target, and return
        if (Mathf.Abs(rotation_diff) < angular_radius_of_satisfaction)
        {
            Orientation = target_rotation_euler;
            Aligned = true;
            return;
        }

        //if this was not the case we need to keep rotating

        //-------------------------------------- GOAL ANGULAR VELOCITY -------------------------------------//

        //first step is to compute the goal angular velocity, which is the speed to the target based on time to target and angle from target
        float goal_angular_velocity = MaxAngularVelocity * rotation_diff / angular_slow_down_radius;

        //with the goal angular acceleration the character should have based on time to target
        float ang_acc = MaxAngularAcceleration;

        //we only change the angular acceleration if if its less than the max acceleration, otherwise acceleration is capped at its max
        float char_acc = (goal_angular_velocity - AngularVelocity) / angular_time_to_target;

        if (char_acc < ang_acc)
            ang_acc = char_acc;

        //------------------------------------ CAR VELOCITY ----------------------------------------//

        //Now that we have the car's accelertion, we can recompute its angular velocity

        AngularVelocity = AngularVelocity + ang_acc * Time.deltaTime;

        if (AngularVelocity > MaxAngularVelocity)
            AngularVelocity = MaxAngularVelocity;

        //------------------------------------- CAR ORIENTATION ---------------------------------//

        //finally we compute the new orientation based on the new velocity

        Orientation = Orientation + AngularVelocity * Time.deltaTime;
    }

    public abstract void resetTarget();

}

public abstract class ReachMovement : AlignedMovement
{
    public ReachMovement(NPC npc) : base(npc) { }

    public override void Move()
    {
        if (Velocity < 0.1 * MaxVelocity)
        {
            if (DistanceToTarget < cone_of_perception_distance)
                move();

            else
                StopAlignAndMove();
        }

        else
        {
            if (TargetWithinCone)
                AlignAndMove();

            else
                StopAlignAndMove();
        }
    }
}

public class KinematicSeek : ReachMovement
{
    public KinematicSeek(NPC npc) : base(npc) { }

    protected override void move()
    {
        //Debug.Log("Car: " + Position.ToString() + " Target: " + Target.ToString());
        //------------------------------ VELOCITY DIRECTION --------------------------------//

        //the direction of the velocity is computed by taking the difference between the position of the target and the character
        Vector3 velocity_dir = Target - Position;
        //Now that we have the direction we need to normalize it
        velocity_dir = velocity_dir.normalized;

        //-------------------------------- SEEK VELOCITY ---------------------------------//

        //Now that the velocity direction has been computed we need to calculate the seek velocity
        //this is done by multiplying the max velocity of the car by the velocity direction (normalized)
        Vector3 seek_velocity = MaxVelocity * velocity_dir;

        //set the velocity of the car to the seek velocity
        Velocity = seek_velocity.magnitude;

        //------------------------------- POSITION UPDATE -------------------------------//

        //the final step is to use the seek velocity to update the position of the car
        Position = Position + seek_velocity * Time.deltaTime;
    }

    public override void resetTarget()
    {
        
    }
}

public class KinematicArrive : ReachMovement
{
    public KinematicArrive(NPC npc) : base(npc) { }

    protected override void move()
    {
        //------------------------------ VELOCITY DIRECTION --------------------------------//

        //the direction of the velocity is computed by taking the difference between the position of the target and the character
        Vector3 velocity_dir = Target - Position;
        //Now that we have the direction we need to normalize it
        velocity_dir = velocity_dir.normalized;

        //----------------------------- VELOCITY MAGNITUDE -------------------------------//

        //the next step is to determine the magnitude of the velocity to use. This will either be the maximum velocity, or 
        //the velocity based on the time to target. Whichever is smallest will be the one we choose.
        float char_to_target_speed = (Target - Position).magnitude / time_to_target;
        float velocity = 0.0f;

        if (MaxVelocity < char_to_target_speed)
            velocity = MaxVelocity;

        else
            velocity = char_to_target_speed;

        Velocity = velocity;

        //now that we have the velocity we will need to check if we should stop

        //-------------------------------- RADIUS OF SATISFACTION CHECK ----------------------//

        //to check if we should stop, we check how far we are to the target. If the distance is less than the radius of satisfaction,
        //we set the position of the car to the position of the target and return

        if ((Target - Position).magnitude < radius_of_satisfaction)
        {
            Position = Target;
            HasArrived = true;
            return;
        }

        //if we are outside the radius of satisfaction, we need to move with the prescribed velocity
        Vector3 v_move = velocity * velocity_dir;
        Position = Position + v_move * Time.deltaTime;
    }

    public override void resetTarget()
    {

    }
}

public class Pursue : KinematicSeek
{
    private NPC target_char;

    public NPC TargetCharacter
    {
        get { return target_char; }
        set { target_char = value; }
    }

    public Pursue(NPC npc, NPC target_npc) : base(npc) { this.target_char = target_npc;}

    public override void resetTarget()
    {
        try
        {
           Target = target_char.Position;
        }

        catch
        {

        }
        
    }

    protected override void move()
    {
        resetTarget();
        base.move();
    }
}

public abstract class EvadeMovement : AlignedMovement
{
    public EvadeMovement(NPC npc) : base(npc) { }

    public override void Move()
    {
        if (DistanceToTarget < cone_of_perception_distance)
            move();
        else
            StopAlignAndMove();
    }
}

public class KinematicFlee : EvadeMovement
{
    public KinematicFlee(NPC npc) : base(npc) { }

    protected override void move()
    {
        //Debug.Log("Car: " + Position.ToString() + " Target: " + Target.ToString());
        //------------------------------ VELOCITY DIRECTION --------------------------------//

        //the direction of the velocity is computed by taking the difference between the position of the target and the character
        Vector3 velocity_dir = Position - Target;
        //Now that we have the direction we need to normalize it
        velocity_dir = velocity_dir.normalized;

        //-------------------------------- SEEK VELOCITY ---------------------------------//

        //Now that the velocity direction has been computed we need to calculate the seek velocity
        //this is done by multiplying the max velocity of the car by the velocity direction (normalized)
        Vector3 seek_velocity = MaxVelocity * velocity_dir;

        //set the velocity of the car to the seek velocity
        Velocity = seek_velocity.magnitude;

        //------------------------------- POSITION UPDATE -------------------------------//

        //the final step is to use the seek velocity to update the position of the car
        Position = Position + seek_velocity * Time.deltaTime;
    }

    protected override void Align()
    {
        //----------------------------- LOOK DIRECTION ---------------------------//

        //the first thing we need to do here is determine the target orientation
        //this is done by taking the difference in the orientation of the line connection the car and target and the current orientation
        //of the car
        //first lets find the orientation of that line
        //lets get the direction from the position of the car to the target
        Vector3 dir = Position - Target;

        //next lets create the target rotation, which is the orientation to the target position
        Quaternion target_rotation = Quaternion.LookRotation(dir, Vector3.up);
        float target_rotation_euler = target_rotation.eulerAngles.y;

        //since we want the target rotation to be signed, if it is larger than 180 degrees, we will make it negative from 0
        if (target_rotation_euler > 180)
            target_rotation_euler = target_rotation_euler - 360.0f;

        //we only allow the character to rotate around the y_axis, therefore we only need the angle around y axis that separates
        //the car and the target

        float rotation_diff = target_rotation_euler - Orientation;

        //------------------------------------- RADIUS OF SATISFACTION CHECK ---------------------------------//

        //we need to check if we are within the radius of satifaction, it which case, we should stop rotating the character
        //set its orientation to that of the target, and return
        if (Mathf.Abs(rotation_diff) < angular_radius_of_satisfaction)
        {
            Orientation = target_rotation_euler;
            Aligned = true;
            return;
        }

        //if this was not the case we need to keep rotating

        //-------------------------------------- GOAL ANGULAR VELOCITY -------------------------------------//

        //first step is to compute the goal angular velocity, which is the speed to the target based on time to target and angle from target
        float goal_angular_velocity = MaxAngularVelocity * rotation_diff / angular_slow_down_radius;

        //with the goal angular acceleration the character should have based on time to target
        float ang_acc = MaxAngularAcceleration;

        //we only change the angular acceleration if if its less than the max acceleration, otherwise acceleration is capped at its max
        float char_acc = (goal_angular_velocity - AngularVelocity) / angular_time_to_target;

        if (char_acc < ang_acc)
            ang_acc = char_acc;

        //------------------------------------ CAR VELOCITY ----------------------------------------//

        //Now that we have the car's accelertion, we can recompute its angular velocity

        AngularVelocity = AngularVelocity + ang_acc * Time.deltaTime;

        if (AngularVelocity > MaxAngularVelocity)
            AngularVelocity = MaxAngularVelocity;

        //------------------------------------- CAR ORIENTATION ---------------------------------//

        //finally we compute the new orientation based on the new velocity

        Orientation = Orientation + AngularVelocity * Time.deltaTime;
    }

    public override void resetTarget()
    {

    }
}

public class Evade : KinematicFlee
{
    private NPC target_char;

    public NPC TargetCharacter
    {
        get { return target_char; }
        set { target_char = value; }
    }

    public Evade(NPC npc, NPC target_char) : base(npc) { this.target_char = target_char; }

    public override void resetTarget()
    {
        Target = target_char.Position;
    }

    protected override void move()
    {
        resetTarget();
        base.move();
    }

}

public class Wander : AlignedMovement
{
    private const float wander_target_distance = 1.0f;
    private const float wander_target_radius = 0.6f;
    private const float target_radius_of_satisfaction = 0.2f;
    private bool first_run = true;

    public Wander(NPC car) : base(car) 
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    public override void resetTarget()
    {
        //------------------------------ RESETTING THE TARGET POSITION ----------------------//

        //We need to reset the target position. In order to do this, we pick a point in front of the car, with a fixed distance away
        Target = (NPC.transform.forward.normalized * wander_target_distance) + Position;

        Vector3 circle_vector = (NPC.transform.forward.normalized * wander_target_radius);

        //now we need to rotate this vector with a random angle around the y axis
        float angle = Random.Range(0.0f, 360.0f);

        circle_vector = new Vector3(circle_vector.x * Mathf.Cos(angle) + circle_vector.z * Mathf.Sin(angle),
                                    circle_vector.y, 
                                    -1 * circle_vector.z * Mathf.Sin(angle) + circle_vector.z * Mathf.Cos(angle));

        Target = Target + 1.2f*circle_vector;
    }

    protected override void move()
    {
        //------------------------------ VELOCITY DIRECTION --------------------------------//

        //the direction of the velocity is computed by taking the difference between the position of the target and the character
        Vector3 velocity_dir = Target - Position;
        //Now that we have the direction we need to normalize it
        velocity_dir = velocity_dir.normalized;

        //-------------------------------- SEEK VELOCITY ---------------------------------//

        //Now that the velocity direction has been computed we need to calculate the seek velocity
        //this is done by multiplying the max velocity of the car by the velocity direction (normalized)
        Vector3 seek_velocity = MaxVelocity * velocity_dir;

        //set the velocity of the car to the seek velocity
        Velocity = seek_velocity.magnitude;

        //------------------------------- POSITION UPDATE -------------------------------//

        //the final step is to use the seek velocity to update the position of the car
        Position = Position + seek_velocity * Time.deltaTime;
    }

    public override void Move()
    {
        if(DistanceToTarget <= target_radius_of_satisfaction || first_run)
        {
            resetTarget();
            first_run = false;
        }

        AlignAndMove();
    }
}