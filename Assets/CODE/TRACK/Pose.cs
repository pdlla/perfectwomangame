using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class PoseElement
{
    public ZgJointId joint;
    public float angle;
    public float weight = 1;
    public bool important = true; //this means we care about angle
    //other info if we want it...
}

[System.Serializable]
public class Pose
{
    public List<PoseElement> mElements = new List<PoseElement>();
    public PoseElement find_element(ZgJointId id)
    {
        foreach (PoseElement e in mElements)
            if (e.joint == id)
                return e;
        throw new UnityException("can't find ZigJointId " + id + " in Pose");
    }
	
	public static Pose interpolate(Pose A, Pose B, float lambda)
	{
		Pose r  = new Pose();
		foreach(PoseElement e in A.mElements)
		{
			PoseElement pe = new PoseElement();
			pe.joint = e.joint;
			pe.important = e.important;
			pe.weight = e.weight;
			pe.angle = VectorMathUtilities.MathHelper.interpolate_degrees(e.angle,B.find_element(e.joint).angle,lambda);
			r.mElements.Add(pe);
		}
		
		return r;
	}
	//TODO copy operation
}

[System.Serializable]
public class PoseAnimation
{
	public List<Pose> poses = new List<Pose>();
	public Pose get_pose(int index){ return poses[index % poses.Count]; }
	
	public static PoseAnimation load_from_folder(string aFolder)
	{
        Debug.Log("trying to load poses from folder " + aFolder);
		PoseAnimation r = new PoseAnimation();
		//Debug.Log(Directory.GetFiles(aFolder).Where(e => Path.GetExtension(e) == ".txt").Count());
		foreach(string e in Directory.GetFiles(aFolder).Where(e => System.IO.Path.GetExtension(e) == ".txt"))
		{
			//string text = (new StreamReader(e)).ReadToEnd();
			r.poses.Add(ProGrading.from_file(e));
		}
		return r;
	}

    public void save_to_folder(string aPrefix,string aFolder)
    {
        int i = 1;
        foreach (var e in poses)
        {
            ProGrading.write_pose_to_file(e,aFolder+"/"+aPrefix+"_"+i+".txt");
            i++;
        }
    }

    public PoseAnimation Clone()
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            return (PoseAnimation) formatter.Deserialize(ms);
        }
    }
}

public class PerformanceType
{
	public enum PType
	{
		STATIC,
		SLOW,
		SWITCH,
		SLOWSWITCH,
        MANUAL,
		COUNT = 5
	}
	
	public PType PT
	{ get; set; }
	public float BPM //TODO or DELETE
	{ get; set; }
	public float ChangeTime
	{ get; set; }
	public float Offset 
	{ get; set; }
	protected PoseAnimation PA
	{ get; set; }


    bool mManualAdvance = false;
	public void ManualAdvance()
    {
        mManualAdvance = true;
    }

	public PerformanceType(PoseAnimation aAnim, CharacterIndex aIndex)
	{
		PA = aAnim;
		PT = PType.SWITCH;
		Offset = 0;
		BPM = 0;
		ChangeTime = 4;
		
		/*
		if(aIndex.LevelIndex == 0 || aIndex.LevelIndex == 1 || aIndex.LevelIndex == 7 )
			PT = PType.STATIC;
		if(aIndex.LevelIndex == 2 || aIndex.LevelIndex == 6 )
			PT = PType.SWITCH;
		if(aIndex.LevelIndex == 3 || aIndex.LevelIndex == 5 )
			PT = PType.SLOW;
		if(aIndex.LevelIndex == 4)
			PT = PType.SLOWSWITCH;*/
		
			
	}
	public PerformanceType(PoseAnimation aAnim, PType aType)
	{
		PA = aAnim;
		PT = aType;
		Offset = 0;
		BPM = 0;
		ChangeTime = 4;
		
	}
	

	public void set_change_time(float aTarget)
	{
		if(BPM == 0)
			ChangeTime = aTarget;
		else
		{
			ChangeTime = Mathf.RoundToInt((aTarget/(BPM/60.0f))) * (BPM/60.0f);
		}
	}
	
	//TODO this function may need to change if you change get_pose...
	//really get pose should cache the last returned pose...
	public bool does_pose_change(float aTime, float aDelta)
	{
		aTime = aTime - Offset;
		float changeTime = ChangeTime;
		return ((int)(aTime/changeTime)) != ((int)((aTime-aDelta)/changeTime));
	}
	public bool does_pose_change_precoginitive(float aTime, float aDelta, float aPrecognition)
	{
		aTime = aTime - Offset;
		aTime = aTime + aPrecognition;
		float changeTime = ChangeTime;
		return ((int)(aTime/changeTime)) != ((int)((aTime-aDelta)/changeTime));
	}
	
	public virtual Pose get_pose(float aTime)
	{
		aTime = aTime - Offset;
		float changeTime = ChangeTime;
		if(PT == PType.STATIC)
		{
			if(PA != null && PA.poses.Count != 0)
				return PA.get_pose(0);
			return null;
		}
		else if(PT == PType.SWITCH)
		{
			//want to change once per beat???
			
			int rIndex = ((int)(aTime/changeTime));
			return PA.get_pose(rIndex);
		}
		else if(PT == PType.SLOW)
		{
			//want to change once per beat???
			
			int rIndex = ((int)(aTime/changeTime));
			float lambda = (aTime-(rIndex*changeTime))/changeTime;
			return Pose.interpolate(PA.get_pose(rIndex),PA.get_pose(rIndex + 1),lambda);
		}
		else if(PT == PType.SLOWSWITCH)
		{
			//make sure there are an odd # of poses
			//want to change once per beat???
			
			int rIndex = ((int)(aTime/changeTime));
			float lambda = (aTime-(rIndex*changeTime))/changeTime;
			return Pose.interpolate(PA.get_pose(rIndex*2),PA.get_pose(rIndex*2 + 1),lambda);
		}
        else if(PT == PType.MANUAL)
        {
            //TODO TEST
            if(mManualAdvance)
            {
                mManualAdvance = false;
                aTime += changeTime-aTime%changeTime;
                int rIndex = ((int)(aTime/changeTime));
                return PA.get_pose(rIndex);
            }
        }
		return null;
	}
}
