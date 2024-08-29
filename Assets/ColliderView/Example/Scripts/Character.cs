/*****************************************************************************/
/* File: Character.cs                                                        */
/*                                                                           */
/* Description:                                                              */
/*                                                                           */
/* Uses the StateManager to drive a wandering character AI.                  */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/19/15  * Austin  * Removed force setting of CV_Camera renderFlags      */
/*---------------------------------------------------------------------------*/
/*  3/1/15   * Austin  * Compile fixes for Unity 5                           */
/*---------------------------------------------------------------------------*/
/* 11/08/12  * Austin  * Initial release                                     */
/*****************************************************************************/

using UnityEngine;
using ColliderView;

// begin by defining zero-based consecutive integer values to represent your states
static class CharacterStates
{
	public const int Idle	= 0;
	public const int Walk	= 1;
	public const int Jump	= 2;
	public const int Count	= 3;

	// only useful for debugging and can be omitted
	public static string ToString( int state )
	{
		switch( state )
		{
			case Idle:
				return "IDLE";

			case Walk:
				return "WALK";

			case Jump:
				return "JUMP";

			default:
				return "UNKNOWN";
		}
	}
}

public class Character : MonoBehaviour
{
	public StateManager sm;

	public Vector3 goal;
	private const float moveSpeed = 10.0f;
	private const float turnSpeed = 2.5f;

	private Rigidbody compRigidbody;
	private Animation compAnimation;

	void Start()
	{
		compRigidbody = CV_Common.SafeGetComponent< Rigidbody >( gameObject );
		compAnimation = CV_Common.SafeGetComponent< Animation >( gameObject );
	}

	void Awake() 
	{
		// create a new state manager by passing in the starting state along with the total number of states
		sm = new StateManager( CharacterStates.Idle, CharacterStates.Count );

		// register each state with a start, update, and (optional) timed event function
		if ( !sm.RegisterState( CharacterStates.Idle, StartIdle, UpdateIdle, AiWander ) )
		{
			Debug.LogError( "RegisterState() failed for state: " + CharacterStates.ToString( CharacterStates.Idle ) );
		}

		if ( !sm.RegisterState( CharacterStates.Walk, StartWalk, UpdateWalk, AttemptJump ) )
		{
			Debug.LogError( "RegisterState() failed for state: " + CharacterStates.ToString( CharacterStates.Walk ) );
		}

		if ( !sm.RegisterState( CharacterStates.Jump, StartJump, UpdateJump ) )
		{
			Debug.LogError( "RegisterState() failed for state: " + CharacterStates.ToString( CharacterStates.Jump ) );
		}
	}

	void Update()
	{
		// let the state manager drive the AI
		sm.Update();
	}
	
	// physics-based locomotion
	void FixedUpdate()
	{
		// animation override?
		if ( compRigidbody.isKinematic )
		{
			return;
		}

		// never rotate the rigid-body (the animation will handle this)
		compRigidbody.angularVelocity = Vector3.zero;

		if ( sm.CurrentState != CharacterStates.Walk )
		{
			return;
		}

		// direction of travel is always towards goal
		Vector3 dir = ( goal - compRigidbody.position );
		dir.y = 0.0f;
		dir.Normalize();

		// do the move
		compRigidbody.AddForce( dir * Time.deltaTime * moveSpeed, ForceMode.VelocityChange );

		// look in the direction of the velocity (note that this doesn't rotate the rigid-body capsule)
		if ( dir != this.transform.forward && compRigidbody.velocity != Vector3.zero )
		{
			Quaternion q = Quaternion.LookRotation( compRigidbody.velocity );
			q.x = q.z = 0.0f;
			this.transform.rotation = Quaternion.Slerp( this.transform.rotation, q, Time.deltaTime * turnSpeed );
		}
	}

	private void StartIdle()
	{
		compAnimation["idle"].speed = Random.Range( 1.2f, 1.5f );
		compAnimation.CrossFade( "idle" );

		float length = compAnimation["idle"].length / compAnimation["idle"].speed;

		// the function AiWander() will be called by the StateManager once this timer triggers
		sm.TimedEvent = Random.Range( length * 0.5f, length );
	}

	private void UpdateIdle()
	{
	}
	
	private void AiWander()
	{
		goal = Random.insideUnitSphere * 4.75f;
		goal.y = 0.0f;

		sm.CurrentState = CharacterStates.Walk;
	}

	private void StartWalk()
	{
		compRigidbody.isKinematic = false;

		compAnimation["walk"].speed = 0.9f;

		if ( sm.PreviousState == CharacterStates.Jump )
		{
			compAnimation.CrossFade( "walk" );
		}
		else
		{
			compAnimation.CrossFade( "walk", 0.5f );
		}

		sm.TimedEvent = Random.Range( 3.0f, 5.0f );
	}

	private void UpdateWalk()
	{
		Vector3 dir = ( goal - transform.position );
		dir.y = 0.0f;
		dir.Normalize();

		float dot = Vector3.Dot( compRigidbody.velocity.normalized, dir );

		if ( dot < 0.01f )
		{
			// likely stuck on geometry
			sm.CurrentState = CharacterStates.Idle;
		}
		else if ( IsAtGoal() )
		{
			sm.CurrentState = CharacterStates.Idle;
		}
	}

	private void AttemptJump()
	{
		if ( Vector3.Distance( transform.position, goal ) > 1.0f )
		{
			sm.CurrentState = CharacterStates.Jump;
		}
	}

	private void StartJump()
	{
		compRigidbody.isKinematic = true;

		compAnimation.Rewind( "jump" );
		compAnimation.CrossFade( "jump" );
	}

	private void UpdateJump()
	{
		if ( compAnimation["jump"].normalizedTime < 0.5f )
		{
			transform.position = transform.position + transform.forward * 0.0075f;
		}
		else if ( compAnimation["jump"].normalizedTime >= 1.0f )
		{
			sm.CurrentState = CharacterStates.Walk;
		}
	}

	private bool IsAtGoal()
	{
		float x = Mathf.Abs( compRigidbody.position.x - goal.x );
		float z = Mathf.Abs( compRigidbody.position.z - goal.z );

		// close to goal
		return ( x < 0.1f && z < 0.1f );
	}
}