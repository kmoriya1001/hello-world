using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine.VR;
using UnityEngine.UI;


public class PTestScript1 : xmgAugmentedMarker
{
		#region  Variable Declaration
	public float Game_Time,Time_Left,t;
	public Text text;
	public int[] M = {248, 249, 250, 251, 252, 253, 254, 255};
	public int iNbrSceneObject, iNbrOfDetection;
	public int[] Flag, indexPivot;
	Rigidbody Temp;
	public String address,path,data; 
	
	private GameObject FrontDisplay,Trackers;
	public GameObject Freeze_Cube, Pivot_Freeze;
	public float smoothTimeA = 0.1F,DIST=15.0F, smoothTimeS = 1F,zA, distance1, distance2, distance3,timeleft;
	public Vector3 scaling1,scaling2;
	
	public Quaternion[] Current_Rot,Freeze_Rot;
	public Vector3[] Current_Pos, Freeze_Pos;
	public GameObject[] Object_Save,Current_Model, Current_Model_Frozen, Pivots, Frozen,RoomObjects;
	public float[] currentAngle;
	public string[] outp;
	public GameObject Save_zone,G;

	public string Name,ExpNo;
    public float GT;

	[SerializeField]
	public GameObject FROZEN;
	public Camera Orthographic_Shot,Perspective_Shot;
	public TextMesh Save_Zone;
    #endregion

