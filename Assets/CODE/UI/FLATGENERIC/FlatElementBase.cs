using UnityEngine;
using System.Collections.Generic;
using System;

public class FlatElementBase {

    
    public class TimedEventHandler
    {
        Dictionary<QuTimer, Func<FlatElementBase,float,bool>> mTimedEvents = new Dictionary<QuTimer, Func<FlatElementBase,float,bool>>();
        public void update(float aDeltaTime, FlatElementBase aElement)
        {
            LinkedList<KeyValuePair<QuTimer, Func<FlatElementBase,float,bool>>> removal = new LinkedList<KeyValuePair<QuTimer, Func<FlatElementBase,float,bool>>>();
            foreach (KeyValuePair<QuTimer, Func<FlatElementBase,float,bool>> e in mTimedEvents)
            {
                e.Key.update(aDeltaTime);
                if (e.Key.isExpired())
                {
                    if (e.Value(aElement,e.Key.getTimeSinceStart()))
                        removal.AddLast(e);

                }
            }
            foreach (KeyValuePair<QuTimer, Func<FlatElementBase,float,bool>> e in removal)
                mTimedEvents.Remove(e.Key);
        }
        public void add_event(Func<FlatElementBase,float,bool> aEvent, float aTime)
        {
            mTimedEvents[new QuTimer(0,aTime)] = aEvent;
        }
    }

    
    GameObject mPrimaryGameObject;
    public GameObject PrimaryGameObject
    {
        get
        {
            return mPrimaryGameObject;
        }
        protected set
        {
            mPrimaryGameObject = value;
            //mBaseScale = Vector3.one; //maybe you should this with global scale instead...
            mBaseScale = mPrimaryGameObject.transform.localScale;
        }
    }


    bool mEnabled = true;
    public virtual bool Enabled
    {
        get
        {
            return mEnabled;
        }
        set
        {
            if (mEnabled != value)
            {
                mEnabled = value;
                foreach (Renderer e in PrimaryGameObject.GetComponentsInChildren<Renderer>())
                    e.enabled = value;
            }
        }
    }
	
    public virtual float SoftInterpolation{get;set;}
    public TimedEventHandler Events { get; private set; }


    Vector3 mCurrentPosition;
    Vector3 mTargetPosition;
    public Vector3 mLocalPosition = Vector3.zero;
    public virtual Vector3 SoftPosition
    {
        get{ return mTargetPosition; }
        set{ mTargetPosition = value; }
    }
    public virtual Vector3 HardPosition
    {
        get { return mCurrentPosition; }
        set 
        { 
            mCurrentPosition = value; 
            mTargetPosition = value; 
        }
    }
	public virtual float PositionInterpolationMaxLimit //maximum rate of position change in pixels per second
	{ get; set; }
	public virtual float PositionInterpolationMinLimit //in pixels per second
	{ get; set; }

    Vector3 mBaseScale;
    Vector3 mCurrentScale;
    Vector3 mTargetScale;
    public Vector3 mLocalScale = Vector3.one;
    public virtual Vector3 SoftScale
    {
        get { return mTargetScale; }
        set { mTargetScale = value; }
    }
    public virtual Vector3 HardScale
    {
        get { return mCurrentScale; }
        set { mCurrentScale = mTargetScale = value; }
    }

    Quaternion mCurrentRotation = Quaternion.identity;
    Quaternion mTargetRotation = Quaternion.identity;
    public Quaternion mLocalRotation = Quaternion.identity;
    public virtual float SoftFlatRotation
    {
        get { return mTargetRotation.flat_rotation(); }
        set { mTargetRotation = Quaternion.AngleAxis(value, Vector3.forward); }
    }
    public virtual float HardFlatRotation
    {
        get { return mCurrentRotation.flat_rotation(); }
        set { mCurrentRotation = mTargetRotation = Quaternion.AngleAxis(value, Vector3.forward); }
    }

    Color mCurrentColor;
    Color mTargetColor;
    public Color mLocalColor = new Color(0,0,0,0);
    public virtual Color SoftColor
    {
        get { return mTargetColor; }
        set { mTargetColor = value; }
    }
    public virtual Color HardColor
    {
        get { return mCurrentColor; }
        set { mCurrentColor = mTargetColor = value; }
    }

	//maximum rate of color change in units per second
	public virtual float ColorInterpolationMaxLimit
	{ get; set; }
	//mininum rate of color will change
	public virtual float ColorInterpolationMinLimit
	{ get; set; }

	//TODO this should really be bounds not rect
    public virtual Rect BoundingBox
    {
        get { return new Rect(0, 0, 0, 0); }
    }

    protected int mDepth = 0;
    public virtual int Depth
    {
        get { return mDepth; }
        set
        {
            mDepth = value;
            foreach (Renderer e in PrimaryGameObject.GetComponentsInChildren<Renderer>())
                e.material.renderQueue = mDepth;
        }
    }
	
	
	
    public virtual Shader HardShader
    {
		set
		{
			if (PrimaryGameObject != null)
	        {
	            foreach (Renderer e in PrimaryGameObject.GetComponentsInChildren<Renderer>())
				{
	            	e.material.shader = value;
					e.material.renderQueue = mDepth;
				}
	        }
			//Depth = Depth;
		}
    }
	
