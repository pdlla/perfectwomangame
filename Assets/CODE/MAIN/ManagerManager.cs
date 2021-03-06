using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

public class ManagerManager : MonoBehaviour{

	//TODO move to gameconstants
	//public const float FORCED_ASPECT_RATIO = 16/10f;
	public static float FORCED_ASPECT_RATIO = 16/9f;
	public static float DESIRED_SCENE_HEIGHT = 1800*GameConstants.SCALE;


    static ManagerManager sManager = null;
    public static ManagerManager Manager
    {
        get { return sManager; }
        private set { sManager = value; }
    }

    //debug nonsense
	//TODO DELET
    public bool mRecordMode = false;
	public bool mSimianMode = false;
	
    //constants
    public static int MANIPULATOR_LAYER = 8;
	
	//delegate stuff
	public delegate void VoidDelegate();
	HashSet<FakeMonoBehaviour> mScripts = new HashSet<FakeMonoBehaviour>();
	VoidDelegate mStartDelegates = null;
	public VoidDelegate mUpdateDelegates = null;
	VoidDelegate mFixedUpdateDelegates = null;

    public ZgManager mZigManager;
	public ProjectionManager mProjectionManager;
    public BodyManager mBodyManager;
    public BodyManager mTransparentBodyManager;
    public BackgroundManager mBackgroundManager;
    public CameraManager mCameraManager;
    public AssetBundleLoader mAssetLoader;
    public NewGameManager mGameManager;
	public TransitionCameraManager mTransitionCameraManager;
	public CharacterBundleManager mCharacterBundleManager;
	public MusicManager mMusicManager;
	public MetaManager mMetaManager;
	
    public PrefabReferenceBehaviour mReferences;
    //public MenuReferenceBehaviour mMenuReferences;
	public NewMenuReferenceBehaviour mNewRef;

    public System.Action<string,object[]> GameEventDistributor { get; set; }

