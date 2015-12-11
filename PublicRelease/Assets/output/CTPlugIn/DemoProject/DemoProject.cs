///--------------------------------------------------------------------------------------
// File: DemoProject.cs
//
// Developed by Reallusion Developer team.
//
// Copyright © 2014 Reallusion Inc. All rights reserved.
//--------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class DemoProject : MonoBehaviour {
	public GUISkin ctSkin = null;
	
	private const int ACTOR_COUNT = 3;	//Max actor count
	private const int ANIM_COUNT = 6;	//Max animation count of each actor
	private const int IDLE_COUNT = 3;	//MAx idle motion count of each actor
	private int itemStart = 75;
	private int itemAdd = 102;

	public GameObject[] actors = null;
	private CTPluginAPI[] actorAPIs = null;
	private ArrayList actorAnims = new ArrayList();
	
	private int curSel = -1;
	private bool showActor = true;
	private float showDuration = 1.0f;
	
	private string[] curAnims = null;
	private int curPlayAnim = -1;
	private bool loopPlay = false;
	
	private string[] curIdles = null;
	private int curPlayIdle = 0;
	
	private CT.LookAtMode lookatMode = CT.LookAtMode.FullScreen; //fullscreen mode
	private float lookatStrength = 1.0f;
	private float lookatDelay = 0.2f;
	public GameObject lookatTarget = null;	//tracking target
	
	private int curPositionX = 17;
	private int curPositionY = 17;
	private float curScale = 1.0f;
	
	private float actorScale = 1.0f;
	private float actorTransDuration = 1.0f;
	private int gazeDegree = 90;
	private int gazeRadius = 100;
	private float gazeDuration = 1.0f;
	
	private string strPlay = "";
	private Vector2 scrollPosition = Vector2.zero;

	// Use this for initialization
	void Start () {
		ArrayList tmpList = new ArrayList();
		if ( actors.Length > 0 ) {
			actorAPIs = new CTPluginAPI[actors.Length];
			for ( int i = 0; i < actors.Length; ++i ) {
				if ( i > ACTOR_COUNT ) break;
				actorAPIs[i] = actors[i].GetComponent( "CTPluginAPI" ) as CTPluginAPI;
				tmpList.Clear();
				foreach ( AnimationState state in actors[i].GetComponent<Animation>() ) {
					tmpList.Add( state.name );
				}
				actorAnims.Add ( (string[])tmpList.ToArray(typeof(string)) );
				actorAPIs[i].ShowActor( false, 0.0f, 0.0f );
			}
			onSelActorChange( 0 );
		}	
	}
	
	void onSelActorChange( int actorIndex ) {
		if ( ( curSel != -1 ) && ( actorIndex != curSel ) ) {
			StopAllMotions();
			StopAllIdles();
		}
		for ( int i = 0; i < actors.Length; ++i ) {
			if ( i == actorIndex ) {
				actorAPIs[i].playStateChanged = NotifyPlayState;
				if ( lookatMode == CT.LookAtMode.TargetTracking )
					actorAPIs[i].SetLookAtMode( lookatMode, lookatTarget );
				else
					actorAPIs[i].SetLookAtMode( lookatMode );
				actorAPIs[i].SetLookAtStrength( lookatStrength );				
				actorAPIs[i].ShowActor( true, 0.0f, showDuration );
				lookatTarget.transform.parent = actors[i].transform;
				showActor = true;
			} else if ( i == curSel ) {
				actorAPIs[i].playStateChanged = null;
				if ( actors[i].activeSelf ) {
					actorAPIs[i].ShowActor( false, 0.0f, showDuration );
				}
			}
		}	
		curSel = actorIndex;

		// initialize curAnims and curIdles of the current actor
		ArrayList aryAnim = new ArrayList();
		ArrayList aryIdle = new ArrayList();
		foreach ( string clip  in ( actorAnims[curSel] as string[] ) ) {
			if ( aryAnim.Count < ANIM_COUNT ) {
				aryAnim.Add( clip );
			} else {		
				aryIdle.Add( clip );
				if ( aryIdle.Count >= IDLE_COUNT ) break;
			}
		}
		curAnims = (string[])aryAnim.ToArray (typeof(string));
		curIdles = (string[])aryIdle.ToArray (typeof(string));
		if ( curPlayIdle != -1 )
			actorAPIs[curSel].SetIdleMotion( curIdles[curPlayIdle] );
	}

	void OnGUI() {
		int widthPanel = itemStart+itemAdd*3+15;
		GUILayout.BeginArea(new Rect(Screen.width-widthPanel, 0, widthPanel, Screen.height), "");
		
		scrollPosition = GUI.BeginScrollView(new Rect(0, 0, widthPanel, Screen.height), scrollPosition, new Rect(0, 0, widthPanel-20, 685));
		GUI.Box( new Rect(0, 0, widthPanel, (Screen.height>685)?Screen.height:685), "CrazyTalk Function Demo" );
		OnGUIActors();
		if ( ( actors.Length >= 1 ) && ( curSel >= 0 ) && ( curSel < actors.Length ) ) {
			try {
				OnGUIPerforms();
				OnGUIIdles();	
				OnGUILookAt();
				OnGUIOrigin();
				OnGUITransform();
				OnGUIGaze();
			} catch(UnityException ex) {
				Debug.Log( ex.Message );
			}
		}
		GUI.EndScrollView();
		GUILayout.EndArea();	
		OnGUIStatus();	
	}

	void OnGUIActors() {
		int posX = itemStart;
		int posY = 25;
		int selChanged = -1;
		float[] arySec = {0.2f, 1.0f, 2.0f};
		for ( int i = 0; i < actors.Length; ++i ) {
			if ( GUI.Button( new Rect(posX, posY, 100, 25), actors[i].name, ctSkin.customStyles[(curSel==i)?1:2] ) ) {
				selChanged = i;
			}
			posX += itemAdd;
		}
		showActor = GUI.Toggle( new Rect(itemStart, posY+34, 14, 14), showActor, "", ctSkin.customStyles[0] );
		if ( GUI.changed ) {
			actorAPIs[curSel].ShowActor( showActor, 0.0f, showDuration );
		}
		GUI.Label ( new Rect(itemStart+20, posY+30, 100, 30), "Show/Hide");	
		if (SystemInfo.supportsStencil != 1) {
			GUI.enabled = false;
			GUI.Label( new Rect(itemStart+210, posY+55, 100, 30), "(Unity Pro Only)");	
		}
		GUI.Label( new Rect(itemStart, posY+55, 100, 30), "Duration(Sec):");	
		var srtPos = itemStart+itemAdd;
		for ( int i=0; i<arySec.Length; ++i ) {
			if ( GUI.Button( new Rect(srtPos, posY+55, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(showDuration==arySec[i])?1:2] )){
				showDuration = arySec[i];
			}
			srtPos += 34;
		}
		GUI.enabled = true;
		if ( selChanged != -1 ) {
			// Actor change
			onSelActorChange( selChanged );		
		}
		GUI.Label ( new Rect (20, 25, 50, 30), "Actor:");	
	}

	void OnGUIPerforms() {
		int posX = itemStart;
		int posY = 115;
		int valueChange = -1;;
		
		if (curPlayAnim!=-1) GUI.enabled = false;
		loopPlay = GUI.Toggle( new Rect(itemStart, posY+4, 14, 14), loopPlay, "", ctSkin.customStyles[0] );
		GUI.enabled = true;
		GUI.Label ( new Rect(itemStart+20, posY, 50, 30), "Loop");	
		
		int curY = posY + 25;
		for ( int i = 0; i < curAnims.Length; ++i ) {
			if ( GUI.Button( new Rect(posX, curY, 100, 25), curAnims[i], ctSkin.customStyles[(curPlayAnim==i)?1:2] ) ) {
				valueChange = i;
			}
			posX += itemAdd;
			if ( 0 == (i+1)%3 ) {
				posX = itemStart;
				curY += 27;
			}
		}
		if ( valueChange != -1 ) {
			if ( curPlayAnim != valueChange ) {	//from false to true -> play
				// stop all others
				StopAllMotions();
				// play current
				actorAPIs[curSel].Play( curAnims[valueChange], loopPlay, 0.0f );
				curPlayAnim = valueChange;
			} else {
				actorAPIs[curSel].Stop( 0.0f );
				curPlayAnim = -1;
			}
		}
		GUI.Label ( new Rect(20, posY, 50, 30), "Motion:");	
	}

	void OnGUIIdles() {
		int posY = 205;
		int posX = itemStart;
		int valueChange = -1;
		for ( int i = 0; i < curIdles.Length; ++i ) {
			if ( GUI.Button( new Rect(posX, posY, 100, 25), curIdles[i], ctSkin.customStyles[(curPlayIdle==i)?1:2] ) ) {
				valueChange = i;
			}
			posX += itemAdd;
		}
		if ( valueChange != -1 ) {
			if ( curPlayIdle != valueChange ) {	//from false to true -> play
				// stop all others
				StopAllMotions();			
				StopAllIdles();
				// play current
				actorAPIs[curSel].SetIdleMotion( curIdles[valueChange] );
				curPlayIdle = valueChange;
			} else {	// from true to false -> stop
				actorAPIs[curSel].SetIdleMotion( null );
				curPlayIdle = -1;
			}
		}
		GUI.Label ( new Rect (20, posY, 50, 30), "Idle:");	
	}

	void OnGUILookAt() {
		int posY = 245;
		string[] aryLookat = {"Game View", "Full Screen","Target Tracking"};
		CT.LookAtMode[] aryLookatMode = {CT.LookAtMode.GameView, CT.LookAtMode.FullScreen, CT.LookAtMode.TargetTracking};
		int posX = itemStart;
		Component halo;
		float value = lookatStrength;
		for ( int i = 0; i < aryLookat.Length; ++i ) {
			if ( GUI.Button( new Rect( posX, posY, 100, 25 ), aryLookat[i], ctSkin.customStyles[(lookatMode == aryLookatMode[i])?1:2] ) ) {
				if ( lookatMode == aryLookatMode[i] ) {
					lookatMode = CT.LookAtMode.Off;
				} else {
					lookatMode = aryLookatMode[i];
				}
				if ( lookatMode == CT.LookAtMode.TargetTracking ) {
					actorAPIs[curSel].SetLookAtMode(lookatMode, lookatTarget);
					if ( lookatTarget ) {
						halo = lookatTarget.GetComponent("Halo");
						halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
					}
				} else {
					actorAPIs[curSel].SetLookAtMode(lookatMode);
					if ( lookatTarget ) {
						halo = lookatTarget.GetComponent("Halo");
						halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
					}
				}
			}
			posX += itemAdd; 
		}
		GUI.Label ( new Rect (75, posY+30, 70, 30), "Strength:");	
		GUI.Label ( new Rect (368, posY+30, 20, 30), "x");		
		value = float.Parse( GUI.TextArea( new Rect(335, posY+32, 30, 18), lookatStrength.ToString() ) );	
		if ( value != lookatStrength ) {
			if ( value > 2.0f ) value = 2.0f;
			if ( value < 0.0f ) value = 0.0f;		
			lookatStrength = value;	
			actorAPIs[curSel].SetLookAtStrength( value );	
		}
		value = GUI.HorizontalSlider( new Rect(140, posY+37, 190, 20), lookatStrength, 0.0f, 2.0f);
		if ( value != lookatStrength ) {
			lookatStrength = value;
			actorAPIs[curSel].SetLookAtStrength( value );	
		}
		GUI.Label ( new Rect (75, posY+55, 70, 30), "Delay:");	
		GUI.Label ( new Rect (368, posY+55, 22, 30), "s");		
		value = float.Parse( GUI.TextArea( new Rect(335, posY+57, 30, 18), lookatDelay.ToString() ) );	
		if ( value != lookatDelay ) {
			if ( value > 1.0f ) value = 1.0f;
			if ( value < 0.0f ) value = 0.0f;		
			lookatDelay = value;	
			actorAPIs[curSel].SetLookAtDelay( lookatDelay );	
		}
		value = GUI.HorizontalSlider( new Rect(140, posY+62, 190, 20), lookatDelay, 0.0f, 1.0f);
		if ( value != lookatDelay ) {
			lookatDelay = value;
			actorAPIs[curSel].SetLookAtDelay( value );	
		}	
		GUI.Label ( new Rect (20, posY, 50, 30), "LookAt:");	
	}

	void OnGUIOrigin() {
		int value;	
		float valueF;
		int posY = 355;
		
		GUI.Label ( new Rect (75, posY, 70, 30), "X:");	
		GUI.Label ( new Rect (368, posY, 20, 30), "%");			
		value = int.Parse( GUI.TextArea( new Rect(335, posY+2, 30, 18), curPositionX.ToString() ) );		
		if ( value != curPositionX ) {
			if ( value > 150 ) value = 150;
			if ( value < -50 ) value = -50;		
			curPositionX = value;
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		value = (int)GUI.HorizontalSlider( new Rect(140, posY+7, 190, 20), curPositionX, -50, 150);
		if ( value != curPositionX ) {
			curPositionX = value;
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		GUI.Label ( new Rect (75, posY+25, 70, 30), "Y:");
		GUI.Label ( new Rect (368, posY+25, 20, 30), "%");	
		value = int.Parse( GUI.TextArea( new Rect(335, posY+27, 30, 18), curPositionY.ToString() ) );	
		if ( value != curPositionY ) {
			if ( value > 150 ) value = 150;
			if ( value < -50 ) value = -50;		
			curPositionY = value;
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		value = (int)GUI.HorizontalSlider( new Rect(140, posY+32, 190, 20), curPositionY, -50, 150);
		if ( value != curPositionY ) {
			curPositionY = value;
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		GUI.Label ( new Rect (75, posY+50, 70, 30), "Scale:");	
		GUI.Label ( new Rect (368, posY+50, 20, 30), "x");		
		valueF = float.Parse( GUI.TextArea( new Rect(335, posY+52, 30, 18), curScale.ToString() ) );	
		if ( valueF != curScale ) {
			if ( valueF > 3.0f ) valueF = 3.0f;
			if ( valueF < 0.01f ) valueF = 0.01f;			
			curScale = valueF;	
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		valueF = GUI.HorizontalSlider( new Rect(140, posY+57, 190, 20), curScale, 0.01f, 3.0f);
		if ( valueF != curScale ) {
			curScale = valueF;	
			ActorTransform( new Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
		}
		if ( GUI.Button( new Rect( itemStart+itemAdd*2, posY+75, 100, 25 ), "Reset" )) {
			ActorTransform( new Vector2(17, 17), 1.0f, 0, 0.0f );
		}
		GUI.Label( new Rect (20, posY-25, 200, 30), "Initial Position:");	
	}
	void OnGUITransform() {
		int posY = 435;
		string[] aryTrans = {"Left Bottom", "Right Bottom", "Left Up", "Center"};
		Vector2[] aryTransPos = new Vector2[ 4 ];
		aryTransPos [0] = new Vector2(36, posY+85);
		aryTransPos [1] = new Vector2(122, posY+85);
		aryTransPos [2] = new Vector2(208, posY+85);
		aryTransPos [3] = new Vector2(294, posY+85);
		Vector2[] aryValuePos = new Vector2[ 4 ];
		aryValuePos [0] = new Vector2 (17, 17);
		aryValuePos [1] = new Vector2 (84, 17);
		aryValuePos [2] = new Vector2 (17, 84);
		aryValuePos [3] = new Vector2 (50, 50);
		float[] arySec = {0.2f, 1.0f, 2.0f};
		
		for ( int i=0; i<aryTrans.Length; ++i) {
			if ( GUI.Button( new Rect(aryTransPos[i].x, aryTransPos[i].y, 86, 25), aryTrans[i]/*, ctSkin.customStyles[2]*/ ) ) {
				ActorTransform( aryValuePos[i], actorScale, 0, actorTransDuration );
			}
		}
		GUI.Label ( new Rect (75, posY+25, 70, 30), "Scale:");	
		GUI.Label ( new Rect (368, posY+25, 20, 30), "x");		
		actorScale = float.Parse( GUI.TextArea( new Rect(335, posY+27, 30, 18), actorScale.ToString() ) );	
		if ( actorScale > 3.0f ) actorScale = 3.0f;
		if ( actorScale < 0.01f ) actorScale = 0.01f;
		actorScale = GUI.HorizontalSlider( new Rect(140, posY+32, 190, 30), actorScale, 0.01f, 3.0f);
		
		GUI.Label( new Rect(itemStart, posY+55, 100, 30), "Duration(Sec):");	
		var srtPos = itemStart+itemAdd;
		for ( int i=0; i<arySec.Length; ++i ) {
			if ( GUI.Button( new Rect(srtPos, posY+55, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(actorTransDuration==arySec[i])?1:2] )){
				actorTransDuration = arySec[i];
			}
			srtPos += 34;
		}
		GUI.Label( new Rect (20, posY, 200, 30), "Transform animation:");	
	}
	
	void OnGUIGaze() {
		int posY = 575;
		int add = 30;
		float[] arySec = {0.2f, 1.0f, 2.0f};	
		//degree
		GUI.Label( new Rect(75, posY, 70, 30), "Direction:");	
		gazeDegree = (int)GUI.HorizontalSlider( new Rect(140, posY+5, 190, 30), gazeDegree, 0, 360);
		gazeDegree = int.Parse( GUI.TextArea( new Rect(335, posY, 30, 18), gazeDegree.ToString() ) );
		if ( gazeDegree > 360 ) gazeDegree = 360;
		if ( gazeDegree < 0 ) gazeDegree = 0;
		
		//radius
		GUI.Label( new Rect(75, posY+add, 70, 30), "Strength:");	
		GUI.Label ( new Rect (368, posY+add, 20, 30), "%");		
		gazeRadius = (int)GUI.HorizontalSlider( new Rect(140, posY+5+add, 190, 30), gazeRadius, 0, 100);
		gazeRadius = int.Parse( GUI.TextArea( new Rect(335, posY+add, 30, 18), gazeRadius.ToString() ) );		
		if ( gazeRadius > 100 ) gazeRadius = 100;
		if ( gazeRadius < 0 ) gazeRadius = 0;
		
		//Duration
		GUI.Label( new Rect(75, posY+add*2, 100, 30), "Duration(Sec):");	
		int srtPos = itemStart+itemAdd;
		for ( int i=0; i<arySec.Length; ++i ) {
			if ( GUI.Button( new Rect(srtPos, posY+add*2, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(gazeDuration==arySec[i])?1:2] )){
				gazeDuration = arySec[i];
			}
			srtPos += 34;
		}		
		//Apply
		if ( GUI.Button( new Rect(itemStart+itemAdd*2, posY+add*2, 100, 25), "Play") ) {
			actorAPIs[curSel].ActorGaze( gazeDegree, gazeRadius, 0, gazeDuration );		
		}
		GUI.Label ( new Rect (20,posY-25, 200, 30), "Gaze animation:");	
	}
	
	void OnGUIStatus() {
		
		string str = "Status:";
		str += (actorAPIs[curSel].isLookingAt)? " Look At On," : " Look At Off,";
		str += (actorAPIs[curSel].isShowing)? " Sowing Actor," : "";
		str += (actorAPIs[curSel].isTransforming)? " Transforming Actor," : "";
		str += (actorAPIs[curSel].isGazing)? " Gaze On," : " Gaze Off,";
		str += " " + strPlay;
		GUI.Label( new Rect( 15, 15, 500, 30 ), str );
	}
	void NotifyPlayState( CT.PlayState state, string animation, float sec ) {
		strPlay = "";
		if ( state == CT.PlayState.Playing ) {
			strPlay = "Playing '" + animation + "' " + sec + "Sec";
		} else if ( state == CT.PlayState.Stop ) {
			strPlay = "Stop Playing";
		} else if ( state == CT.PlayState.Eof ) {
			strPlay = "End Playing";
			curPlayAnim = -1;
		}
	}
	
	void ActorTransform( Vector2 pos, float scale, float delay, float time ) {
		for ( int i = 0; i < actors.Length; ++i ) {
			actorAPIs[i].ActorTransform( pos, scale, delay, (i==curSel)?time:0.0f);	
		}	
		curPositionX = (int)pos.x;
		curPositionY = (int)pos.y;
		curScale = scale;	
	}

	void StopAllMotions() {
		if ( curPlayAnim != -1 ) {
			actorAPIs[curSel].Stop( 0.0f );	
			curPlayAnim = -1;
		}
	}
	
	void StopAllIdles() {
		// stop all others
		if ( curPlayIdle != -1 ) {
			actors[curSel].GetComponent<Animation>().Stop(curIdles[curPlayIdle]);
		}
	}

}
