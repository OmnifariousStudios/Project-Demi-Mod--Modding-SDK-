using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using Formatting = Newtonsoft.Json.Formatting;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This script is used to create hand poses from the player avatar's hand transforms.
/// In Play Mode, the script will play through the hand animations one by one.
/// Click Save Pose to save the current hand pose,and move on to the next one.
/// The hand poses are then converted to JSON and saved to a file to be included with the mod.
/// </summary>
public class HandPoseCopier : MonoBehaviour
{
    public bool debugHandPoseCopier = false;
    
    public PlayerAvatar playerAvatarScript;
    
    private Dictionary<int, bool> PoseCompletedChecklist = new Dictionary<int, bool>();
    int poseCompletedCount = 0;

    public RuntimeAnimatorController avatarRuntimeController;
    public Animator avatarAnimator;
    public List<AnimationClip> handPoseAnimations;
    private AnimationClip currentClipToRecord;
    
    public List<GameObject> rightHandWeaponShapes;
    public List<GameObject> leftHandWeaponShapes;
    public int currentWeaponShapeIndex = 0;
    
    public Dictionary<int, int> PoseToShapeDictionary = new Dictionary<int, int>();

    public List<HVRHandPose> handPoses;
    
    public HVRHandPose newPose;
    
    public string jsonFile;

    // IK Targeting Rotation
    public Quaternion rightHandIKRotation;
    public Quaternion leftHandIKRotation;
    
    public Vector3 rightHandIKRotationEuler;
    public Vector3 leftHandIKRotationEuler;

    // Weapon Pose Offsets
    public Vector3 leftHandPositionOffset;
    public Vector3 leftHandRotationOffset;
    public Vector3 rightHandPositionOffset;
    public Vector3 rightHandRotationOffset;
    
    public int currentPoseIndex = 0;
    public HVRHandPose currentPose;

    // Debug Panel Texts
    public Text currentPoseIndexReadout;
    public Text currentPoseReadout;
    public Text currentWeaponShapeReadout;
    public Text poseCompletionReadout;

    private void Awake()
    {
        // Sphere Grip Pose -> FireballShape
        PoseToShapeDictionary.Add(3, 0);
        
        // Hold Object Pose -> SwordShape
        PoseToShapeDictionary.Add(4, 1);
        
        // Hold Object Pose -> Shield
        PoseToShapeDictionary.Add(5, 2);
        
        // Hold Object Pose -> Kunai
        PoseToShapeDictionary.Add(6, 3);
        
        // Hold Object Pose -> Spear
        PoseToShapeDictionary.Add(7, 4);
        
        // Katana
        PoseToShapeDictionary.Add(8, 5);
        
        // Hold Object Pose -> Warhammer
        PoseToShapeDictionary.Add(9, 6);
        
        // Hold Object Pose -> Baton
        PoseToShapeDictionary.Add(10, 7);
        
        // Pistol Pose -> Pistol
        PoseToShapeDictionary.Add(11, 8);
        
        // Pistol Secondary
        PoseToShapeDictionary.Add(12, 8);
    }

    private void Start()
    {
        for (int i = 0; i < handPoseAnimations.Count; i++)
        {
            PoseCompletedChecklist.Add(i, false);
        }
        
        currentPoseIndex = 0;
        currentWeaponShapeIndex = 0;
        currentClipToRecord = handPoseAnimations[0];
            
        // Play the first animation.
        if (!avatarAnimator)
        {
            Debug.Log("No Avatar Animator Found. Please make sure the Avatar Animator is assigned in the inspector.");
        }
        
        avatarAnimator.Play(currentClipToRecord.name);
        
        if(currentPoseIndexReadout)
            currentPoseIndexReadout.text = "Current Pose Index: " + currentPoseIndex;
        
        if (currentPoseReadout)
            currentPoseReadout.text = "Current Pose: " + currentClipToRecord.name;
        
        if (currentWeaponShapeReadout)
            currentWeaponShapeReadout.text = "Current Weapon Shape: " + "None";
        
        if (poseCompletionReadout)
            poseCompletionReadout.text = "Pose Completion: " + "0 / " + handPoseAnimations.Count;
    }


