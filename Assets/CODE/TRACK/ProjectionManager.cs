using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ProjectionManager : FakeMonoBehaviour {
    public ProjectionManager(ManagerManager aManager) : base(aManager)
	{
	}
	
	public class Smoothing
	{
        public float snapInTolerance = 2.0f;
        public float snapOutTolerance = 4.0f;

        public bool snapped = false;
		public float target = 0;
		public float current = 0;

        public bool snap_change(float value, float snapValue, float interp)
        {
            bool r = false;
            float diff = Mathf.Abs(value - snapValue);
            if (diff > 180) diff -= 360;
            diff = Mathf.Abs(diff);
            if (snapped)
            {
                if (diff > snapOutTolerance)
                    snapped = false;
            }
            else
            {
                if (diff < snapInTolerance)
                {
                    snapped = true;
                    current = target = snapValue;
                    r = true;
                }
            }
            if (!snapped)
                target = value;
            else target = snapValue;

            set_current(interp);
            return r;
        }
        public void change(float value, float interp)
        {
            target = value;
            set_current(interp);
        }

        void set_current(float interp)
        {
            //current = interp * current + target * (1 - interp);
            if (current - target > 180)
                target += 360;
            else if (current - target < -180)
                target -= 360;
            current = interp * current + target * (1 - interp);
            if (target < -180)
                target += 360;
            else if (target > 180)
                target -= 360;
        }
	}
	//public Dictionary<GradingManager.WeightedZigJointPair,Smoothing> mImportant = new Dictionary<GradingManager.WeightedZigJointPair, Smoothing>(new GradingManager.WeightedZigJointPairComparer());

    public class Stupid
    {
        public ZgJointId otherEnd;
        public Smoothing smoothing = new Smoothing();
        public float weight = 1;
        public Stupid(ZgJointId other) { otherEnd = other; }
    }
    public Dictionary<ZgJointId, Stupid> mImportant = new Dictionary<ZgJointId, Stupid>();
	public override void Start () 
    {
        mImportant[ZgJointId.LeftShoulder] = new Stupid(ZgJointId.LeftElbow);
        mImportant[ZgJointId.LeftElbow] = new Stupid(ZgJointId.LeftHand);
        mImportant[ZgJointId.LeftHip] = new Stupid(ZgJointId.LeftKnee);
        mImportant[ZgJointId.LeftKnee] = new Stupid(ZgJointId.LeftAnkle);
        mImportant[ZgJointId.RightShoulder] = new Stupid(ZgJointId.RightElbow);
        mImportant[ZgJointId.RightElbow] = new Stupid(ZgJointId.RightHand);
        mImportant[ZgJointId.RightHip] = new Stupid(ZgJointId.RightKnee);
        mImportant[ZgJointId.RightKnee] = new Stupid(ZgJointId.RightAnkle);
        mImportant[ZgJointId.Neck] = new Stupid(ZgJointId.Head);
        mImportant[ZgJointId.Torso] = new Stupid(ZgJointId.Neck);
	}
    public Smoothing mWaist = new Smoothing();
	public Vector3 mNormal = Vector3.forward;
	public Vector3 mUp = Vector3.up;
    public float mSmoothing = 0.5f;
	
	public float get_smoothed_relative(ZgJointId A, ZgJointId B)
	{
		return mImportant[A].smoothing.current;
	}
	public float get_relative(ZgInputJoint A, ZgInputJoint B)
	{
		float r = 0;
		if(A.Id == ZgJointId.None)
			return 0;
		if(!B.GoodPosition)
		{
			if(B.Id == ZgJointId.LeftElbow 
			   || B.Id == ZgJointId.RightElbow )
			   //|| B.Id == ZigJointId.LeftHand 
			   //|| B.Id == ZigJointId.RightHand)
			{
				r = mImportant[A.Id].smoothing.current;
			}
			else
				r = -A.Rotation.flat_rotation() + 90;
		}
		else
        	r = get_relative(A.Position, B.Position);


		//openni fix to solve head being set to -90 angle problem
		if(B.Id == ZgJointId.Head)
			if(!B.GoodPosition)
				r = -90;



		if(B.Id == ZgJointId.LeftHand)
			if(!B.GoodPosition)
				r = mImportant[ZgJointId.LeftShoulder].smoothing.current;

		if(B.Id == ZgJointId.RightHand)
			if(!B.GoodPosition)
				r = mImportant[ZgJointId.RightShoulder].smoothing.current;

        //double the waist angle on XB1 only because waist is not as sensitive on kinect2.0
        if (B.Id == ZgJointId.Neck && GameConstants.XB1)
            r = -90 + (r+90)*2;

        return r;
	}

    public float get_waist(ZgInputJoint waist, ZgInputJoint L, ZgInputJoint R)
    {

		float r = 0;
        //TODO some problems with this lockngi... Should default to below if that happens
		//if(!mManager.mZigManager.using_nite())
            //r = -waist.Rotation.flat_rotation() + 90;    
        //else
            r = get_relative(waist.Position, L.Position * 0.5f + R.Position * 0.5f);

		return r;
    }

    public float get_relative(Vector3 A, Vector3 B)
    {
        Vector3 right = Vector3.Cross(mUp, mNormal);
        Vector3 v = B - A;
       
        Vector3 projected = Vector3.Exclude(mNormal, v);
        float r = Vector3.Angle(right, projected);
        if (Vector3.Dot(Vector3.Cross(right, projected), mNormal) < 0)
        {
            r *= -1;
        }
        return -r;
    }

    /*
    Dictionary<ZigJointId, GameObject> mDebugCharacter = new Dictionary<ZigJointId, GameObject>();
    public static ZigJointId[] mFullJoints = { ZigJointId.Neck, ZigJointId.LeftElbow, ZigJointId.LeftKnee, ZigJointId.LeftShoulder, ZigJointId.LeftHip, ZigJointId.RightElbow, ZigJointId.RightKnee, ZigJointId.RightShoulder, ZigJointId.RightHip, ZigJointId.Torso};
    public static ZigJointId[] mStubJoints = { ZigJointId.LeftHand, ZigJointId.RightHand, ZigJointId.LeftAnkle, ZigJointId.RightAnkle };
    public void create_debug_character()
    {
        GameObject parent = new GameObject("DEBUG_CHARACTER_PARENT");
        foreach (ZigJointId e in mFullJoints)
        {
            GameObject j = (GameObject)GameObject.Instantiate(mManager.mReferences.mDebugLimb);
            j.transform.parent = parent.transform;
            j.transform.localScale = new Vector3(30, 30, 30);
            mDebugCharacter[e] = j;
        }
        foreach (ZigJointId e in mStubJoints)
        {
            GameObject j = (GameObject)GameObject.Instantiate(mManager.mReferences.mDebugLimb);
            GameObject.Destroy(j.transform.FindChild("Cylinder"));
            j.transform.parent = parent.transform;
            j.transform.localScale = new Vector3(30, 30, 30);
            mDebugCharacter[e] = j;
        }
    }
    public void update_debug_character()
    {
        foreach (var e in mDebugCharacter)
        {
            e.Value.transform.position = mManager.mZigManager.Joints[e.Key].Position;
            e.Value.transform.rotation = mManager.mZigManager.Joints[e.Key].Rotation;
        }
    }*/

	public override void Update () {
        if (mManager.mZigManager.has_user())
        {
			//Pose targetPose = mManager.mGameManager.CurrentTargetPose;
			//Pose currentPose = mManager.mGameManager.CurrentPose; //this may have one frame of lag but oh well
			
            foreach (KeyValuePair<ZgJointId,Stupid> e in mImportant)
            {
                if (e.Key != ZgJointId.None && e.Key != ZgJointId.Waist)
                {
                    //ZgJointId parentJoint = BodyManager.get_parent(e.Key);
                    try
                    {
                        /* note, if you do snapping, you need to turn it off for baby
                        if (parentJoint == ZigJointId.None || (parentJoint == ZigJointId.Waist) || mImportant[parentJoint].smoothing.snapped == true)
                        {
                            if(e.Value.smoothing.snap_change(get_relative(mManager.mZigManager.Joints[e.Key], mManager.mZigManager.Joints[e.Value.otherEnd]), mManager.mTransparentBodyManager.mFlat.mTargetPose.find_element(e.Key).angle, mSmoothing))
                                ;// Debug.Log("SNAP " + e.Key);
                        }
                        else*/
                        {
                            e.Value.smoothing.change(get_relative(mManager.mZigManager.Joints[e.Key], mManager.mZigManager.Joints[e.Value.otherEnd]), mSmoothing);
                            
                        }
                    }
                    catch
                    {
                        //TODO wyh does this fail on first run?
                    }
                }
            }
            try
            {
				
				float waistAngle = get_waist(mManager.mZigManager.Joints[ZgJointId.Torso], mManager.mZigManager.Joints[ZgJointId.LeftKnee], mManager.mZigManager.Joints[ZgJointId.RightKnee]);
				//waist smoothing angle hack
				/*
				if(!mManager.mZigManager.using_nite() && targetPose != null)
				{
					float interp = Mathf.Clamp01(
						(ProGrading.grade_joint(currentPose,targetPose,ZigJointId.LeftHip) + 
						ProGrading.grade_joint(currentPose,targetPose,ZigJointId.RightHip))/100f);
					waistAngle = targetPose.find_element(ZigJointId.Waist).angle * (1-interp) + waistAngle * interp;
				}*/
				mWaist.change(waistAngle,mSmoothing);
				
                //mWaist.snap_change(get_waist(mManager.mZigManager.Joints[ZigJointId.Waist], mManager.mZigManager.Joints[ZigJointId.LeftKnee], mManager.mZigManager.Joints[ZigJointId.RightKnee]), mManager.mTransparentBodyManager.mTargetPose.find_element(ZigJointId.Waist).angle, mSmoothing);
                //mWaist.change(get_waist(mManager.mZigManager.Joints[ZigJointId.Waist], mManager.mZigManager.Joints[ZigJointId.LeftKnee], mManager.mZigManager.Joints[ZigJointId.RightKnee]), mSmoothing);
                //mWaist.target = get_waist(mManager.mZigManager.Joints[ZigJointId.Waist], mManager.mZigManager.Joints[ZigJointId.LeftHip], mManager.mZigManager.Joints[ZigJointId.RightHip]);
            }
            catch
            {
            }
        }
	}
}
