﻿/*
 * Generated code file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// Image 46: Assembly-CSharp.dll - Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null - Types 4339-9572

namespace ActCommandSystem
{
	[ActCommand] // 0x0060B400-0x0060B440
	public class ActCommandError : ActCommand_Basic // TypeDefIndex: 8743
	{
		// Fields
		public string originalType; // 0x28
		public string unknownData; // 0x30
	
		// Constructors
		public ActCommandError() {} // 0x008DD170-0x008DD200
	
		// Methods
		// [IteratorStateMachine] // 0x0063C100-0x0063C170
		public override IEnumerator callAction(ActTargetBase target, ActDataPlayer.PlayContext context, Action endWait) => default; // 0x008DD0D0-0x008DD130
		public override void CalcSnapshotState(ActObjSnapshotState workSnapshotState, ActSnapshotContext snapshotContext, ScriptData refScriptData) {} // 0x008DD160-0x008DD170
	}
}
