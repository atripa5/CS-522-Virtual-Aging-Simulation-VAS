///--------------------------------------------------------------------------------------
// File: DemoProject.js
//
// Developed by Reallusion Developer team.
//
// Copyright © 2014 Reallusion Inc. All rights reserved.
//--------------------------------------------------------------------------------------
#pragma strict
#pragma downcast

public var ctSkin : GUISkin;

private var ACTOR_COUNT:int = 3;	//Max actor count
private var ANIM_COUNT:int = 6;		//Max animation count of each actor
private var IDLE_COUNT:int = 3;		//MAx idle motion count of each actor
private var itemStart:int = 75;
private var itemAdd:int = 102;

public var actors : GameObject[];
private var actorAPIs : CTPluginAPI[];
private var actorAnims : Array = new Array();

private var curSel : int = -1;
private var showActor : boolean = true;
private var showDuration : float = 1.0;

private var curAnims : String[];
private var curPlayAnim : int = -1;
private var loopPlay : boolean = false;

private var curIdles : String[];
private var curPlayIdle : int = 0;

private var lookatMode : CT.LookAtMode = CT.LookAtMode.FullScreen; //fullscreen mode
private var lookatStrength : float = 1.0;
private var lookatDelay : float = 0.2;
public var lookatTarget : GameObject = null;

private var curPositionX : int = 17;
private var curPositionY : int = 17;
private var curScale : float = 1.0;

private var actorScale : float = 1.0;
private var actorTransDuration = 1.0;
private var gazeDegree : int = 90;
private var gazeRadius : int = 100;
private var gazeDuration : float = 1.0;

private var strPlay : String = "";
private var scrollPosition : Vector2 = Vector2.zero;

function Start () {
	var tmpList : Array = new Array();
	if ( actors.Length ) {
		actorAPIs = new CTPluginAPI[actors.Length];
		actorAnims.Clear();
		for ( var i = 0; i < actors.Length; ++i ) {
			if ( i > ACTOR_COUNT ) break;
			actorAPIs[i] = actors[i].GetComponent( "CTPluginAPI" ) as CTPluginAPI;
			tmpList.Clear();
			for ( var state : AnimationState in actors[i].GetComponent.<Animation>() ) {
				tmpList.Add( state.name );
			}
			actorAnims.Add( tmpList.ToBuiltin(String) as String[] );
			actorAPIs[i].ShowActor( false, 0.0, 0.0 );
		}
		onSelActorChange( 0 );
	}
}

function onSelActorChange( actorIndex : int ) {
	if ( ( curSel != -1 ) && ( actorIndex != curSel ) ) {
		StopAllMotions();
		StopAllIdles();
	}
	for ( var i = 0; i < actors.Length; ++i ) {
		if ( i == actorIndex ) {
			actorAPIs[i].playStateChanged = NotifyPlayState;
			if ( lookatMode == CT.LookAtMode.TargetTracking )
				actorAPIs[i].SetLookAtMode( lookatMode, lookatTarget );
			else
				actorAPIs[i].SetLookAtMode( lookatMode );
			actorAPIs[i].SetLookAtStrength( lookatStrength );				
			actorAPIs[i].ShowActor( true, 0.0, showDuration );
			lookatTarget.transform.parent = actors[i].transform;
			showActor = true;
		} else if ( i == curSel ) {
			actorAPIs[i].playStateChanged = null;
			if ( actors[i].activeSelf ) {
				actorAPIs[i].ShowActor( false, 0.0, showDuration );
			}
		}
	}	
	curSel = actorIndex;
	
	// initialize curAnims and curIdles of the current actor
	var aryAnim = new Array();
	var aryIdle = new Array();
	for ( var clip : String in ( actorAnims[curSel] as String[] ) ) {
		if ( aryAnim.length < ANIM_COUNT ) {
			aryAnim.Push( clip );
		} else {		
			aryIdle.Push( clip );
			if ( aryIdle.length >= IDLE_COUNT ) break;
		}
	}
	curAnims = aryAnim.ToBuiltin(String) as String[];
	curIdles = aryIdle.ToBuiltin(String) as String[];
	if ( curPlayIdle != -1 )
		actorAPIs[curSel].SetIdleMotion( curIdles[curPlayIdle] );
}