    [ContextMenu("Copy All Hand Poses")]
    public void StartCopyAllHandPoses()
    {
        if (playerAvatarScript && avatarAnimator)
        {
            if(avatarAnimator.runtimeAnimatorController != avatarRuntimeController)
                avatarAnimator.runtimeAnimatorController = avatarRuntimeController;
        }
        
        
        StartCoroutine(CopyAllHandPoses());
    }
    
    public IEnumerator CopyAllHandPoses()
    {
        handPoses = new List<HVRHandPose>();

        for (int i = 0; i < handPoseAnimations.Count; i++)
        {
            currentClipToRecord = handPoseAnimations[i];
            
            // Play the animation.
            avatarAnimator.Play(currentClipToRecord.name);

            yield return new WaitForSeconds(0.25f);

            // Copy the hand poses.
            CreatePoseFromCurrentHandTransform();
            
            yield return new WaitForSeconds(0.25f);
        }
    }



    //[ContextMenu("Create Pose From Current Hand Transform")]
    public void CreatePoseFromCurrentHandTransform()
    {
        if (!playerAvatarScript)
            playerAvatarScript = GetComponent<PlayerAvatar>();
        
        if (playerAvatarScript)
        {
            if (playerAvatarScript.rightHand)
            {
                if(Application.isPlaying)
                    newPose = ScriptableObject.CreateInstance<HVRHandPose>();
                else
                {
                    newPose = new HVRHandPose();
                }

                HVRHandPoseData newRightHandData = new HVRHandPoseData();
                HVRHandPoseData newLeftHandData = new HVRHandPoseData();
                
                newPose.RightHand = newRightHandData;
                newPose.LeftHand = newLeftHandData;

                CreateWeaponHandPose(newPose);
                
                if(currentClipToRecord)
                {
                    newPose.handPoseName = currentClipToRecord.name;
                    newPose.RightHand.handPoseName = currentClipToRecord.name;
                    newPose.LeftHand.handPoseName = currentClipToRecord.name;
                }

                HVRPosableFingerData newFingerData = new HVRPosableFingerData();
                
                // For each finger bone transform. Should be 30 total.
                for (int i = 0; i < playerAvatarScript.fingerBoneTransforms.Count; i++)
                {

                    if (i % 3 == 0)
                    {
                        newFingerData = new HVRPosableFingerData();
                    }

                    if (i < playerAvatarScript.fingerBoneTransforms.Count / 2)
                    {
                        if (i == 0)
                        {
                            newPose.LeftHand.Index = newFingerData;
                        }
                        else if (i == 3)
                        {
                            newPose.LeftHand.Middle = newFingerData;
                        }
                        else if (i == 6)
                        {
                            newPose.LeftHand.Pinky = newFingerData;
                        }
                        else if (i == 9)
                        {
                            newPose.LeftHand.Ring = newFingerData;
                        }
                        else if (i == 12)
                        {
                            newPose.LeftHand.Thumb = newFingerData;
                        }
                        
                    }
                    else
                    {
                        if (i == 15)
                        {
                            newPose.RightHand.Index = newFingerData;
                        }
                        else if (i == 18)
                        {
                            newPose.RightHand.Middle = newFingerData;
                        }
                        else if (i == 21)
                        {
                            newPose.RightHand.Pinky = newFingerData;
                        }
                        else if (i == 24)
                        {
                            newPose.RightHand.Ring = newFingerData;
                        }
                        else if (i == 27)
                        {
                            newPose.RightHand.Thumb = newFingerData;
                        }
                    }
                    
                    Transform currentFinger = playerAvatarScript.fingerBoneTransforms[i];
                        
                    // Get position and rotation from each finger bone transform and store as a new bone data.
                    HVRPosableBoneData newBoneData = new HVRPosableBoneData();
                    newBoneData.Position = currentFinger.localPosition;
                    newBoneData.Rotation = currentFinger.localRotation;
                        
                    // Add the new bone data to the new finger data.
                    newFingerData.Bones.Add(newBoneData);
                }



                // Add the new pose to the hand poses array.
                handPoses.Add(newPose);
                
                if(debugHandPoseCopier)
                    Debug.Log("Added pose " + newPose.name + " to hand poses array.");
            }
        }
    }


