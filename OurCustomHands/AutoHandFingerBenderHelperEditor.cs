#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.UI;
using System.IO;

[CustomEditor(typeof(AutoHandFingerBenderHelper))]
public class AutoHandFingerBenderHelperEditor : UnityEditor.Editor
{
    //  the left is just minus
    public static readonly List<Vector3> FingerTipLeft_AVATAR = new List<Vector3>()
    {
            { new Vector3(-0.0075f,0,0) }, // thumb
            { new Vector3(-0.015f,0,0) },
            { new Vector3(-0.015f,0,0) },
            { new Vector3(-0.015f,0,0) },
            { new Vector3(-0.015f,0,0) },
    };

    public static readonly List<Vector3> FingerTipRight_AVATAR = new List<Vector3>()
    {
            { new Vector3(0.0075f,0,0) }, // thumb
            { new Vector3(0.015f,0,0) },
            { new Vector3(0.015f,0,0) },
            { new Vector3(0.015f,0,0) },
            { new Vector3(0.015f,0,0) },
    };


    public class BendData
    {
        public string Name;
        public Vector3 Rotation;
        public Quaternion RotationQuaternion;
        public Vector3 Position;
    }

    private const string DEBUG_TIP = "Tip";
    private const string DEBUG_CAPSULE = "DEBUG_CAPSULE";
    private const int SPACE = 10;

