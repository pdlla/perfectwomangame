using UnityEngine;
using System.Collections;

//TODO DELETE 
public class ChoiceObjectPair : FlatElementMultiBase {
    public FlatElementImage mSquare;
    DifficultyObject mDifficultyStars = null;
    DifficultyObject mDifficultyBalls = null;
    public FlatBodyObject mBody = null;
    public MeterImageObject mMeter = null;

    float mSide = 45;
    float mRightBot = -80;

    public ChoiceObjectPair(Texture2D aLeftTex, int aDepth)
    {
        mSquare = new FlatElementImage(aLeftTex, aDepth);
		//NOTE these assets were removed since this class is no longer being used.
        //mDifficultyStars = new DifficultyObject(ManagerManager.Manager.mNewRef.uiPerfectStar, aDepth);
        //mDifficultyBalls = new DifficultyObject(ManagerManager.Manager.mNewRef.uiPerfectStar, aDepth);
		//just to get rid of warnings...

        mElements.Add(new FlatElementMultiBase.ElementOffset(mSquare, new Vector3(mSide, 0, 0)));
        mElements.Add(new FlatElementMultiBase.ElementOffset(mDifficultyStars, new Vector3(-mSide, mRightBot, 0)));
        mElements.Add(new FlatElementMultiBase.ElementOffset(mDifficultyBalls, new Vector3(-mSide, mRightBot, 0)));

        PrimaryGameObject = create_primary_from_elements();
    }

    public ChoiceObjectPair(Texture2D aLeftTex, CharacterTextureBehaviour aChar, Pose aPose, int aDepth)
    {
        mSquare = new FlatElementImage(aLeftTex, aDepth);
		//NOTE these assets were removed since this class is no longer being used.
        //mDifficultyStars = new DifficultyObject(ManagerManager.Manager.mNewRef.uiPerfectStar, aDepth);
        //mDifficultyBalls = new DifficultyObject(ManagerManager.Manager.mNewRef.uiPerfectStar, aDepth);
       
        
        mElements.Add(new FlatElementMultiBase.ElementOffset(mSquare, new Vector3(mSide, 0, 0)));
        mElements.Add(new FlatElementMultiBase.ElementOffset(mDifficultyStars, new Vector3(-mSide, mRightBot, 0)));
        mElements.Add(new FlatElementMultiBase.ElementOffset(mDifficultyBalls, new Vector3(-mSide, mRightBot, 0)));

        mMeter = new MeterImageObject(aLeftTex,null, MeterImageObject.FillStyle.DU, aDepth + 1);
        mMeter.Percentage = 0.0f;
        mBody = new FlatBodyObject(aChar, aDepth + 2);
        mBody.set_target_pose(aPose);
        mElements.Add(new FlatElementMultiBase.ElementOffset(mMeter, new Vector3(mSide,0, 0)));
        mElements.Add(new FlatElementMultiBase.ElementOffset(mBody, new Vector3(mSide + 24, -7, 0)));

        PrimaryGameObject = create_primary_from_elements();
    }

    public void set_pose(Pose aPose)
    {
        if (mBody != null)
            mBody.set_target_pose(aPose);
    }
    public void set_difficulty(int difficulty)
    {
        mDifficultyStars.Enabled = false;
        mDifficultyBalls.Difficulty = difficulty;
    }

    public void set_perfectness(int perfectness)
    {
        mDifficultyBalls.Enabled = false;
        mDifficultyStars.Difficulty = perfectness;
    }

    public void fade_pose(bool aIn)
    {
        if (mBody != null)
            mBody.SoftColor = aIn ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0);
    }
    public override Color SoftColor
    {
        get
        {
            return base.SoftColor;
        }
        set
        {
            mDifficultyBalls.SoftColor = value;
        }
    }

    public override Color HardColor
    {
        get
        {
            return mDifficultyBalls.HardColor;
        }
        set
        {
            mDifficultyBalls.HardColor = value;
        }
    }

    public override void update_parameters(float aDeltaTime)
    {
        base.update_parameters(aDeltaTime);
    }
}
