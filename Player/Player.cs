using Godot;
using System;

public class Player : KinematicBody
{
    //References for nodes
    Camera camera_;
    Spatial head_;
    RayCast Floor_cast;

    //Variables used for movement
    Vector3 Direction_vect;
    Vector3 Normal_vect;
    Vector3 Velocity_vect;
    Vector3 Grav;

    //Variables used for movement
    bool Floor_dect;
    bool Floor_dect_PFPS;
    bool Jumping;

    //Variable that controls mouse sensitivity (Exported for changing)
    [Export]float MouseSenstivity = 0.2f;

    //Set references
    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
        camera_ = GetNode<Camera>("Head/Camera");
        head_ = GetNode<Spatial>("Head");
        Floor_cast = GetNode<RayCast>("RayCast");
    }

    //Physics movement thingies
    public override void _PhysicsProcess(float delta)
    {
        //Execute parts separately
        ProcessInputs(delta);
        ProcessMovement(delta);
    }

    //Processes Inputs
    public void ProcessInputs(float delta)
    {
        //Resets Direction_vect
        Direction_vect = Vector3.Zero;
        //Sets Direction_vect depending on the way you are looking + your inputs
        Direction_vect += (Input.GetActionStrength("Player_up")-Input.GetActionStrength("Player_down"))*-Transform.basis.z;
        Direction_vect += (Input.GetActionStrength("Player_left")-Input.GetActionStrength("Player_right"))*-Transform.basis.x;

        //Normalizes the vector if Abs is superior to 1 (for future controller support)
        if ((Mathf.Abs(Direction_vect.x) + Mathf.Abs(Direction_vect.z)) > 1)
        {
            Direction_vect = Direction_vect.Normalized();
        }
    }

    //Processes movement
    public void ProcessMovement(float delta)
    {
        //Sets the normal in a variable to calculate the dot product later
        if (GetFloorNormal() != Vector3.Zero)
        {
        Normal_vect = GetFloorNormal();
        }
        else if(Floor_cast.GetCollisionNormal() != Vector3.Zero)
        {
            Normal_vect = Floor_cast.GetCollisionNormal();
        }

        //Sets a redundant check to secure that the character isn't on the ground (and sets a fake normal that is perpendicular to the up vector)
        if (!Floor_cast.IsColliding())
        {
            Normal_vect = Vector3.Forward;
            Floor_dect = false;
        }
        //Reset the redundant check to true if it detects it touched the floor (and sets a -10 gravity value)
        if (IsOnFloor())
        {
            Floor_dect = true;
            Grav.y = -10;
        }
        //Sets the velocity of gravity if the check is negative
        if (!Floor_dect)
        {
            Grav.y -= 30.0f*delta;
        }

        //Sets the velocity vector (acceleration coming in the future)
        Velocity_vect = Direction_vect*20;

        //If the dot product of the normalized normal and the Vector 3 that points downwards is -0.5 < or 0.5 > and the player presed jump (aka space) set a variable to jump and change the gravity
        if ((new Vector3(0,-1,0).Dot(Normal_vect.Normalized()) > 0.5 || new Vector3(0,-1,0).Dot(Normal_vect.Normalized()) < -0.5) && Input.IsActionPressed("Player_jump"))
        {
            Jumping = true;
            Grav.y = 10;
        }

        //If you were not jumping and you fell from a platform it resets the gravity to 0 for 1 frame
        if(Floor_dect_PFPS && !Floor_dect && !Jumping)
        {
            Grav.y = 0;
        }

        //If you were jumping and you touched the floor it reset jumping to false
        if(!Floor_dect_PFPS && Floor_dect && Jumping)
        {
            Jumping = false;
        }

        //If the dot product of the normalized normal and the Vector 3 that points downwards is not -0.5 < and not 0.5 > or the Vector_velocity isn't 0 or is jumping, it aplies the movement WITH gravity
        if (((!(new Vector3(0,-1,0).Dot(Normal_vect.Normalized()) > 0.5) && !(new Vector3(0,-1,0).Dot(Normal_vect.Normalized()) < -0.5)) || Velocity_vect != Vector3.Zero) || Jumping)
        {
        MoveAndSlide((Velocity_vect)+Grav,Vector3.Up, true);
        }

        Floor_dect_PFPS = Floor_dect;

        //If you touch the ceiling with a positive y value on gravity and you keep pressing jump you get ""grabed"" on the ceiling, else you bonk
        if (IsOnCeiling() && Grav.y > 0)
        {
            if (Input.IsActionPressed("Player_jump"))
            {
                Grav.y = 2;
            }
            else
            {
            Grav.y = 0;
            }
        }
    }

    //Camera movement/Pause start
    public override void _Input(InputEvent @event)
    {
    //Changes camera and rotation values depending on mouse movement
    if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
    {
        InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
        head_.RotateX(Mathf.Deg2Rad(-mouseEvent.Relative.y * MouseSenstivity)); 
        RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * MouseSenstivity));
    }
    //Changes mouse mode
    if (Input.IsActionJustPressed("ui_cancel"))
    {
        if (Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
        }
        else
        {
            Input.SetMouseMode(Input.MouseMode.Captured);
        }
    }
        //Clamps the head x rotation to -90 and 90
        Vector3 HeadRotation = head_.RotationDegrees;
        HeadRotation.x = Mathf.Clamp(head_.RotationDegrees.x, -90, 90);
        head_.RotationDegrees = HeadRotation;
    }
}
