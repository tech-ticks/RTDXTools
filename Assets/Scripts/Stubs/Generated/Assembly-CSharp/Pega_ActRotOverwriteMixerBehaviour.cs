﻿/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Image 46: Assembly-CSharp.dll - Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null - Types 4339-9572

public class Pega_ActRotOverwriteMixerBehaviour : PlayableBehaviour // TypeDefIndex: 5642
{
	// Fields
	private bool m_FirstFrameHappened; // 0x10
	public Pega_ActRotOverwriteTrack ownerTrack; // 0x18
	public PlayableDirector refPlayableDirector; // 0x20
	private List<TimelineClip> timelineClips_; // 0x28
	private Vector3 prevWorldPos; // 0x30

	// Constructors
	public Pega_ActRotOverwriteMixerBehaviour() {} // 0x00798690-0x00798730

	// Methods
	public override void OnGraphStart(Playable playable) {} // 0x00797B90-0x00797C30
	public override void OnGraphStop(Playable playable) {} // 0x00797C30-0x00797C40
	public override void ProcessFrame(Playable ownerPlayable, FrameData info, object playerData) {} // 0x00797C40-0x00798690
}