	void Awake () {
		#if !UNITY_XBOXONE
		string words = "";
		using (StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/language.txt"))
		{
			words = reader.ReadLine();
			GameConstants.language = System.Convert.ToInt32(words);
		}
		#endif

		Random.seed = System.Environment.TickCount;
		Application.targetFrameRate = (int)GameConstants.TARGET_FRAMERATE;
        GameEventDistributor += delegate(string arg1, object[] arg2) { Log("ManagerManager.cs: GAME EVENT: " + arg1); };

		Cursor.visible = false;
		gameObject.AddComponent<AudioListener>();
		
        //Debug.Log("setting up managers");
        mReferences = GetComponent<PrefabReferenceBehaviour>();
        //mMenuReferences = GetComponent<MenuReferenceBehaviour>();
		mNewRef = GetComponent<NewMenuReferenceBehaviour>();
        
        Manager = this;

		mCharacterBundleManager = new CharacterBundleManager(this);
		mMusicManager = new MusicManager(this);
		mZigManager = new ZgManager(this);
		mProjectionManager = new ProjectionManager(this);
        mBodyManager = new BodyManager(this);
        mTransparentBodyManager = new BodyManager(this);
		mTransparentBodyManager.mMode = 1; //nasty
        mBackgroundManager = new BackgroundManager(this);
        mCameraManager = new CameraManager(this);
        mAssetLoader = new AssetBundleLoader(this);
        mGameManager = new NewGameManager(this);
		mTransitionCameraManager = new TransitionCameraManager(this);
		mMetaManager = new MetaManager(this);
		
		if (mStartDelegates != null)
        {
            /* CAN DELETE
            var time = Time.time;
            string output = "";
            foreach(var e in mStartDelegates.GetInvocationList())
            {
                e.DynamicInvoke();
                output += e.ToString() + " took: " + (Time.time - time) + "\n";
                time = Time.time;
            }
            Debug.Log(output);*/

            mStartDelegates();
        }

        ManagerManager.Log("ManagerManager.cs: AWAKE() COMPLETE");
	}
	

	
	
	
	public void register_FakeMonoBehaviour(FakeMonoBehaviour aScript)
	{
		mScripts.Add(aScript);

		if(aScript.is_method_overridden("Start"))
			mStartDelegates += aScript.Start;
		if(aScript.is_method_overridden("Update"))
			mUpdateDelegates += aScript.Update;
		if(aScript.is_method_overridden("FixedUpdate"))
			mFixedUpdateDelegates += aScript.FixedUpdate;
	}
	
	public void deregister_FakeMonoBehaviour(FakeMonoBehaviour aScript)
	{
		mScripts.Add(aScript);
		if(aScript.is_method_overridden("Start"))
			mStartDelegates -= aScript.Start;
		if(aScript.is_method_overridden("Update"))
			mUpdateDelegates -= aScript.Update;
		if(aScript.is_method_overridden("FixedUpdate"))
			mFixedUpdateDelegates -= aScript.FixedUpdate;
	}
		
	//for screen resolution callback
	Vector2 mLastScreenSize = new Vector2();
	void Update () {

        //try{
    		Vector2 newScreenSize = new Vector2(Screen.width,Screen.height);
    		if(mLastScreenSize != newScreenSize)
    		{
    			//TODO screen sized changed callback
    		}
    		mLastScreenSize = newScreenSize;
    		
    		if(mUpdateDelegates != null) 
    			mUpdateDelegates();

            if (KeyMan.GetKeyDown("Quit"))
            {
                Application.Quit();
            }
            if (KeyMan.GetKeyDown("Restart"))
            {
                restart_game();
            }

            //mDebugString = ((int)(1 / Time.deltaTime)).ToString();
        //} 
        /*
        catch(System.Exception e)
        {
            mDebugString2 = e.ToString();
            throw e;
        }*/
	}
	
	void FixedUpdate() {

		if(mFixedUpdateDelegates != null) mFixedUpdateDelegates();
	}
	
    public void restart_game()
    {
        if (mGameManager.GS != NewGameManager.GameState.MENU)
        {
            mBackgroundManager.unload();
            mBodyManager.unload();
            mTransparentBodyManager.unload();
            mBodyManager.unload();
            mMusicManager.unload();
            mGameManager.mModeNormalPlay.reset();
            mGameManager.GS = NewGameManager.GameState.NONE;

            //Debug.Log("unloaded current character");

            mCharacterBundleManager.cleanup();
            //Resources.UnloadUnusedAssets();
            //System.GC.Collect();

            //Debug.Log("GCed assets");

            //mTransitionCameraManager.go_to_play(); 
            mTransitionCameraManager.reload();

            ManagerManager.Log("restarted");

            //HARD RESET WAY
            //Application.LoadLevel(Application.loadedLevel);
        }
    }

    //Screenshot nonsense
    //TODO move this nonsense into its own class
    static int sScreenShotNumber = 0;
    public static string sScreenShotPrefix = "char";
    public static bool sTakeManual = true;
    public static bool sTakeKinect = true;
    public static string sFolderPrefix = "Assets/StreamingAssets/POSE_TESTING/";
    public static string sImageFolderPrefix = "";
    public static string ScreenShotName { get { return sScreenShotPrefix + sScreenShotNumber; } }
    
    public void take_screenshot(string filename, Camera cam)
    {
        int resWidth = 800;
        int resHeight = 450;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        CameraClearFlags ccf = cam.clearFlags;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1,1,1,0);
        //cam.DoClear();
        cam.Render();
        cam.clearFlags = ccf;
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
        //Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    void LateUpdate()
    {
		/*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Camera cam = mCameraManager.MainBodyCamera;
            string filename = sImageFolderPrefix + ScreenShotName + "_k.png";
            if (sTakeKinect)
            {
                take_screenshot(filename, cam);
                mBodyManager.write_pose(sFolderPrefix + ScreenShotName + "_k.txt", false);
            }
            cam = mCameraManager.TransparentBodyCamera;
            filename = sImageFolderPrefix + ScreenShotName + "_m.png";
            if (sTakeManual)
            {
                take_screenshot(filename, cam);
                mTransparentBodyManager.write_pose(sFolderPrefix + ScreenShotName + "_m.txt", true);
            }
            sScreenShotNumber++;
        }*/
    }
	

    public static List<string> mDebugMessages = new List<string>();
    public static void Log(string aMsg)
    {
        mDebugMessages.Add(aMsg);
    }


      
	public string mDebugString = "";//"WORK IN PROGRESS";
    public string mDebugString2 = "";
	void OnGUI()
    {
        if (GameConstants.SHOW_DEBUG)
        {
            GUI.depth = int.MinValue;
            //GUI.Box(new Rect(20, 20, 80,30), mDebugString);
            GUIStyle style = new GUIStyle();

            //GUI.Box(new Rect(0,0,Screen.width,Screen.height),MainRenderTexture,style);
            style.fontSize = 15;
            style.normal.textColor = new Color(1, 1, 1, 1);
            GUI.TextArea(new Rect(1, 1, 300, 70), mDebugString, style);
            GUI.TextArea(new Rect(500, 1, 300, 70), mDebugString2, style);
            //GUI.TextArea(new Rect(50,50,300,100),"WORK IN PROGRESS",style);

            //GUI.Box(new Rect(0,0,Screen.width * mGameManager.mModeNormalPlay.mLastGrade,50),""); 

            int heightCounter = 25;
            foreach (var e in mDebugMessages.Reverse<string>().Select((val, index) => new { val, index }))
            {
                var height = style.CalcHeight(new GUIContent(e.val), Screen.width);
                GUI.TextArea(new Rect(10, heightCounter, Screen.width, (int)height), e.val, style);
                heightCounter += (int)height + 2;

                if (heightCounter > Screen.height - 40)
                    break;
            }


            //for debugging
            //mZigManager.DepthView.OnGUI ();
            //mZigManager.ImageView.OnGUI();

        }
    }


    void OnApplicationQuit() {
        GameEventDistributor("TERMINATE",null);
    }
	
}