    [ContextMenu("Copy Hands for IK Rotation")]
    public void StartCopyHandsForIKRotation()
    {
        if(debugHandPoseCopier)
            Debug.Log("Copied hands for IK rotation.");
        
        rightHandIKRotation = playerAvatarScript.rightHand.transform.rotation;
        leftHandIKRotation = playerAvatarScript.leftHand.transform.rotation;
        
        rightHandIKRotationEuler = playerAvatarScript.rightHand.transform.rotation.eulerAngles;
        leftHandIKRotationEuler = playerAvatarScript.leftHand.transform.rotation.eulerAngles;
    }
    

    public void ConvertHandPosesToJSON()
    {
        AvatarModData avatarModData = new AvatarModData();

        avatarModData.avatarName = playerAvatarScript.name;
        
        avatarModData.avatarHandPoses = handPoses;

        avatarModData.leftHandIKRotation = leftHandIKRotation;
        avatarModData.leftHandIKRotationEuler = leftHandIKRotation.eulerAngles;
        
        if(debugHandPoseCopier)
        {
            Debug.Log("Left Hand IK Rotation: " + leftHandIKRotation);
            Debug.Log("Left Hand IK Rotation: " + leftHandIKRotation.eulerAngles);
        }

        avatarModData.rightHandIKRotation = rightHandIKRotation;
        avatarModData.rightHandIKRotationEuler = rightHandIKRotation.eulerAngles;
        
        if(debugHandPoseCopier)
        {
            Debug.Log("Right Hand IK Rotation: " + rightHandIKRotation);
            Debug.Log("Right Hand IK Rotation: " + rightHandIKRotation.eulerAngles);
        }
        
        jsonFile = JsonConvert.SerializeObject(avatarModData, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });



        // Check if Folder for this Mod exists in MODS folder. If not, create one.
        string avatarModFolderPath = Path.Combine(Application.dataPath, "MODS" + "/" + playerAvatarScript.gameObject.name);

        if (Directory.Exists(avatarModFolderPath))
        {
            if(debugHandPoseCopier)
                Debug.Log("Avatar mod folder already exists");
        }
        else
        {
            if(debugHandPoseCopier)
                Debug.Log("Creating avatar mod folder");
            Directory.CreateDirectory(avatarModFolderPath);
        }
        
        
        // Check if the AvatarModHandPoses.json file exists in Avatar Mod Folder and overwrite it. If not, create one.
        string handPoseCachePath = Path.Combine(Application.dataPath, "MODS" + "/" + playerAvatarScript.gameObject.name + "/AvatarModHandPoses.json");
        
        if(debugHandPoseCopier)
            Debug.Log("Writing json file to memory.");
        if (File.Exists(handPoseCachePath))
        {
            File.WriteAllText(handPoseCachePath, jsonFile);
            
            if(debugHandPoseCopier)
                Debug.Log("Overwriting existing json file.");
        }
        else
        {
            File.AppendAllText(handPoseCachePath, jsonFile);
            
            if(debugHandPoseCopier)
                Debug.Log("Creating new json file.");
        }
        
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        
        