    public override void OnInspectorGUI()
    {
        AutoHandFingerBenderHelper helper = (AutoHandFingerBenderHelper)target;

        if (GUILayout.Button("AddTips"))
        {
            AddTips(helper);
        }

        if (GUILayout.Button("RemoveTips"))
        {
            RemoveTips(helper);
        }

        GUILayout.Space(SPACE);
        GUILayout.Space(SPACE);

        if (GUILayout.Button("CreateOutline"))
        {
            CreateOutLine(helper);
        }

        if (GUILayout.Button("RemoveOutline"))
        {
            RemoveOutLine(helper);
        }

        GUILayout.Space(SPACE);
        GUILayout.Space(SPACE);

        string defaultDirectory = $"{Application.dataPath}/SystemCoreVR/JsonHandRotations";

        if (GUILayout.Button("Save Data to File"))
        {
            List<BendData> allData = new List<BendData>();
            var all = helper.GetComponentsInChildren<Transform>();
            for (int i = 0; i < all.Length; i++)
            {
                BendData data = new BendData();
                data.Name = all[i].name;
                data.Rotation = all[i].transform.localEulerAngles;
                data.RotationQuaternion = all[i].transform.localRotation;
                data.Position = all[i].transform.localPosition;
                allData.Add(data);
            }

            if (Directory.Exists(defaultDirectory) == false)
            {
                Directory.CreateDirectory(defaultDirectory);
            }
            string path = EditorUtility.SaveFilePanel("Save Hand .json", defaultDirectory, "Save", "json");
            if (path.Length != 0)
            {
                Json.JsonNet.WriteToFile(allData, path, true);
                EditorUtility.DisplayDialog("HandHelper", $"Full wriiten : {path}", "OK");
            }
        }

        if (GUILayout.Button("Load Data from file"))
        {
            List<BendData> allData = new List<BendData>();
            var all = helper.GetComponentsInChildren<Transform>().ToList();
            for (int i = 0; i < all.Count; i++)
            {
                BendData data = new BendData();
                data.Name = all[i].name;
                data.Rotation = all[i].transform.localEulerAngles;
                data.RotationQuaternion = all[i].transform.localRotation;
                allData.Add(data);
            }
            if (Directory.Exists(defaultDirectory) == false)
            {
                Directory.CreateDirectory(defaultDirectory);
            }
            string path = EditorUtility.OpenFilePanel("Open Hand .json", defaultDirectory, "json");
            if (path.Length != 0)
            {
                if (File.Exists(path) == false)
                {
                    path = path.Remove(path.Length - ".json".Length, ".json".Length);
                }

                int index = path.IndexOf(".");
                if (index >= 0)
                {
                    path = path.Substring(0, index);
                }

                var data = Json.JsonNet.ReadFromFile<List<BendData>>(path);

                foreach (var item in data)
                {
                    if (item.Name.StartsWith("Left") == true)
                    {
                        item.Name = item.Name.Remove(0, "Left".Length);
                    }
                    if (item.Name.StartsWith("Right") == true)
                    {
                        item.Name = item.Name.Remove(0, "Right".Length);
                    }
                }
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].name == "OurAvatarPlayerHandSkeletonLeft" || all[i].name == "OurAvatarPlayerHandSkeletonRight")
                    {
                        all[i].transform.localEulerAngles = Vector3.zero;
                        continue;
                    }

                    if (all[i].name == "LeftHandWrist_jnt")
                    {
                        all[i].transform.localEulerAngles = new Vector3(90f, 90f, 0);
                        continue;
                    }

                    if (all[i].name == "RightHandWrist_jnt")
                    {
                        all[i].transform.localEulerAngles = new Vector3(270f, 270f, 0);
                        continue;
                    }


                    if (all[i].name.EndsWith("_tgt")) // if use this. it inverts badly
                    {
                        all[i].transform.localPosition = Vector3.zero;
                        continue;
                    }

                    var item = data.Find(e => all[i].name.Contains(e.Name));
                    if (item != null)
                    {
                        Debug.LogError($"Found Name: {all[i].name}, item {item.Name}  ");
                        all[i].transform.localEulerAngles = item.Rotation;
                        //all[i].transform.localPosition = item.Position;
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Name: {all[i].name}");
                    }
                }
                EditorUtility.SetDirty(helper.gameObject);
                EditorUtility.DisplayDialog("HandHelper", $"file read : {path}", "OK");
                EditorUtility.SetDirty(helper.gameObject);
            }
        }

        GUILayout.Space(SPACE);
        if (GUILayout.Button("OpenFolder"))
        {
            EditorUtility.RevealInFinder(defaultDirectory);
        }
    }


    public void AddTips(AutoHandFingerBenderHelper helper)
    {
        var hand = helper.GetComponent<Autohand.Hand>();

        if (hand.left)
        {
            AddFingerTipsFor_AVATAR(helper, hand, FingerTipLeft_AVATAR);
        }
        else
        {
            AddFingerTipsFor_AVATAR(helper, hand, FingerTipRight_AVATAR);
        }
    }


    public void RemoveTips(AutoHandFingerBenderHelper helper)
    {
        var hand = helper.GetComponent<Autohand.Hand>();

        if (hand.left)
        {
            RemoveTipsFor_AVATAR(helper, hand, FingerTipLeft_AVATAR);
        }
        else
        {
            RemoveTipsFor_AVATAR(helper, hand, FingerTipRight_AVATAR);
        }
    }

    private void AddFingerTipsFor_AVATAR(AutoHandFingerBenderHelper helper, Autohand.Hand hand, List<Vector3> tipPositions)
    {
        // have to add finger tip AFTER wards like other stuff as the OvrAvatarCustomHandPose explodes if doing to before hand
        // need tip  as AutoHand will not bend OUR end of finger without it 
        // with out this the "Distal_jnt" parts do not bend

        for (int i = 0; i < hand.fingers.Length; i++)
        {
            var finger = hand.fingers[i];
            var oldTip = finger.tip;
            GameObject newTip = oldTip.gameObject;
            if (oldTip.name != "Tip")
            {
                newTip = new GameObject("Tip");
                newTip.transform.SetParent(oldTip);
                newTip.transform.ClearLocals();
                finger.tip = newTip.transform;

            }
            newTip.transform.localPosition = tipPositions[i];

        }

        UnityEditor.EditorUtility.SetDirty(helper.gameObject);
    }


    private void RemoveTipsFor_AVATAR(AutoHandFingerBenderHelper helper, Autohand.Hand hand, List<Vector3> tipPositions)
    {
        // have to add finger tip AFTER wards like other stuff as the OvrAvatarCustomHandPose explodes if doing to before hand
        // need tip  as AutoHand will not bend OUR end of finger without it 
        // with out this the "Distal_jnt" parts do not bend

        for (int i = 0; i < hand.fingers.Length; i++)
        {
            var finger = hand.fingers[i];
            var oldTip = finger.tip;

            if (oldTip.name == "Tip")
            {
                finger.tip = oldTip.transform.parent;
                DestroyImmediate(oldTip.gameObject);
            }
        }

        UnityEditor.EditorUtility.SetDirty(helper.gameObject);
    }

    public void RemoveOutLine(AutoHandFingerBenderHelper helper)
    {
        var cap = helper.transform.SearchAll(DEBUG_CAPSULE);
        foreach (var item in cap)
        {
            DestroyImmediate(item.gameObject);
        }
        UnityEditor.EditorUtility.SetDirty(helper.gameObject);
    }

    public void CreateOutLine(AutoHandFingerBenderHelper helper)
    {

        RemoveOutLine(helper);
        var col = helper.GetComponentsInChildren<CapsuleCollider>();
        foreach (var item in col)
        {
            if (item.name.EndsWith("_jnt") == false)
            {
                continue;
            }


            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = DEBUG_CAPSULE;
            var collider = capsule.GetComponent<Collider>();
            DestroyImmediate(collider);
            capsule.transform.SetParent(item.transform);
            capsule.ClearLocals();
            capsule.transform.localRotation = Quaternion.Euler(0f, 0, 90f);
            capsule.transform.localScale = new Vector3(item.radius * 2, item.height / 2, item.radius * 2);
            capsule.transform.localPosition = item.center;
        }

        UnityEditor.EditorUtility.SetDirty(helper.gameObject);
    }

}
#endif