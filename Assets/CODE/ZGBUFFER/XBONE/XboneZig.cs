using UnityEngine;
using System.Collections.Generic;
using System.Linq;


//TODO you'll want to switch this over to the Unity pulgins eventually

public class MicrosoftZig : ZgInterface
{
    
    ZgManager mZig;
	XboneUsers mUsers;
	XboneKinect mKinect;
	XbonePLM mPLM;
	XboneStorage mStorage;
	XboneEvents mEvents;

	bool Initialized { get; set; }
	
	public void initialize(ZgManager aZig)
	{
        mZig = aZig;
		mPLM = new XbonePLM ();
		mKinect = new XboneKinect ();
		mUsers = new XboneUsers ();
		mStorage = new XboneStorage ();
		mEvents = new XboneEvents ();


		mPLM.Start ();
		mKinect.Start ();
		mUsers.Start ();
		mStorage.Start ();
		mEvents.Start();

		Initialized = true;
	}
	
	public void update()
	{
		mKinect.Update ();

        //TODO update with real depth texture...
        //ManagerManager.Manager.mZigManager.DepthView.Zig_Update (ZgInput);
	}
	
	public bool has_user()
	{
		return mKinect.IsTracking;
	}

	//TODO should check for users
	public bool can_start()
	{
		if(Initialized)
			return true;
		return false;
	}

    public Texture2D take_color_image()
    {
        if(ManagerManager.Manager.mZigManager.is_reader_connected() == 2)
        {
            //TODO update with real textures...
            return null;
            //return ManagerManager.Manager.mZigManager.ImageView.UpdateTexture (ZgImage,ZgLabelMap);
            //Debug.Log ("updated image");
        }
        return null;
    }


	//NOTE calling these assumes mStorage has been properly initialized already... 
	void write_data(byte[] aData, string aName){mStorage.write_data (aData, aName);}
	byte[] read_data(string aName){return mStorage.read_data (aName);}
	
	public ZgDepth DepthImage{get{ return null; }}
	public ZgImage ColorImage{get{ return null; }}
	public ZgLabelMap LabelMap{get{ return null; }}
    public bool ReaderInitialized { get{ return mKinect.IsReaderConnected; } } //TODO maybe return something more useful...
	public bool IsMicrosoftKinectSDK { get{ return true; } }
}


