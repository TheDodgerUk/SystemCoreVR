/**
 * Copyright (c) 2017 The Campfire Union Inc - All Rights Reserved.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for
 * full license information.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using System;
using System.Collections;

namespace VRKeys {
	/// <summary>
	/// Manages placement of the keyboard relative to the user, including
	/// grabbing it to move and resize the keyboard.
	/// </summary>
	public class Placement : MonoBehaviour {

		[Serializable]
		public class PlacementSettings {
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 scale;
		}

		public float minScale = 0.4f;

		public float maxScale = 1.2f;

		public PlacementSettings settings;

		private Keyboard keyboard;

		private Vector3 previousPosition;

		private float previousRotationX;

		private Vector3 initialScale;

		private float applyScale = 1f;

		private float initialHandDistance = -1f;

		private float initialApplyScale = 1f;

		private string prefsKey = "vrkeys:placement";

		public void CreateKeyboard()
        {
			keyboard = GetComponent<Keyboard> ();

			if (PlayerPrefs.HasKey (prefsKey)) {
				JsonUtility.FromJsonOverwrite (PlayerPrefs.GetString (prefsKey), settings);
			}

			keyboard.keyboardWrapper.transform.localPosition = settings.position;
			keyboard.keyboardWrapper.transform.localRotation = settings.rotation;
			keyboard.keyboardWrapper.transform.localScale = settings.scale;

			initialScale = keyboard.keyboardWrapper.transform.localScale;
		}


		void SaveChanges () {
			settings.position = keyboard.keyboardWrapper.transform.localPosition;
			settings.rotation = keyboard.keyboardWrapper.transform.localRotation;
			settings.scale = keyboard.keyboardWrapper.transform.localScale;

			PlayerPrefs.SetString (prefsKey, JsonUtility.ToJson (settings));
		}
	}
}