    void Start()
	{
        //VRSettings.enabled = true
        iNbrSceneObject = 8;
        Name = PlayerPrefs.GetString("UserName");
		ExpNo = PlayerPrefs.GetString("ExpNo");
		GT = PlayerPrefs.GetFloat("LeftTime");
		address= PlayerPrefs.GetString("Address");
        #region Resize Arrays
        Array.Resize (ref Flag, iNbrSceneObject);
		Array.Resize (ref RoomObjects,iNbrSceneObject);
		Array.Resize (ref Current_Model,iNbrSceneObject);
		Array.Resize (ref Current_Model_Frozen,iNbrSceneObject);
		Array.Resize (ref Current_Pos,iNbrSceneObject);
		Array.Resize (ref Freeze_Pos,iNbrSceneObject);
		Array.Resize (ref Current_Rot,iNbrSceneObject);		
		Array.Resize (ref Freeze_Rot,iNbrSceneObject);
		Array.Resize (ref indexPivot,iNbrSceneObject+1);
		Array.Resize (ref Object_Save,3);
		Array.Resize (ref outp,iNbrSceneObject+1);
		#endregion

		#region Initialize
		Game_Time=GT;
		Time_Left=Game_Time*60;
		DIST=0.0F;
		Flag = Fill_Array(Flag, -1);
		path = "";
		#endregion

		RoomObjects = GameObject.FindGameObjectsWithTag("RoomObjects").OrderBy(go=>go.name).ToArray();
		Pivots = GameObject.FindGameObjectsWithTag("Trackers").OrderBy(go=>go.name).ToArray();
		Current_Model = Pivots;
		Object_Save = GameObject.FindGameObjectsWithTag("save").OrderBy(go=>go.name).ToArray();
        		
		scaling1 = new Vector3(1F, 1F, 1F);
		scaling2 = GameObject.Find ("WorldPivotExperience").transform.localScale;
                      
    }
	void Update()
	{

		iNbrOfDetection = xmgAugmentedVisionBridge.xzimgMarkerGetNumber();

		#region Timer
		Time_Left -= Time.deltaTime;
		t=(float)Time_Left/60.0F;
		float t1=(t-(int)t)*60.0F;
		//text.text = "     Time " + (int)t+" : "+Math.Round(t1,2)+"  ";
		text.text = " Time " + (int)t+" : "+(int)t1+"  ";

		if(Time_Left <0 && Flag[5]==-1)
		{
			Flag[0]=9;
			Flag[1]=2;
			Flag[3]=-3;
			Flag[5]=99;
		}
		#endregion
		DIST=0.00664F;

		#region Function Call
		Set_Flags();
		Deactivate_Objects(Current_Model,-2);
		Detect_All_Pivot();
		//Attach_To_Pivot();
		//CurrentModel_PositionSet();
		Freeze_Object_Position();
	    Freeze_Model();
		Un_Freeze_Model();
		//Write_TextFile();
      //  go_StartScene();

        #endregion

    }
	#region FILL ARRAY
	int[] Fill_Array(int[] a, int v)
	{
		for (int i = 0; i < a.Length; i++)
		{
			a[i] = v;
		}
		return a;
	}
	#endregion
	#region Deactivate Object
	void Deactivate_Objects(GameObject[] a, int z)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if(z==-2)
				Current_Model[i].gameObject.SetActive (false);
			
		}
	}
	#endregion
	#region Activate Current Detected Objects
	void Detect_All_Pivot()
	{
		if(Flag[7]==-1)
		{
			Fill_Array (indexPivot,-1);
		if (iNbrOfDetection > 0)
		{
				for (int j = 0; j < iNbrOfDetection; j++) {
					xmgMarkerInfo markerInfo = new xmgMarkerInfo ();
					xmgAugmentedVisionBridge.xzimgMarkerGetInfoForUnity (j, ref markerInfo);
					int w = GetPivotIndex (markerInfo.markerID);
					Debug.Log (w);
					int k = System.Array.IndexOf (M, w);
					Debug.Log (k);
					if (w != 170) {
						indexPivot [k] = w;
						Pivots [k].gameObject.SetActive (true);
					} else if (w == 170) {
						indexPivot [8] = 170;
						CurrentModel_PositionSet ();
					}
				}	
			}

		}
	}
	#endregion

	#region Current Model
	void CurrentModel_PositionSet()
	{
		for(int i=0;i<Pivots.Length;i++)	
		{	Current_Pos[i]=Pivots[i].transform.position;
			Current_Rot[i]=Pivots[i].transform.rotation;
		}
		
	}
	#endregion
	#region Obtain Frozen Position
	public void Freeze_Object_Position()
	{
		if(Flag[0]==9||Input.GetKey ("a"))
		{   
			Flag[0]=-9;
			for(int i=0;i<Pivots.Length;i++)	
			{
				Freeze_Pos[i]=Pivots[i].transform.position;
				Freeze_Rot[i]=Pivots[i].transform.rotation;
				}
			if (Input.GetKey ("a")) {
				Flag [1] = 2;
			}
		}
	}
	#endregion
	#region Set Control Flags
	void Set_Flags()
	{
		distance1=(Object_Save[0].transform.position-Save_zone.transform.position).sqrMagnitude;
		distance2=(Object_Save[1].transform.position-Save_zone.transform.position).sqrMagnitude;
		distance3=(Object_Save[2].transform.position-Save_zone.transform.position).sqrMagnitude;

		if(((distance1 <= DIST )||(distance2 <= DIST)||(distance3 <= DIST)) && (Flag[3]==-1))
		{
			Flag[0]=9;
			Flag[1]=2;
			Flag[3]=-3;
		}
	}
	#endregion
	#region Instantiate Freeze all objects
	public void Freeze_Model()
	{		
		GameObject a=GameObject.Find("Fixed Models");

		if(Flag[1]==2)
		{   
			Flag[1]=-2;
			for(int i=0;i<Current_Model.Length;i++)
			{
				Current_Model_Frozen[i] = Instantiate(Current_Model[i], Freeze_Pos[i], Freeze_Rot[i]) as GameObject;
				Current_Model_Frozen[i].transform.parent=a.gameObject.transform;
				a.transform.localScale = scaling2;
				Current_Model_Frozen[i].transform.localScale = scaling1;

			}
			if (Input.GetKey ("a")) {
				
				Flag [7] = -7;
				Deactivate_Objects (Pivots, -2);
			}
			//Flag[4]=-4;
		}
	}

	void Un_Freeze_Model()
	{
		if (Flag [4] != -4 && Input.GetKey ("k") ) {
			Flag [7] = -1;
			foreach (Transform child in FROZEN.transform) {
				GameObject.Destroy (child.gameObject);
			}
		}
	}

    #endregion
	#region Export to File
   

	void Write_TextFile()
	{
		if (Flag [4] == -4)
        {
			Flag [4] = 0;
			path = address + "Experiment_Data.xls";
			Debug.Log (path);
			StreamWriter saveFile = new StreamWriter (path, true);
			GameObject[] transforms =Current_Model_Frozen as GameObject[];	

			saveFile.WriteLine ("\n\t\t Experiment Pilot\t" + ExpNo );
            saveFile.WriteLine("\n\tName :\t"+Name);
            saveFile.WriteLine ("\n\tPositions are X100\tRotation in Degrees\n");
			saveFile.WriteLine ("Room Object\tUsed/Not Used\tPositon x\tPositon y\tPositon z\t Rotation x\t Rotation y\t Rotation z\n");
			try {
				
				foreach (GameObject t in transforms) 
				{
					Vector3 V=t.transform.position*100;
					Vector3 R=t.transform.eulerAngles;

					saveFile.WriteLine (t.transform.name+"\t"+t.activeSelf+"\t"+V.x+"\t"+V.y+"\t"+V.z+"\t"+R.x+"\t"+R.y+"\t"+R.z);
				}
			} catch (System.Exception ex) {
				Debug.Log (ex.Message);
			} finally {
				saveFile.Close ();
			}
			Flag [4] = -55;
			Save_Zone.text="SAVE OK";
			Save_Zone.color = Color.green;
		}
	}
    #endregion
    void go_StartScene()
    {
        if (Flag[4] == -55 && indexPivot[8]==-1)
        {
            Flag[4] = -66;
           // Application.CaptureScreenshot("End_of_Experiment.png");
			ScreenShot(Perspective_Shot,"_P");
			ScreenShot(Orthographic_Shot,"_O");
			UnityEngine.SceneManagement.SceneManager.LoadScene("VR Disabled");

        }
    }
	public void ScreenShot(Camera Screen_Shot, string View_Type)
	{
		
			Flag [6] = -27;
		    //ScreenShotCam.gameObject.GetComponent<Camera> ().enabled=true;
		Camera cam = Screen_Shot;
		Texture2D screenshot = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
		RenderTexture renderTex1 = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
		cam.targetTexture = renderTex1;
		cam.Render();
		RenderTexture.active = renderTex1;
		screenshot.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
		screenshot.Apply(false);
		cam.targetTexture = null;
		RenderTexture.active = null;
		Destroy(renderTex1);

		byte[] bytes = screenshot.EncodeToPNG();
		FileStream fs = new FileStream(address+"Save_"+ExpNo+"_"+Name+"_"+View_Type+".png", FileMode.OpenOrCreate);
		BinaryWriter w = new BinaryWriter(fs);
		w.Write(bytes);
		w.Close();
		fs.Close();

	}
}