function OnGUI() {
	var widthPanel = itemStart+itemAdd*3+15;
 	GUILayout.BeginArea(new Rect(Screen.width-widthPanel, 0, widthPanel, Screen.height), "");

	scrollPosition = GUI.BeginScrollView(new Rect(0, 0, widthPanel, Screen.height), scrollPosition, new Rect(0, 0, widthPanel-20, 685));
	GUI.Box(Rect(0, 0, widthPanel, (Screen.height>685)?Screen.height:685), "CrazyTalk Function Demo" );			 	
	OnGUIActors();
	if ( ( actors.Length >= 1 ) && ( curSel >= 0 ) && ( curSel < actors.Length ) ) {
		try {
			OnGUIPerforms();
			OnGUIIdles();	
			OnGUILookAt();
			OnGUIOrigin();
			OnGUITransform();
			OnGUIGaze();
		} catch( error ) {
			print( error.Message );
		}
	}
	GUI.EndScrollView();
 	GUILayout.EndArea();	
	OnGUIStatus();	
}

function OnGUIActors() {
	var i = 0;
	var posX = itemStart;
	var posY = 25;
	var selChanged = -1;
	var arySec : float[] = [0.2, 1, 2];
	for ( i = 0; i < actors.Length; ++i ) {
		if ( GUI.Button( Rect(posX, posY, 100, 25), actors[i].name, ctSkin.customStyles[(curSel==i)?1:2] ) ) {
			selChanged = i;
		}
		posX += itemAdd;
	}
	showActor = GUI.Toggle( Rect(itemStart, posY+34, 14, 14), showActor, "", ctSkin.customStyles[0] );
	if ( GUI.changed ) {
		actorAPIs[curSel].ShowActor( showActor, 0.0f, showDuration );
	}
	GUI.Label ( Rect(itemStart+20, posY+30, 100, 30), "Show/Hide");	
	if (SystemInfo.supportsStencil != 1) {
		GUI.enabled = false;
		GUI.Label( Rect(itemStart+210, posY+55, 100, 30), "(Unity Pro Only)");	
	}
	GUI.Label( Rect(itemStart, posY+55, 100, 30), "Duration(Sec):");	
	var srtPos = itemStart+itemAdd;
	for ( i=0; i<arySec.Length; ++i ) {
		if ( GUI.Button( Rect(srtPos, posY+55, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(showDuration==arySec[i])?1:2] )){
			showDuration = arySec[i];
		}
		srtPos += 34;
	}
	GUI.enabled = true;
	if ( selChanged != -1 ) {
		// Actor change
		onSelActorChange( selChanged );		
	}
	GUI.Label (Rect (20, 25, 50, 30), "Actor:");	
}

