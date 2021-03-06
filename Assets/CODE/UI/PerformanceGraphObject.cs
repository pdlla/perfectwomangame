using UnityEngine;
using System.Collections;

public class PerformanceGraphObject  : FlatElementMultiBase {
    public FlatRenderTextureGraphElement mBackground;
	public FlatElementImage mForeground;
	GameObject mStupidDot = null;
	public PerformanceGraphObject(int aDepth)
	{
		//NOTE these assets were removed since this class is no longer being used.
		//mBackground = new FlatGraphElement(ManagerManager.Manager.mNewRef.bbGraphBackground,aDepth);	
		//mBackground = new FlatRenderTextureGraphElement(ManagerManager.Manager.mNewRef.bbGraphBackground,aDepth);	
		//mForeground = new FlatElementImage(ManagerManager.Manager.mNewRef.bbGraphFrame,aDepth+1);

		mElements.Add(new FlatElementMultiBase.ElementOffset(mBackground, new Vector3(0,0,0)));
		mElements.Add(new FlatElementMultiBase.ElementOffset(mForeground, new Vector3(0,0,0)));
        PrimaryGameObject = create_primary_from_elements();
		Depth = aDepth;
		

		//NOTE these assets were removed since this class is no longer being used.
		//mStupidDot = ImageGameObjectUtility.create(ManagerManager.Manager.mNewRef.bbGraphDot);
		mStupidDot.GetComponentInChildren<Renderer>().material.color = new Color(0,0,0,1);
		mStupidDot.GetComponentInChildren<Renderer>().material.shader = ManagerManager.Manager.mReferences.mGraphShader;
	}
	
	public void update_graph(float aTime, float aScore)
	{
		float ci = 0.2f;
		float lb = 0.3f;
		float ub = 0.7f;
		//red -> blue -> gold
		Color colorInterp = new Color(1,1,1,1);
		if(aScore < lb + ci)
			colorInterp = Color.Lerp(new Color(1,0,0,1),new Color(0,0,1,1),(Mathf.Clamp(aScore,lb-ci, lb+ci)-(lb-ci))/(ci*2));
		else
			colorInterp = Color.Lerp(new Color(0,0,1,1),new Color(0,1,0,1),(Mathf.Clamp(aScore,ub-ci, ub+ci)-(ub-ci))/(ci*2));
		
		aScore = 0.06f + aScore * (0.71f-0.06f);
		aTime = 0.016f + aTime * (0.986f-0.016f);
		
		
		
		//mBackground.draw_point(new Vector2(aTime,aScore),5,colorInterp);
		
		mBackground.draw_point(new Vector2(aTime,aScore),mStupidDot,colorInterp);
		mStupidDot.GetComponentInChildren<Renderer>().material.color = new Color(0,0,0,1);
	}
}