    public virtual bool Destroyed {get{return PrimaryGameObject == null;}}

    public virtual void destroy()
    {
		//obviously, don't call this if you want to handle destruction yourself
		GameObject.Destroy(PrimaryGameObject);
    }

    public FlatElementBase()
    {
        SoftInterpolation = 0.08f;
        HardScale = Vector3.one;
		HardColor = new Color(0.5f,0.5f,0.5f,0.5f);
		
		ColorInterpolationMaxLimit = 1f;
		ColorInterpolationMinLimit = 0.04f;

		PositionInterpolationMaxLimit = Mathf.Infinity;
		PositionInterpolationMinLimit = 0;
		
        Events = new TimedEventHandler();
    }



    public virtual void set_position(Vector3 aPos)
    {
        if (PrimaryGameObject != null)
        {
            PrimaryGameObject.transform.position = aPos*GameConstants.SCALE;
        }
    }
    public virtual void set_scale(Vector3 aScale)
    {
        if (PrimaryGameObject != null)
        {
            if(PrimaryGameObject.transform.localScale != aScale)
                PrimaryGameObject.transform.localScale = aScale;
        }
    }
    public virtual void set_rotation(Quaternion aRot)
    {
        if (PrimaryGameObject != null)
        {
            PrimaryGameObject.transform.rotation = aRot;
        }
    }
    public virtual void set_color(Color aColor)
    {
        if (PrimaryGameObject != null)
        {
            foreach (Renderer e in PrimaryGameObject.GetComponentsInChildren<Renderer>())
            {
                try { e.material.SetColor("_TintColor", aColor); }
                catch { }
                try { e.material.color = aColor; }
                catch { }
            }
            /*
            Renderer rend = PrimaryGameObject.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                try { rend.material.SetColor("_TintColor", aColor); }
                catch { }
                try { rend.material.color = aColor; }
                catch { }
            }*/
        }
    }
    

    public void update(float aDeltaTime)
    {
        update_parameters(aDeltaTime);
        set();
    }
    public virtual void update_parameters(float aDeltaTime)
    {
        Events.update(aDeltaTime, this);
		{
	        
	        mCurrentRotation = Quaternion.Slerp(mCurrentRotation, mTargetRotation, SoftInterpolation);
	        mCurrentScale = (1 - SoftInterpolation) * mCurrentScale + SoftInterpolation * mTargetScale;
			

			if(PositionInterpolationMaxLimit < Mathf.Infinity || PositionInterpolationMinLimit > 0)
			{
				float minLimitChange = PositionInterpolationMinLimit * aDeltaTime;
				float maxLimitChange = PositionInterpolationMaxLimit * aDeltaTime;

				Vector3 desiredPosition = (1 - SoftInterpolation) * mCurrentPosition + SoftInterpolation * mTargetPosition;
				float desiredPositionDistance = (desiredPosition-mCurrentPosition).magnitude;
				if((mCurrentPosition-mTargetPosition).magnitude < minLimitChange)
					mCurrentPosition = desiredPosition;
				else if(desiredPositionDistance > 0)
					mCurrentPosition += (desiredPosition-mCurrentPosition) / desiredPositionDistance * Mathf.Clamp(desiredPositionDistance,minLimitChange,maxLimitChange);
			}
			else
				mCurrentPosition = (1 - SoftInterpolation) * mCurrentPosition + SoftInterpolation * mTargetPosition;
			
			
			Vector4 lerpingColorA = new Vector4(mCurrentColor.r, mCurrentColor.g, mCurrentColor.b, mCurrentColor.a);
			Color targetColor = (1 - SoftInterpolation) * mCurrentColor + SoftInterpolation * mTargetColor;
			Vector4 lerpingColorB = new Vector4(targetColor.r, targetColor.g, targetColor.b, targetColor.a);
			Vector4 finalColor = new Vector4(mTargetColor.r, mTargetColor.g, mTargetColor.b, mTargetColor.a);
			Vector4 lerpingColorDir = lerpingColorB - lerpingColorA;
			float lerpingColorDist = lerpingColorDir.magnitude;
			Vector4 actualColor = lerpingColorA;

			float maxChange = ColorInterpolationMaxLimit*aDeltaTime;
			float minChange = ColorInterpolationMinLimit*aDeltaTime;

			if((lerpingColorA - finalColor).magnitude < minChange)
				actualColor = finalColor;
			else if(lerpingColorDist > 0)
				actualColor = lerpingColorA + lerpingColorDir/lerpingColorDist * Mathf.Clamp(lerpingColorDist,minChange,maxChange);

			mCurrentColor = new Color(actualColor.x,actualColor.y,actualColor.z,actualColor.w);
		}
    }

    public virtual void set()
    {
        set_position(mCurrentPosition + mLocalPosition);// + new Vector3(0,0,Depth));
        set_scale(mBaseScale.component_multiply(mCurrentScale).component_multiply(mLocalScale));
        set_rotation(mCurrentRotation * mLocalRotation);
        set_color(mCurrentColor + mLocalColor);
    }

}
