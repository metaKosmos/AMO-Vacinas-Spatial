/*****************************************************************************/
/* File: PhysicsObject.cs                                                    */
/*                                                                           */
/* Description:                                                              */
/*                                                                           */
/* Resets the world position of this physics object when idle or it falls    */
/* out of the world.                                                         */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/1/15   * Austin  * Compile fixes for Unity 5                           */
/*---------------------------------------------------------------------------*/
/* 12/05/14  * Austin  * Zero velocity when setting new position             */
/*---------------------------------------------------------------------------*/
/* 11/08/12  * Austin  * Initial release                                     */
/*****************************************************************************/

using UnityEngine;
using ColliderView;

public class PhysicsObject : MonoBehaviour
{
	public const float maxIdleTimeSecs = 3f;
	private float idleTime = 0f;

	private Rigidbody compRigidbody;
	private float startY;

	private void Start()
	{
		compRigidbody = CV_Common.SafeGetComponent< Rigidbody >( gameObject );
		startY = transform.localPosition.y;
	}

	private void Update()
	{
		float velLengthSq = compRigidbody.velocity.sqrMagnitude;
		
		if ( velLengthSq < 0.1f || transform.localPosition.y < -1f )
		{
			idleTime += Time.deltaTime;

			if ( idleTime >= maxIdleTimeSecs )
			{
				NewPosition();
			}
		}
		else
		{
			idleTime = 0f;
		}
	}

	private void NewPosition()
	{
		Vector3 pos = Random.insideUnitSphere * 4.75f;
		pos.y = startY;
		
		compRigidbody.velocity = compRigidbody.angularVelocity = Vector3.zero;
		transform.localPosition = pos;
		transform.localEulerAngles = Random.insideUnitSphere * 180f;
		
		idleTime = 0f;
	}
}