        Debug.Log("Hand Poses converted to JSON and saved to file! You may now exit Play Mode.");
    }


    public void StartHandPoseProcess()
    {
        if (playerAvatarScript && avatarAnimator)
        {
            if(avatarAnimator.runtimeAnimatorController != avatarRuntimeController)
                avatarAnimator.runtimeAnimatorController = avatarRuntimeController;
        }
        
        handPoses = new List<HVRHandPose>();
        currentPoseIndex = 0;
        currentPose = handPoses[currentPoseIndex];
    }
    
    // Pressed by Canvas Buttons
    
    [ContextMenu("Create Weapon Hand Pose")]
    public void CreateWeaponHandPose(HVRHandPose handPose, GameObject weaponShape = null)
    {
        // Set Position and Rotation for the current pose.
        
        Transform leftHandGrabPoint = leftHandWeaponShapes[currentWeaponShapeIndex].transform.Find("Grab Point");
        Transform rightHandGrabPoint = rightHandWeaponShapes[currentWeaponShapeIndex].transform.Find("Grab Point");

        Debug.Log("Getting Grab Point from Weapon Shape: " + leftHandGrabPoint.parent.name);

        leftHandPositionOffset = leftHandGrabPoint.InverseTransformPointUnscaled(playerAvatarScript.leftHand.position);
        rightHandPositionOffset = rightHandGrabPoint.InverseTransformPointUnscaled(playerAvatarScript.rightHand.position);
        
        
        if(debugHandPoseCopier)
        {
            Debug.Log("Left Hand Position Offset: " + leftHandPositionOffset.x + ", " + leftHandPositionOffset.y + ", " + leftHandPositionOffset.z);
            Debug.Log("Right Hand Position Offset: " + rightHandPositionOffset.x + ", " + rightHandPositionOffset.y + ", " + rightHandPositionOffset.z);
        }

        
        Quaternion leftHandRotationOffset = Quaternion.Inverse(leftHandGrabPoint.rotation) * playerAvatarScript.leftHand.rotation;
        Quaternion rightHandRotationOffset = Quaternion.Inverse(rightHandGrabPoint.rotation) * playerAvatarScript.rightHand.rotation;

        
        if(debugHandPoseCopier)
        {
            Debug.Log("Left Hand Rotation Offset: " + leftHandRotationOffset.eulerAngles);
            Debug.Log("Right Hand Rotation Offset: " + rightHandRotationOffset.eulerAngles);
        }
        
        handPose.LeftHand.Position = leftHandPositionOffset;
        handPose.LeftHand.Rotation = leftHandRotationOffset;
        
        handPose.RightHand.Position = rightHandPositionOffset;
        handPose.RightHand.Rotation = rightHandRotationOffset;
    }

    
    [ContextMenu("Confirm Hand Pose")]
    public void ConfirmHandPose()
    {
        // Copy the hand poses.
        CreatePoseFromCurrentHandTransform();
        
        // Update Checklist
        PoseCompletedChecklist[currentPoseIndex] = true;
        
        UpdatePoseCompletionCount();
    }
    
    
    
    public void NextPose()
    {
        if(Application.isPlaying == false)
            return;
        
        DisableCurrentWeaponShape(currentPoseIndex);
        currentPoseIndex++;

        if(currentPoseIndex > handPoseAnimations.Count - 1)
            currentPoseIndex = 0;
        
        EnableSpecificWeaponShape(currentPoseIndex);
        
        currentClipToRecord = handPoseAnimations[currentPoseIndex];
            
        // Play the animation.
        avatarAnimator.Play(currentClipToRecord.name);

        if(currentPoseIndexReadout)
            currentPoseIndexReadout.text = "Current Pose Index: " + currentPoseIndex;
        
        if (currentPoseReadout)
            currentPoseReadout.text = "Current Pose: " + currentClipToRecord.name;
        
    }
    
    public void PreviousPose()
    {
        if(Application.isPlaying == false)
            return;
        
        
        DisableCurrentWeaponShape();
        currentPoseIndex--;

        if(currentPoseIndex < 0)
            currentPoseIndex = handPoseAnimations.Count - 1;
        
        EnableSpecificWeaponShape(currentPoseIndex);
        
        currentClipToRecord = handPoseAnimations[currentPoseIndex];
            
        // Play the animation.
        avatarAnimator.Play(currentClipToRecord.name);
        
        if(currentPoseIndexReadout)
            currentPoseIndexReadout.text = "Current Pose Index: " + currentPoseIndex;
        
        if (currentPoseReadout)
            currentPoseReadout.text = "Current Pose: " + currentClipToRecord.name;
    }


    public void EnableSpecificWeaponShape(int shapeIndex)
    {
        if (PoseToShapeDictionary.ContainsKey(shapeIndex))
        {
            leftHandWeaponShapes[PoseToShapeDictionary[shapeIndex]].SetActive(true);
            rightHandWeaponShapes[PoseToShapeDictionary[shapeIndex]].SetActive(true);
            
            currentWeaponShapeIndex = PoseToShapeDictionary[shapeIndex];
        }
    }
    
    public void DisableSpecificWeaponShape(int shapeIndex)
    {
        if (PoseToShapeDictionary.ContainsKey(shapeIndex))
        {
            leftHandWeaponShapes[PoseToShapeDictionary[shapeIndex]].SetActive(false);
            rightHandWeaponShapes[PoseToShapeDictionary[shapeIndex]].SetActive(false);
            
            currentWeaponShapeIndex = PoseToShapeDictionary[shapeIndex];
        }
    }
    
    
    public void EnableCurrentWeaponShape()
    {
        if (leftHandWeaponShapes[currentWeaponShapeIndex])
        {
            leftHandWeaponShapes[currentWeaponShapeIndex].SetActive(true);
        }
        
        if (rightHandWeaponShapes[currentWeaponShapeIndex])
        {
            rightHandWeaponShapes[currentWeaponShapeIndex].SetActive(true);
        }

        if (currentWeaponShapeReadout)
        {
            currentWeaponShapeReadout.text = "Current Weapon: " + leftHandWeaponShapes[currentWeaponShapeIndex].name;
        }
    }
    
    public void DisableCurrentWeaponShape(int shapeIndex = -1)
    {
        if (shapeIndex != -1)
        {
            DisableSpecificWeaponShape(shapeIndex);
        }
        
        if (leftHandWeaponShapes[currentWeaponShapeIndex])
        {
            leftHandWeaponShapes[currentWeaponShapeIndex].SetActive(false);
        }
        
        if (rightHandWeaponShapes[currentWeaponShapeIndex])
        {
            rightHandWeaponShapes[currentWeaponShapeIndex].SetActive(false);
        }
        
        if (currentWeaponShapeReadout)
            currentWeaponShapeReadout.text = "Current Weapon Shape: " + "None";
    }

    public void NextWeaponShape()
    {
        DisableCurrentWeaponShape();
        
        currentWeaponShapeIndex++;
        if(currentWeaponShapeIndex >= leftHandWeaponShapes.Count)
            currentWeaponShapeIndex = 0;
        
        EnableCurrentWeaponShape();
    }
    
    public void PreviousWeaponShape()
    {
        DisableCurrentWeaponShape();
        
        currentWeaponShapeIndex--;
        if(currentWeaponShapeIndex < 0)
            currentWeaponShapeIndex = leftHandWeaponShapes.Count - 1;
        
        EnableCurrentWeaponShape();
    }


    public void FinishHandPoseProcess()
    {
        bool completedAllPoses = true;
        for (int i = 0; i < handPoseAnimations.Count - 1; i++)
        {
            if(PoseCompletedChecklist[i] == false)
            {
                completedAllPoses = false;
                
                Debug.Log("Please complete pose: " + i + " before finishing.");
            }
        }
        
        if(completedAllPoses == false)
        {
            //return;
        }

        if (PoseCompletedChecklist[handPoseAnimations.Count - 1])
        {
            // Play the last animation for IK Targeting and record.
            currentPoseIndex = handPoseAnimations.Count - 1;
            currentClipToRecord = handPoseAnimations[currentPoseIndex];
            avatarAnimator.Play(currentClipToRecord.name);
        
            Invoke(nameof(StartCopyHandsForIKRotation), 0.5f);
        }

        Invoke(nameof(ConvertHandPosesToJSON), 1.0f);
    }

    public void UpdatePoseCompletionCount()
    {
        int completedPoseCount = 0;
        
        foreach (var key in PoseCompletedChecklist.Values)
        {
            if(key == true)
                completedPoseCount++;
        }

        poseCompletedCount = completedPoseCount;
        
        if (poseCompletionReadout)
            poseCompletionReadout.text = "Pose Completion: " + poseCompletedCount + " / " + handPoseAnimations.Count;
    }
}



[Serializable]
public class AvatarModData
{
    [SerializeField]
    public string avatarName;
    
    [SerializeField]
    public List<HVRHandPose> avatarHandPoses;
    
    public Quaternion rightHandIKRotation;
    public Quaternion leftHandIKRotation;

    public Vector3 rightHandIKRotationEuler;
    public Vector3 leftHandIKRotationEuler;
}
