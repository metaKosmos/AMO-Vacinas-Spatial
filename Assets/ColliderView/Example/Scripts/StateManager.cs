/*****************************************************************************/
/* File: StateManager.cs                                                     */
/*                                                                           */
/* Description:                                                              */
/*                                                                           */
/* Unity implementation of an array-based generic finite state machine using */
/* function delegates.                                                       */
/*                                                                           */
/* Copyright © 2014 project|JACK, LLC                                        */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/* 11/08/12  * Austin  * Initial release                                     */
/*****************************************************************************/

using UnityEngine;

public delegate void StateCallback();

public class State
{
	private StateCallback start;
	private StateCallback update;
	private StateCallback timedEvent;

	public State( StateCallback start, StateCallback update, StateCallback timedEvent )
	{
		this.start = start;
		this.update = update;
		this.timedEvent = timedEvent;
	}

	public void Start()
	{
		start();
	}

	public void Update()
	{
		update();
	}

	public void TimedEvent()
	{
		timedEvent();
	}
}

public class StateManager
{
	private State[] states;
	private const int SM_INVALID_STATE = -1;
	
	private int statePrevious;
	public int PreviousState
	{
		get { return statePrevious; }
	}

	private int stateCurrent;
	public int CurrentState
	{
		get { return stateCurrent; }
		set { stateCurrent = value; }
	}

	private const float SM_EVENT_TIME_NONE = 0.0f;

	private float eventTime;
	public float TimedEvent
	{
		get { return eventTime; }
		set { this.eventTime = Time.time + value; }
	}

	public StateManager( int initialState, int maxStates )
	{
		statePrevious = SM_INVALID_STATE;
		eventTime = SM_EVENT_TIME_NONE;
		stateCurrent = initialState;

		states = new State[ maxStates ];
	}
	
	public bool RegisterState( int id, StateCallback start, StateCallback update, StateCallback timedEvent )
	{
		if ( id < 0 )
		{
			return false;
		}

		if ( id >= states.Length )
		{
			return false;
		}

		states[ id ] = new State( start, update, timedEvent );
		return true;
	}

	public bool RegisterState( int id, StateCallback start, StateCallback update )
	{
		return ( RegisterState( id, start, update, null ) );
	}

	private void StartState()
	{
		eventTime = SM_EVENT_TIME_NONE;
		states[ stateCurrent ].Start();
		statePrevious = stateCurrent;
	}

	public void Update()
	{
		if ( stateCurrent < 0 || stateCurrent >= states.Length )
		{
			Debug.LogError( "StateManager::Update() illegal state: " + stateCurrent );
			return;
		}

		if ( statePrevious == SM_INVALID_STATE )
		{
			StartState();
			return;
		}

		states[ stateCurrent ].Update();

		if ( eventTime > SM_EVENT_TIME_NONE && Time.time >= eventTime && statePrevious == stateCurrent )
		{
			eventTime = SM_EVENT_TIME_NONE;
			states[ stateCurrent ].TimedEvent();
		}

		if ( statePrevious != stateCurrent )
		{
			StartState();
		}
	}
}