function OnGUIPerforms() {
	var i : int = 0;
	var posX : int = itemStart;
	var posY : int = 115;
	var valueChange : int = -1;;

	if (curPlayAnim!=-1) GUI.enabled = false;
	loopPlay = GUI.Toggle( Rect(itemStart, posY+4, 14, 14), loopPlay, "", ctSkin.customStyles[0] );
	GUI.enabled = true;
	GUI.Label ( Rect(itemStart+20, posY, 50, 30), "Loop");	

	var curY : int = posY + 25;
	for ( i = 0; i < curAnims.Length; ++i ) {
		if ( GUI.Button( Rect(posX, curY, 100, 25), curAnims[i], ctSkin.customStyles[(curPlayAnim==i)?1:2] ) ) {
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
	GUI.Label ( Rect(20, posY, 50, 30), "Motion:");	
}

function OnGUIIdles() {
	var posY = 205;
	var i = 0;
	var posX = itemStart;
	var valueChange = -1;
	for ( i = 0; i < curIdles.Length; ++i ) {
		if ( GUI.Button( Rect(posX, posY, 100, 25), curIdles[i], ctSkin.customStyles[(curPlayIdle==i)?1:2] ) ) {
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
	GUI.Label (Rect (20, posY, 50, 30), "Idle:");	
}
	
function OnGUILookAt() {
	var posY = 245;
	var aryLookat = ["Game View", "Full Screen","Target Tracking"];
	var aryLookatMode = [CT.LookAtMode.GameView, CT.LookAtMode.FullScreen, CT.LookAtMode.TargetTracking];
	var i = 0;
	var posX = itemStart;
	var halo : Component;
	var value = lookatStrength;
	for ( i = 0; i < aryLookat.length; ++i ) {
		if ( GUI.Button( Rect( posX, posY, 100, 25 ), aryLookat[i], ctSkin.customStyles[(lookatMode == aryLookatMode[i])?1:2] ) ) {
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
	GUI.Label (Rect (75, posY+30, 70, 30), "Strength:");	
	GUI.Label (Rect (368, posY+30, 20, 30), "x");		
	value = parseFloat( GUI.TextArea( Rect(335, posY+32, 30, 18), lookatStrength.ToString() ) );	
	if ( value != lookatStrength ) {
		if ( value > 2.0 ) value = 2.0;
		if ( value < 0.0 ) value = 0.0;		
		lookatStrength = value;	
		actorAPIs[curSel].SetLookAtStrength( value );	
	}
	value = GUI.HorizontalSlider( Rect(140, posY+37, 190, 20), lookatStrength, 0.0, 2.0);
	if ( value != lookatStrength ) {
		lookatStrength = value;
		actorAPIs[curSel].SetLookAtStrength( value );	
	}
	GUI.Label (Rect (75, posY+55, 70, 30), "Delay:");	
	GUI.Label (Rect (368, posY+55, 22, 30), "s");		
	value = parseFloat( GUI.TextArea( Rect(335, posY+57, 30, 18), lookatDelay.ToString() ) );	
	if ( value != lookatDelay ) {
		if ( value > 1.0 ) value = 1.0;
		if ( value < 0.0 ) value = 0.0;		
		lookatDelay = value;	
		actorAPIs[curSel].SetLookAtDelay( lookatDelay );	
	}
	value = GUI.HorizontalSlider( Rect(140, posY+62, 190, 20), lookatDelay, 0.0, 1.0);
	if ( value != lookatDelay ) {
		lookatDelay = value;
		actorAPIs[curSel].SetLookAtDelay( value );	
	}

	GUI.Label (Rect (20, posY, 50, 30), "LookAt:");	
}

function OnGUIOrigin() {
	var value : int;	
	var valueF : float;
	var posY = 355;
	
	GUI.Label (Rect (75, posY, 70, 30), "X:");	
	GUI.Label (Rect (368, posY, 20, 30), "%");			
	value = parseInt( GUI.TextArea( Rect(335, posY+2, 30, 18), curPositionX.ToString() ) );		
	if ( value != curPositionX ) {
		if ( value > 150 ) value = 150;
		if ( value < -50 ) value = -50;		
		curPositionX = value;
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	value = GUI.HorizontalSlider( Rect(140, posY+7, 190, 20), curPositionX, -50, 150);
	if ( value != curPositionX ) {
		curPositionX = value;
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	GUI.Label (Rect (75, posY+25, 70, 30), "Y:");
	GUI.Label (Rect (368, posY+25, 20, 30), "%");	
	value = parseInt( GUI.TextArea( Rect(335, posY+27, 30, 18), curPositionY.ToString() ) );	
	if ( value != curPositionY ) {
		if ( value > 150 ) value = 150;
		if ( value < -50 ) value = -50;		
		curPositionY = value;
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	value = GUI.HorizontalSlider( Rect(140, posY+32, 190, 20), curPositionY, -50, 150);
	if ( value != curPositionY ) {
		curPositionY = value;
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	GUI.Label (Rect (75, posY+50, 70, 30), "Scale:");	
	GUI.Label (Rect (368, posY+50, 20, 30), "x");		
	valueF = parseFloat( GUI.TextArea( Rect(335, posY+52, 30, 18), curScale.ToString() ) );	
	if ( valueF != curScale ) {
		if ( valueF > 3.0 ) valueF = 3.0;
		if ( valueF < 0.01 ) valueF = 0.01;			
		curScale = valueF;	
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	valueF = GUI.HorizontalSlider( Rect(140, posY+57, 190, 20), curScale, 0.01, 3.0);
	if ( valueF != curScale ) {
		curScale = valueF;	
		ActorTransform( Vector2( curPositionX, curPositionY ), curScale, 0, 0 );
	}
	if ( GUI.Button( Rect( itemStart+itemAdd*2, posY+75, 100, 25 ), "Reset" )) {
		ActorTransform( new Vector2(17, 17), 1.0f, 0, 0.0f );
	}
	GUI.Label(Rect (20, posY-25, 200, 30), "Initial Position:");	
}

function OnGUITransform() {
	var posY : int = 435;
	var aryTrans : String[] = ["Left Bottom", "Right Bottom", "Left Up", "Center"];
	var aryTransPos : Vector2[] = [	Vector2(36, posY+85),
									Vector2(122, posY+85),
									Vector2(208, posY+85),
									Vector2(294, posY+85)];
	var aryValuePos : Vector2[] = [	Vector2(17, 17),
									Vector2(84, 17),
									Vector2(17, 84),
									Vector2(50, 50)];
	var arySec : float[] = [0.2, 1, 2];
	
	for ( var i=0; i<aryTrans.Length; ++i) {
		if ( GUI.Button( Rect(aryTransPos[i].x, aryTransPos[i].y, 86, 25), aryTrans[i]/*, ctSkin.customStyles[2]*/ ) ) {
			ActorTransform( aryValuePos[i], actorScale, 0, actorTransDuration );
		}
	}
	GUI.Label (Rect (75, posY+25, 70, 30), "Scale:");	
	GUI.Label (Rect (368, posY+25, 20, 30), "x");		
	actorScale = parseFloat( GUI.TextArea( Rect(335, posY+27, 30, 18), actorScale.ToString() ) );	
	if ( actorScale > 3.0 ) actorScale = 3.0;
	if ( actorScale < 0.01 ) actorScale = 0.01;
	actorScale = GUI.HorizontalSlider( Rect(140, posY+32, 190, 30), actorScale, 0.01, 3.0);
	
	GUI.Label(Rect(itemStart, posY+55, 100, 30), "Duration(Sec):");	
	var srtPos = itemStart+itemAdd;
	for ( i=0; i<arySec.Length; ++i ) {
		if ( GUI.Button( Rect(srtPos, posY+55, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(actorTransDuration==arySec[i])?1:2] )){
			actorTransDuration = arySec[i];
		}
		srtPos += 34;
	}
	GUI.Label(Rect (20, posY, 200, 30), "Transform animation:");	
}

function OnGUIGaze() {
	var posY = 575;
	var add = 30;
	var arySec = [0.2, 1, 2];	
	//degree
	GUI.Label(Rect(75, posY, 70, 30), "Direction:");	
	gazeDegree = GUI.HorizontalSlider( Rect(140, posY+5, 190, 30), gazeDegree, 0, 360);
	gazeDegree = parseInt( GUI.TextArea( Rect(335, posY, 30, 18), gazeDegree.ToString() ) );
	if ( gazeDegree > 360 ) gazeDegree = 360;
	if ( gazeDegree < 0 ) gazeDegree = 0;
		
	//radius
	GUI.Label(Rect(75, posY+add, 70, 30), "Strength:");	
	GUI.Label (Rect (368, posY+add, 20, 30), "%");		
	gazeRadius = GUI.HorizontalSlider( Rect(140, posY+5+add, 190, 30), gazeRadius, 0, 100);
	gazeRadius = parseInt( GUI.TextArea( Rect(335, posY+add, 30, 18), gazeRadius.ToString() ) );		
	if ( gazeRadius > 100 ) gazeRadius = 100;
	if ( gazeRadius < 0 ) gazeRadius = 0;
		
	//Duration
	GUI.Label(Rect(75, posY+add*2, 100, 30), "Duration(Sec):");	
	var srtPos = itemStart+itemAdd;
	for ( var i=0; i<arySec.Length; ++i ) {
		if ( GUI.Button( Rect(srtPos, posY+add*2, 32, 25), arySec[i].ToString(), ctSkin.customStyles[(gazeDuration==arySec[i])?1:2] )){
			gazeDuration = arySec[i];
		}
		srtPos += 34;
	}		
	//Apply
	if ( GUI.Button(Rect(itemStart+itemAdd*2, posY+add*2, 100, 25), "Play") ) {
		actorAPIs[curSel].ActorGaze( gazeDegree, gazeRadius, 0, gazeDuration );		
	}
	GUI.Label (Rect (20,posY-25, 200, 30), "Gaze animation:");	
}

function OnGUIStatus() {

	var str = "Status:";
	str += (actorAPIs[curSel].isLookingAt)? " Look At On," : " Look At Off,";
	str += (actorAPIs[curSel].isShowing)? " Sowing Actor," : "";
	str += (actorAPIs[curSel].isTransforming)? " Transforming Actor," : "";
	str += (actorAPIs[curSel].isGazing)? " Gaze On," : " Gaze Off,";
	str += " " + strPlay;
	GUI.Label( Rect( 15, 15, 500, 30 ), str );
}

function NotifyPlayState( state:CT.PlayState, animation : String, sec : float ) {
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

function ActorTransform( pos : Vector2, scale : float, delay : float, time : float ) {
	for ( var i = 0; i < actors.length; ++i ) {
		actorAPIs[i].ActorTransform( pos, scale, delay, (i==curSel)?time:0.0 );	
	}	
	curPositionX = pos.x;
	curPositionY = pos.y;
	curScale = scale;	
}

function StopAllMotions() {
	if ( curPlayAnim != -1 ) {
		actorAPIs[curSel].Stop( 0.0f );	
		curPlayAnim = -1;
	}
}

function StopAllIdles() {
	// stop all others
	if ( curPlayIdle != -1 ) {
		actors[curSel].GetComponent.<Animation>().Stop(curIdles[curPlayIdle]);
	}
}
