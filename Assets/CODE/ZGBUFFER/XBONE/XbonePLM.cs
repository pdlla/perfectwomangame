﻿using UnityEngine;
using System.Collections;

public class XbonePLM
{


#if UNITY_XBOXONE 

	public void Start()
	{
		if (XboxOneApplicationExecutionState.Terminated == XboxOnePLM.GetPreviousExecutionState ()) 
		{
			//TODO attemp to load
		}

		XboxOnePLM.OnSuspendingEvent += Suspending;
		XboxOnePLM.OnResourceAvailabilityChangedEvent += ResourceAvailabilityChangedEvent;
		XboxOnePLM.OnResumingEvent += Resuming;
		//XboxOnePLM.OnActivationEvent += Activation;
	}

	void Suspending()
	{
		//TODO save

		XboxOnePLM.AmReadyToSuspendNow ();
	}

	void ResourceAvailabilityChangedEvent(bool aConstrained)
	{
		//TODO (also make sure the boolean is correct..
	}

	void Resuming(double aTime)
	{
		//TODO check if user has changed
	}

	void Activation(ActivatedEventArgs args)
	{
		//TODO do I need to return something here toget out of constrained mode???
	}

#else
	public void Start()
	{
	}
#endif


}