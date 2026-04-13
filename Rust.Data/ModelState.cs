using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class ModelState
{
    public enum Flag : int
    {
        Ducked = 1 << 0,
        Jumped = 1 << 1,
        OnGround = 1 << 2,
        Sleeping = 1 << 3,
        Sprinting = 1 << 4,
        OnLadder = 1 << 5,
        Flying = 1 << 6,
        Aiming = 1 << 7,
        Prone = 1 << 8,
        Mounted = 1 << 9,
		Relaxed = 1 << 10,
        OnPhone = 1 << 11,
		Crawling = 1 << 12,
		Loading = 1 << 13,
		HeadLook = 1 << 14,
		HasParachute = 1 << 15,
		Blocking = 1 << 16,
		Ragdolling = 1 << 17,
		Catching = 1 << 18,

        //if you add a new flag here be sure to update the maximum amount in BasePlayer-Tick.SendClientTick or the flag will be reset every tick
        //additionally please notify programmers/antihack in slack if a new modelstate is being added
        //!!! PLAYERS WILL BE INCORRECTLY BANNED IF THIS IS NOT DONE !!!
    }

	public ModelState()
	{
		// Set default initial params
		onground = true;
		waterLevel = 0;
		flying = false;
		sprinting = false;
		ducked = false;
		onLadder = false;
		sleeping = false;
		mounted = false;
		relaxed = false;
		crawling = false;
        loading = false;
        ragdolling = false;
		poseType = 0;
        ducking = 0f;
    }

	public bool HasFlag( Flag f ) { return ( flags & (int)f ) == (int)f; }
    public void SetFlag( Flag f, bool b ) { if ( b ) { flags |= (int)f; } else { flags &= ~(int)f; } }

    public bool ducked { get { return HasFlag( Flag.Ducked ); } set { SetFlag( Flag.Ducked, value ); } }
    public bool jumped { get { return HasFlag( Flag.Jumped ); } set { SetFlag( Flag.Jumped, value ); } }
    public bool onground { get { return HasFlag( Flag.OnGround ); } set { SetFlag( Flag.OnGround, value ); } }
    public bool sleeping { get { return HasFlag( Flag.Sleeping ); } set { SetFlag( Flag.Sleeping, value ); } }
    public bool sprinting { get { return HasFlag( Flag.Sprinting ); } set { SetFlag( Flag.Sprinting, value ); } }
    public bool onLadder { get { return HasFlag( Flag.OnLadder ); } set { SetFlag( Flag.OnLadder, value ); } }
    public bool flying { get { return HasFlag( Flag.Flying ); } set { SetFlag( Flag.Flying, value ); } }
    public bool aiming { get { return HasFlag( Flag.Aiming ); } set { SetFlag( Flag.Aiming, value ); } }
    public bool prone { get { return HasFlag( Flag.Prone ); } set { SetFlag( Flag.Prone, value ); } }
    public bool mounted { get { return HasFlag( Flag.Mounted ); } set { SetFlag( Flag.Mounted, value ); } }
	public bool relaxed { get { return HasFlag( Flag.Relaxed ); } set { SetFlag( Flag.Relaxed, value ); } }
    public bool onPhone { get { return HasFlag( Flag.OnPhone ); } set { SetFlag( Flag.OnPhone, value ); } }
	public bool crawling { get { return HasFlag( Flag.Crawling ); } set { SetFlag( Flag.Crawling, value ); } }
	public bool catching { get { return HasFlag( Flag.Catching ); } set { SetFlag( Flag.Catching, value ); } }
	public bool hasParachute { get { return HasFlag( Flag.HasParachute ); } set { SetFlag( Flag.HasParachute, value ); } }
	public bool ragdolling { get { return HasFlag( Flag.Ragdolling ); } set { SetFlag( Flag.Ragdolling, value ); } }
	public bool blocking 
	{ 
		get => HasFlag( Flag.Blocking );
		set => SetFlag(Flag.Blocking, value);
	}
	public bool headLook
	{
		get => HasFlag(Flag.HeadLook);
		set => SetFlag(Flag.HeadLook, value);
	}
    public bool loading { get { return HasFlag( Flag.Loading ); } set { SetFlag( Flag.Loading, value ); } }

    public static bool Equal( ModelState a, ModelState b )
    {
        if ( System.Object.ReferenceEquals( a, b ) ) return true;
        if ( ( (object)a == null ) || ( (object)b == null ) ) return false;

        if ( a.flags != b.flags ) return false;
        if ( a.waterLevel != b.waterLevel ) return false;
        if ( a.lookDir != b.lookDir ) return false;
        if ( a.poseType != b.poseType ) return false;
        if ( a.guidePrefab != b.guidePrefab ) return false;
        if ( a.guidePosition != b.guidePosition ) return false;
        if ( a.guideRotation != b.guideRotation ) return false;
        if ( a.guideValid != b.guideValid ) return false;
        if ( a.ducking != b.ducking ) return false;

        return true;
    }
}
