#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.Linq;
using System.IO;


public class ProjectDemiModAvatarExporter : EditorWindow
{
    string avatarName = "";
    string basePath = "";
    string exportPath = "";
    bool showExportSettings = true;
    bool openAfterExport;

    public HandPoseCopier handPoseCopierScript;
    
    // Demi-Mod Variables
    private GameObject avatarModel;
    public Animator animator;
    public PlayerAvatar playerAvatarScript;
    public string avatarNameString = "";
    
    public string UnityModsFolderPath = "";
    public string activeModFolderPath = "";
    public string activeModPrefabPath = "";
    
    
    public bool FolderSetupComplete = false;
    public bool AvatarSetupComplete = false;
    public bool CustomMaterialSettingsComplete = false;
    public bool CustomHandPoseSettingsComplete = false;
    
    
    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    
    
    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Project Demigod/Avatar Mod Exporter")]
    public static void ShowMapWindow() 
    {
        GetWindow<ProjectDemiModAvatarExporter>("Avatar Mod Exporter");
    }
    
    private void Awake() 
    {
        exportPath = "";
        basePath = FormatPath(UnityEngine.Application.persistentDataPath + "/Export");
        openAfterExport = EditorPrefs.GetBool("OpenAfterExport", false);
        UnityModsFolderPath =  Application.dataPath + "/MODS";
        
        if(buildTarget == BuildTarget.NoTarget)
            buildTarget = BuildTarget.StandaloneWindows64;
    }
    
    
    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80;
        GUILayout.Label("Project Demigod Mod Exporter", EditorStyles.largeLabel);
        GUILayout.Space(10);

        // An absolute-positioned example: We make a scrollview that has a really large client
        // rect and put it in a small rect on the screen.

        //Rect positionRect = new Rect(0, 0, 500, 1000);
        //Rect viewRect = new Rect(0, 0, 500, 1000);
        
        //scrollPosition = EditorGUILayout.BeginScrollView(positionRect, scrollPosition, viewRect, true, true);
        
        
        
        GUILayoutOption[] options = { GUILayout.MaxWidth(1000), GUILayout.MinWidth(250) };
        
        
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);


        if (avatarModel == null) 
        {
            EditorGUILayout.HelpBox("Drag the avatar model here to continue.", MessageType.Info);
        } 
        else if (avatarModel) 
        {
            EditorGUILayout.HelpBox(avatarModel + " will be tested for correct settings.", MessageType.Info);
        } 
        else 
        {
            EditorGUILayout.HelpBox(avatarModel + " is empty.", MessageType.Info);
        }
        
        
        avatarModel = EditorGUILayout.ObjectField("Avatar Model", avatarModel, typeof(GameObject), true) as GameObject;

        AddLineAndSpace();
        
        EditorGUILayout.HelpBox("Current Target: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString(), MessageType.Info);
        
        GUILayout.BeginHorizontal("Switch Platforms", GUI.skin.window);

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.StandaloneWindows64))
        {
            //EditorGUILayout.HelpBox("Current Target: Android", MessageType.Info);
            if(GUILayout.Button("Switch to Windows"))
            {
                buildTarget = BuildTarget.StandaloneWindows64;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
            }
        }

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.Android))
        {
            //EditorGUILayout.HelpBox("Current Target: Windows", MessageType.Info);
            if(GUILayout.Button("Switch to Android"))
            {
                buildTarget = BuildTarget.Android;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;
            }
        }

        GUILayout.EndHorizontal();


        if (avatarModel)
            avatarName = avatarModel.name;
        else
            avatarName = "";

        
        DrawUILine(Color.blue);

        
        if (showExportSettings = EditorGUILayout.Foldout(showExportSettings, "Export Settings")) 
            ExportSettings();
        
        DrawUILine(Color.blue);



        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            GUI.color = Color.white;
            
            EditorGUILayout.HelpBox("Use this button first to get all references and add necessary scripts.", MessageType.Info);
            
            AvatarSetupComplete = playerAvatarScript != null && animator != null && animator.avatar != null && animator.avatar.isHuman;
            
            if (AvatarSetupComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            if (GUILayout.Button("Setup Avatar", GUILayout.Height(20)))
            {
                Debug.Log("Checking model");
                
                if (!avatarModel.GetComponentInChildren<PlayerAvatar>())
                {
                    playerAvatarScript = avatarModel.AddComponent<PlayerAvatar>();
                }
                
                playerAvatarScript = avatarModel.GetComponentInChildren<PlayerAvatar>();
                
                avatarNameString = avatarModel.name;
                
                animator = avatarModel.GetComponent<Animator>();

                if(animator == null)
                {
                    Debug.LogError("Animator not found. Adding Animator component.");
                    avatarModel.AddComponent<Animator>();
                    return;
                }
                else
                {
                    Debug.Log("Animator found");
                }
                
                if(animator.avatar == null)
                {
                    Debug.LogError("Avatar not found");
                    return;
                }
                else
                {
                    Debug.Log("Avatar found");
                }
                
                if (!animator.avatar.isHuman)
                {
                    Debug.Log("Avatar Model is not humanoid! Please convert rig to humanoid in the inspector first.");
                    EditorGUILayout.HelpBox("Avatar Model is not humanoid. Please convert rig to humanoid in the inspector first.", MessageType.Warning);
                    return;
                }
                
                if (avatarModel)
                {
                    animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Avatar Hand Animator");
                }
                
                if (!handPoseCopierScript)
                {
                    handPoseCopierScript = GameObject.Find("Hand Pose Copier").GetComponent<HandPoseCopier>();
                }

                if(handPoseCopierScript)
                {
                    if (animator)
                        handPoseCopierScript.avatarAnimator = animator;

                    if (playerAvatarScript)
                        handPoseCopierScript.playerAvatarScript = playerAvatarScript;
                }
                
                
                // Check if Folder for this Mod exists in MODS folder. If not, create one.
                activeModFolderPath = Path.Combine(UnityModsFolderPath, playerAvatarScript.gameObject.name);
                activeModPrefabPath = Path.Combine(activeModFolderPath, playerAvatarScript.gameObject.name);

                Debug.Log("activeModFolderPath: " + activeModFolderPath);
                Debug.Log("activeModPrefabPath: " + activeModPrefabPath);
                
                if (Directory.Exists(activeModFolderPath))
                {
                    Debug.Log("Avatar mod folder already exists");
                }
                else
                {
                    Debug.Log("Creating avatar mod folder");
                    Directory.CreateDirectory(activeModFolderPath);
                }
                
                
                playerAvatarScript.animator = animator;
                
                MapAvatarBody();
                
                SetupHealthEnergyReadout();
                
                AvatarSetupComplete = true;
            }
            
            if (FolderSetupComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            if(GUILayout.Button("Setup Folder Structure", GUILayout.Height(20)))
            {
                CheckForAvatarModPath();
            }
            
            GUI.color = Color.white;
            

            
            
            AddLineAndSpace();
            
            GUI.color = Color.white;
            //EditorGUILayout.HelpBox("Warning. These buttons will clear all material settings and any changes you made.", MessageType.Warning);
            EditorGUILayout.HelpBox("Collect all data about the Avatar's renderers and materials, so users can customize them in game. Warning. These buttons will clear all material settings and any changes you made.", MessageType.Info);
            
            if (CustomMaterialSettingsComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            
            GUILayout.BeginHorizontal("Material Settings", GUI.skin.window);
            
            if (GUILayout.Button("Create Custom Material Settings", GUILayout.Height(20)))
            {
                GenerateCustomMaterialSettings();
            }

            GUI.color = Color.red;
            if (GUILayout.Button("Clear Custom Material Settings", GUILayout.Height(20)))
            {
                ClearCustomMaterialSettings();
            }
            
            GUILayout.EndHorizontal();
            
            AddLineAndSpace();

            GUI.color = Color.white;
            
            
            EditorGUILayout.HelpBox("Create Hand Poses in Play Mode.", MessageType.Info);
            
            // Start Hand Pose Process in Play Mode
            if (CustomHandPoseSettingsComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }

            if (Application.isPlaying == false)
            {
                if (GUILayout.Button("Create Hand Poses", GUILayout.Height(20)))
                {
                    EditorApplication.EnterPlaymode();
                }
            }
            
            
        }
        
        AddLineAndSpace();
        
        bool canBuild = avatarModel != null;
        
        
        using (new EditorGUI.DisabledScope(!canBuild)) 
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                DisableDebugRenderers();
                
                // Save avatar before building addressable.
                Debug.Log("Saving Avatar Prefab");
                PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                
                ExportWindows();
                
                CheckForAvatarModPath();

                basePath = FormatForFileExplorer(Application.persistentDataPath + "/mod.io/04747/data/mods");
                exportPath = FormatForFileExplorer(basePath + "/" + EditorUserBuildSettings.selectedStandaloneTarget);
                
                if (openAfterExport)
                    EditorUtility.RevealInFinder(exportPath);
                
                RetrievePrefabInstanceFromScene();
            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                DisableDebugRenderers();
                
                // Save avatar before building addressable.
                Debug.Log("Saving Avatar Prefab");
                PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                
                ExportAndroid();
                
                CheckForAvatarModPath();

                basePath = FormatForFileExplorer(Application.persistentDataPath + "/mod.io/04747/data/mods");
                exportPath = FormatForFileExplorer(basePath + "/" + EditorUserBuildSettings.selectedStandaloneTarget);
                
                if (openAfterExport)
                    EditorUtility.RevealInFinder(exportPath);
                
                RetrievePrefabInstanceFromScene();
            }
            
            GUILayout.EndHorizontal();
        }
        
        
        AddLineAndSpace();

        
        using(new EditorGUI.DisabledScope(avatarModel == null))
        {
            EditorGUILayout.HelpBox(" Use this button to Finish Setup for current Avatar AFTER building the Addressable. Adds the Hand Pose JSON to the folder before compression.", MessageType.Info);
            if (GUILayout.Button("Finish Setup", GUILayout.Height(20)))
            {
                // Add json file to the Addressable folder we've created.
                
                if (!playerAvatarScript)
                {
                    if(avatarModel)
                        playerAvatarScript = avatarModel.GetComponentInChildren<PlayerAvatar>();
                }
                
                string handPoseCachePath = Path.Combine(Application.dataPath, "MODS" + "/" + playerAvatarScript.gameObject.name + "/AvatarModHandPoses.json");
                string avatarModFolderPath = CheckForAvatarModPath();
                
                if(avatarModFolderPath == "")
                    return;
                
                if (File.Exists(handPoseCachePath))
                {
                    Debug.Log("Hand Pose Cache File Exists");

                    string newHandPosePath = Path.Combine(avatarModFolderPath, "AvatarModHandPoses.json");

                    newHandPosePath = FormatPathKeepSpaces(newHandPosePath);

                    if (File.Exists(newHandPosePath))
                    {
                        Debug.Log("Hand Pose Script already exists in Local Build Path. Replacing...");
                        FileUtil.ReplaceFile(handPoseCachePath, newHandPosePath);
                    }
                    else
                    {
                        Debug.Log("Hand Pose Script moving to Local Build Path: " + newHandPosePath);
                        FileUtil.CopyFileOrDirectory(handPoseCachePath, newHandPosePath);
                    }

                }
                else
                {
                    Debug.Log("Hand Pose Cache JSON File Does Not Exist! Please enter Play Mode and use the Hand Pose button to create one.");
                }
                
                
                string builtModsFolderPath = Path.Combine(Application.persistentDataPath, "mod.io/04747/data/mods/");

                DirectoryInfo dirInfo = new DirectoryInfo(builtModsFolderPath);
                
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {
                    Debug.Log("File: " + fileInfo.Name);

                    if(fileInfo.Name == "StandaloneWindows64.zip" || fileInfo.Name == "Android.zip")
                        continue;
                    
                    File.Move(fileInfo.FullName, Path.Combine(avatarModFolderPath, fileInfo.Name));
                    
                    Debug.Log("Moved File: " + fileInfo.Name + " to: " + Path.Combine(avatarModFolderPath, fileInfo.Name));
                    
                    //FileUtil.CopyFileOrDirectory(fileInfo.FullName, Path.Combine(avatarModFolderPath, fileInfo.Name));
                    
                }
            }
        }
        
        
        AddLineAndSpace();
        
        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            if (GUILayout.Button("Save Avatar Prefab", GUILayout.Height(20)))
            {
                if (avatarModel)
                {
                    Debug.Log("Saving Avatar Prefab");
                    PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                }
            }
        }

        AddLineAndSpace();
        
        
        GUI.color = Color.white;
        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            GUILayout.BeginHorizontal("Debug Shapes", GUI.skin.window);

            //EditorGUILayout.HelpBox("Enables Debug shapes", MessageType.Info);
            if (GUILayout.Button("Enable All Debug Shapes", GUILayout.Height(20)))
            {
                // Turn on FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    EnableDebugRenderers();
                }
            }

            //EditorGUILayout.HelpBox("Disables Debug Shapes", MessageType.Info);
            if (GUILayout.Button("Disable All Debug Shapes", GUILayout.Height(20)))
            {
                // Turn off FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    DisableDebugRenderers();
                }
            }
        }
        
        GUILayout.EndHorizontal();
        
        
        EditorGUILayout.HelpBox("Use this button to reset this Mod Exporter Tab.", MessageType.Info);
        GUI.color = Color.red;
        if (GUILayout.Button("Clear Mod Exporter", GUILayout.Height(20)))
        {
            ResetButtonCompletionStatus();
        }
        
        
        
        // End the scroll view that we began above.
        EditorGUILayout.EndScrollView();
        
        //EditorGUILayout.EndHorizontal();
    }
    
    private void AddLine()
    {
        DrawUILine(Color.blue);
    }
    
    private void AddLineAndSpace()
    {
        GUILayout.Space(10);
        DrawUILine(Color.blue);
        GUILayout.Space(10);
    }
    
    

    private void ResetButtonCompletionStatus()
    {
        AvatarSetupComplete = false;
        CustomMaterialSettingsComplete = false;
        FolderSetupComplete = false;
        CustomHandPoseSettingsComplete = false;
    }

    private void RetrievePrefabInstanceFromScene()
    {
        if(playerAvatarScript)
        {
            Debug.Log("Still have Player Avatar Script reference");
            avatarModel = playerAvatarScript.gameObject;
        }
        
        if(avatarNameString != "")
        {
            Debug.Log("Still have Avatar Name String: " + avatarNameString);
            avatarModel = GameObject.Find(avatarNameString);
        }
    }
    

    [ContextMenu("Check For Avatar Mod Path")]
    private string CheckForAvatarModPath()
    {
        if (avatarModel)
        {
            playerAvatarScript = avatarModel.GetComponentInChildren<PlayerAvatar>();
        }
        
        if(playerAvatarScript == null)
        {
            return "";
        }
        
        string builtModsFolderPath = Path.Combine(Application.persistentDataPath, "mod.io/04747/data/mods/");

        Debug.Log("Setting path to StandaloneTarget: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString());
        string avatarModStandaloneWindows64FolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.StandaloneWindows64.ToString(), playerAvatarScript.gameObject.name));
        string avatarModAndroidFolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.Android.ToString(), playerAvatarScript.gameObject.name));

        if (Directory.Exists(avatarModStandaloneWindows64FolderPath))
        {
            Debug.Log("Mod Folder Build Path already exists: " + avatarModStandaloneWindows64FolderPath);
        }
        else
        {
            Debug.Log("Creating Avatar Mod StandaloneWindows64 Folder in Local Build Path: " + avatarModStandaloneWindows64FolderPath);
            Directory.CreateDirectory(avatarModStandaloneWindows64FolderPath);
        }

        if (Directory.Exists(avatarModAndroidFolderPath))
        {
            Debug.Log("Mod Folder Build Path already exists: " + avatarModAndroidFolderPath);
        }
        else
        {
            Debug.Log("Creating Avatar Mod Android Folder in Local Build Path: " + avatarModAndroidFolderPath);
            Directory.CreateDirectory(avatarModAndroidFolderPath);
        }
        
        
        FolderSetupComplete = true;

        if (buildTarget == BuildTarget.Android)
        {
            return avatarModAndroidFolderPath;
        }
        else
        {
            return avatarModStandaloneWindows64FolderPath;
        }
        
    }
    
    
    private void MapAvatarBody()
    {
        GetAllHumanBoneReferences();

        SetupPalmsAndSpawnPoints();
        
        SetFingerReferences();
    }


    private void GetAllHumanBoneReferences()
    {
        if (playerAvatarScript.avatarHead == null)
        {
            playerAvatarScript.avatarHead = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        
        if(playerAvatarScript.avatarEyes == null)
        {
            if (animator.GetBoneTransform(HumanBodyBones.Head).FindChildRecursive("Eyes Debug Capsule"))
            {
                playerAvatarScript.avatarEyes = animator.GetBoneTransform(HumanBodyBones.Head).FindChildRecursive("Eyes Debug Capsule");
            }
            else
            {
                
                Debug.Log("No Eye Bones Found. Eyes Debug Shape created. Please the Eyes Debug Shape to the eyes position.");
                
                GameObject eyes = Instantiate(Resources.Load("Eyes Debug Capsule", typeof(GameObject))) as GameObject;
                eyes.transform.SetParent(playerAvatarScript.avatarHead, true);
                
                playerAvatarScript.avatarEyes = eyes.transform;
                
                /*
                if (animator.GetBoneTransform(HumanBodyBones.LeftEye) != null && animator.GetBoneTransform(HumanBodyBones.RightEye) != null)
                {
                    // Create a new gameobject to hold the eyes (so we can rotate them
                    GameObject eyes = new GameObject("Eyes");
                    eyes.transform.parent = playerAvatarScript.avatarHead;

                    Transform leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                    Transform rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

                    eyes.transform.position = (leftEye.position + rightEye.position) / 2;
                    eyes.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                
                    playerAvatarScript.avatarEyes = eyes.transform;
                }
                else
                {
                    Debug.Log("No Eye Bones Found. Eyes Debug Shape created. Please the Eyes Debug Shape to the eyes position.");
                
                    GameObject eyes = Instantiate(Resources.Load("Eyes Debug Capsule", typeof(GameObject))) as GameObject;
                    eyes.transform.SetParent(playerAvatarScript.avatarHead, true);
                
                    playerAvatarScript.avatarEyes = eyes.transform;
                }
                */
            }
        }
                
        if(playerAvatarScript.leftHand == null)
        {
            playerAvatarScript.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        }
                
        if(playerAvatarScript.rightHand == null)
        {
            playerAvatarScript.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
                
        if(playerAvatarScript.leftForearm == null)
        {
            playerAvatarScript.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        }
                
        if(playerAvatarScript.rightForearm == null)
        {
            playerAvatarScript.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }
        
        if(playerAvatarScript.leftForearmTwist == null)
        {
            if (playerAvatarScript.leftForearm.parent)
            {
                foreach (Transform forearmChild in playerAvatarScript.leftForearm.parent)
                {
                    if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                        playerAvatarScript.leftForearmTwist = forearmChild;
                }

                if (playerAvatarScript.leftForearmTwist == null)
                {
                    foreach (Transform forearmChild in playerAvatarScript.leftForearm)
                    {
                        if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                            playerAvatarScript.leftForearmTwist = forearmChild;
                    }
                }
            }
        }
        
        if(playerAvatarScript.rightForearmTwist == null)
        {
            if (playerAvatarScript.rightForearm.parent)
            {
                foreach (Transform forearmChild in playerAvatarScript.rightForearm.parent)
                {
                    if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                        playerAvatarScript.rightForearmTwist = forearmChild;
                }
                
                if(playerAvatarScript.rightForearmTwist == null)
                {
                    foreach (Transform forearmChild in playerAvatarScript.rightForearm)
                    {
                        if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                            playerAvatarScript.rightForearmTwist = forearmChild;
                    }
                }
            }
        }
        
        
        // Fingers
        playerAvatarScript.leftIndexRoot = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        playerAvatarScript.leftIndexIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        playerAvatarScript.leftIndexEnd = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        
        playerAvatarScript.leftMiddleRoot = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        playerAvatarScript.leftMiddleIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        playerAvatarScript.leftMiddleEnd = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        
        playerAvatarScript.leftRingRoot = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        playerAvatarScript.leftRingIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        playerAvatarScript.leftRingEnd = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        
        playerAvatarScript.leftPinkyRoot = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        playerAvatarScript.leftPinkyIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        playerAvatarScript.leftPinkyEnd = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        
        playerAvatarScript.leftThumbRoot = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        playerAvatarScript.leftThumbIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        playerAvatarScript.leftThumbEnd = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        
        
        playerAvatarScript.rightIndexRoot = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        playerAvatarScript.rightIndexIntermediate = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        playerAvatarScript.rightIndexEnd = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        
        playerAvatarScript.rightMiddleRoot = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        playerAvatarScript.rightMiddleIntermediate = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        playerAvatarScript.rightMiddleEnd = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        
        playerAvatarScript.rightRingRoot = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
        playerAvatarScript.rightRingIntermediate = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        playerAvatarScript.rightRingEnd = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
        
        playerAvatarScript.rightPinkyRoot = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        playerAvatarScript.rightPinkyIntermediate = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        playerAvatarScript.rightPinkyEnd = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        
        playerAvatarScript.rightThumbRoot = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        playerAvatarScript.rightThumbIntermediate = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        playerAvatarScript.rightThumbEnd = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
    }
    

    private void SetupPalmsAndSpawnPoints()
    {

        // Add Palm Transform if not already created.
        GameObject existingPalmLeft = null;
        Transform leftPalmTransform;
        bool foundPalmLeft = false;
        
        
        foreach (Transform handChild in playerAvatarScript.leftHand)
        {
            if (handChild.name.Contains("Avatar Left Palm Spawnpoints Prefab"))
            {
                foundPalmLeft = true;
                existingPalmLeft = handChild.gameObject;
            }
            else
            {
                existingPalmLeft = null;
            }
        }
        
        if (!foundPalmLeft)
        {
            //var tip = new GameObject("Tip");

            //var Palm = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            //Palm.name = "Palm";
            
            GameObject Palm = Instantiate(Resources.Load("Avatar Left Palm Spawnpoints Prefab", typeof(GameObject))) as GameObject;
            
            leftPalmTransform = Palm.transform;
            leftPalmTransform.parent = playerAvatarScript.leftHand;
            leftPalmTransform.localPosition = Vector3.zero;
            leftPalmTransform.localRotation = Quaternion.identity;
            
            if (handPoseCopierScript)
            {
                handPoseCopierScript.leftHandWeaponShapes.Clear();
                
                foreach (Transform childTransform in Palm.transform)
                {
                    if(childTransform.name == "Sphere" || childTransform.name.Contains("Palm Shape"))
                        continue;
                    
                    handPoseCopierScript.leftHandWeaponShapes.Add(childTransform.gameObject);
                }
            }
        }
        else
        {
            leftPalmTransform = existingPalmLeft.transform;
            //leftPalmTransform.localScale = Vector3.one * 0.02f;
        }
        
        playerAvatarScript.leftPalm = leftPalmTransform;
        playerAvatarScript.leftHandSpawnPointParent = leftPalmTransform;

        
        // Add Palm Transform if not already created.
        GameObject existingPalmRight = null;
        Transform rightPalmTransform;
        bool foundPalmRight = false;

        foreach (Transform handChild in playerAvatarScript.rightHand)
        {
            if (handChild.name.Contains("Avatar Right Palm Spawnpoints Prefab"))
            {
                foundPalmRight = true;
                existingPalmRight = handChild.gameObject;
            }
            else
            {
                existingPalmRight = null;
            }
        }
        

        if (!foundPalmRight)
        {
            //var tip = new GameObject("Tip");

            //var Palm = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            //Palm.name = "Palm";
            
            GameObject Palm = Instantiate(Resources.Load("Avatar Right Palm Spawnpoints Prefab", typeof(GameObject))) as GameObject;
            
            rightPalmTransform = Palm.transform;
            rightPalmTransform.parent = playerAvatarScript.rightHand;
            rightPalmTransform.localPosition = Vector3.zero;
            rightPalmTransform.localRotation = Quaternion.identity;

            if (handPoseCopierScript)
            {
                handPoseCopierScript.rightHandWeaponShapes.Clear();
                
                foreach (Transform childTransform in Palm.transform)
                {
                    if(childTransform.name == "Sphere" || childTransform.name.Contains("Palm Shape"))
                        continue;
                    
                    handPoseCopierScript.rightHandWeaponShapes.Add(childTransform.gameObject);
                }
            }
        }
        else
        {
            rightPalmTransform = existingPalmRight.transform;
            //rightPalmTransform.localScale = Vector3.one * 0.02f;
        }
        
        playerAvatarScript.rightPalm = rightPalmTransform;
        playerAvatarScript.rightHandSpawnPointParent = rightPalmTransform;
    }
    
    
    private void SetFingerReferences()
    {
        // Need to add FingerTip transforms if not already created.
        for (int i = 0; i < 10; i++)
        {
            var last = playerAvatarScript.leftIndexEnd;
            
            switch (i)
            {
                case 0:
                    if(!playerAvatarScript.leftIndexEnd)
                        continue;
                    
                    last = playerAvatarScript.leftIndexEnd;
                    break;
                
                case 1:
                    if(!playerAvatarScript.leftMiddleEnd)
                        continue;
                    
                    last = playerAvatarScript.leftMiddleEnd;
                    break;
                
                case 2:
                    if(!playerAvatarScript.leftPinkyEnd)
                        continue;
                    
                    last = playerAvatarScript.leftPinkyEnd;
                    break;
                
                case 3:
                    if(!playerAvatarScript.leftRingEnd)
                        continue;
                    
                    last = playerAvatarScript.leftRingEnd;
                    break;
                
                case 4:
                    if(!playerAvatarScript.leftThumbEnd)
                        continue;
                    
                    last = playerAvatarScript.leftThumbEnd;
                    break;
                
                case 5:
                    if(!playerAvatarScript.rightIndexEnd)
                        continue;
                    
                    last = playerAvatarScript.rightIndexEnd;
                    break;
                
                case 6:
                    if(!playerAvatarScript.rightMiddleEnd)
                        continue;
                    
                    last = playerAvatarScript.rightMiddleEnd;
                    break;
                
                case 7:
                    if(!playerAvatarScript.rightPinkyEnd)
                        continue;
                    
                    last = playerAvatarScript.rightPinkyEnd;
                    break;
                
                case 8:
                    if(!playerAvatarScript.rightRingEnd)
                        continue;
                    
                    last = playerAvatarScript.rightRingEnd;
                    break;
                
                case 9:
                    if(!playerAvatarScript.rightThumbEnd)
                        continue;
                    
                    last = playerAvatarScript.rightThumbEnd;
                    break;
            }
            
            var existing = last.Find("FingerTip");
            Transform tipTransform;
            if (!existing)
            {
                //var tip = new GameObject("Tip");

                var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "FingerTip";
                tip.GetComponent<SphereCollider>().enabled = false;
                tipTransform = tip.transform;
                
                //tipTransform.localPosition = Vector3.zero;
                //tipTransform.localRotation = Quaternion.identity;
                tipTransform.localScale = Vector3.one * 0.02f;

                tip.transform.position = last.position;
                
                tipTransform.SetParent(last);
            }
            else
            {
                tipTransform = existing;
                //tipTransform.localScale = Vector3.one * 0.02f;
            }


            if (playerAvatarScript.fingerTips == null)
                playerAvatarScript.fingerTips = new List<Transform>();
            
            if(playerAvatarScript.fingerTips.Contains(tipTransform) == false)
                playerAvatarScript.fingerTips.Add(tipTransform);

            switch (i)
            {
                case 0:
                    playerAvatarScript.leftIndexTip = tipTransform;
                    break;
                
                case 1:
                    playerAvatarScript.leftMiddleTip = tipTransform;
                    break;
                
                case 2:
                    playerAvatarScript.leftPinkyTip = tipTransform;
                    break;
                
                case 3:
                    playerAvatarScript.leftRingTip = tipTransform;
                    break;
                
                case 4:
                    playerAvatarScript.leftThumbTip = tipTransform;
                    break;
                
                case 5:
                    playerAvatarScript.rightIndexTip = tipTransform;
                    break;
                
                case 6:
                    playerAvatarScript.rightMiddleTip = tipTransform;
                    break;
                
                case 7:
                    playerAvatarScript.rightPinkyTip = tipTransform;
                    break;
                
                case 8:
                    playerAvatarScript.rightRingTip = tipTransform;
                    break;
                
                case 9:
                    playerAvatarScript.rightThumbTip = tipTransform;
                    break;
            }
        }


        // Clear list and re-populate with all finger transforms.
        if(playerAvatarScript.fingerBoneTransforms != null)
            playerAvatarScript.fingerBoneTransforms.Clear();
        else
        {
            playerAvatarScript.fingerBoneTransforms = new List<Transform>();
        }

        if(playerAvatarScript.leftIndexRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexRoot);
        
        if(playerAvatarScript.leftIndexIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexIntermediate);
        
        if(playerAvatarScript.leftIndexEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexEnd);
        
        if(playerAvatarScript.leftMiddleRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleRoot);
        
        if(playerAvatarScript.leftMiddleIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleIntermediate);
        
        if(playerAvatarScript.leftMiddleEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleEnd);
        
        if(playerAvatarScript.leftPinkyRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyRoot);
        
        if(playerAvatarScript.leftPinkyIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyIntermediate);
        
        if(playerAvatarScript.leftPinkyEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyEnd);

        if(playerAvatarScript.leftRingRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingRoot);
        
        if(playerAvatarScript.leftRingIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingIntermediate);
        
        if(playerAvatarScript.leftRingEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingEnd);
        
        if(playerAvatarScript.leftThumbRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbRoot);
        
        if(playerAvatarScript.leftThumbIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbIntermediate);
        
        if(playerAvatarScript.leftThumbEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbEnd);


        if(playerAvatarScript.rightIndexRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexRoot);
        
        if(playerAvatarScript.rightIndexIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexIntermediate);
        
        if(playerAvatarScript.rightIndexEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexEnd);

        if(playerAvatarScript.rightMiddleRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleRoot);
        
        if(playerAvatarScript.rightMiddleIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleIntermediate);
        
        if(playerAvatarScript.rightMiddleEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleEnd);
        
        if(playerAvatarScript.rightPinkyRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyRoot);
        
        if(playerAvatarScript.rightPinkyIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyIntermediate);
        
        if(playerAvatarScript.rightPinkyEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyEnd);
        
        if(playerAvatarScript.rightRingRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingRoot);
        
        if(playerAvatarScript.rightRingIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingIntermediate);
        
        if(playerAvatarScript.rightRingEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingEnd);
        
        if(playerAvatarScript.rightThumbRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbRoot);
        
        if(playerAvatarScript.rightThumbIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbIntermediate);
        
        if(playerAvatarScript.rightThumbEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbEnd);

    }


    private void SetupHealthEnergyReadout()
    {
        EnergyHealthBar energyHealthBar = playerAvatarScript.GetComponentInChildren<EnergyHealthBar>();

        GameObject healthBar;
        
        if (energyHealthBar)
        {
            healthBar = energyHealthBar.gameObject;
        }
        else
        {
            healthBar = Instantiate(Resources.Load<GameObject>("Energy and Health Bars"));
            
            if (playerAvatarScript.rightForearmTwist)
            {
                healthBar.transform.SetParent(playerAvatarScript.rightForearmTwist);
            }
            else
            {
                healthBar.transform.SetParent(playerAvatarScript.rightForearm);
            }

            healthBar.transform.localPosition = Vector3.zero;
            
            energyHealthBar = healthBar.GetComponent<EnergyHealthBar>();
        }
        
        if(!healthBar)
            return;


        playerAvatarScript.healthAndEnergyBar = healthBar;
        

        if (energyHealthBar)
        {
            playerAvatarScript.healthParentObject = energyHealthBar.healthParentObject;
            playerAvatarScript.healthText = energyHealthBar.healthText;
            playerAvatarScript.healthSlider = energyHealthBar.healthSlider;
            
            playerAvatarScript.energyParentObject = energyHealthBar.energyParentObject;
            playerAvatarScript.energyText = energyHealthBar.energyText;
            playerAvatarScript.energySlider = energyHealthBar.energySlider;
        }
    }
    

    private void GenerateCustomMaterialSettings()
    {
        if(!avatarModel || !playerAvatarScript)
            return;
                
        List<Renderer> avatarRenderers = avatarModel.GetComponentsInChildren<Renderer>().ToList();

        for (int i = 0; i < avatarRenderers.Count; i++)
        {
            if(avatarRenderers[i].name.Contains("FingerTip") || avatarRenderers[i].name.Contains("Palm") || avatarRenderers[i].name == "Cube" 
               || avatarRenderers[i].name == "Capsule" || avatarRenderers[i].transform.parent.name.Contains("Palm Shape") || avatarRenderers[i].name.Contains("Don't Move"))
            {
                avatarRenderers.RemoveAt(i);
                i--;
            }
        }
        
        CustomMaterialSetting[] customSettingsArray = new CustomMaterialSetting[avatarRenderers.Count];
                
        for (int i = 0; i < avatarRenderers.Count; i++)
        {
            if (avatarRenderers[i])
            {
                CustomMaterialSetting newSetting = new CustomMaterialSetting();
                
                newSetting.renderer = avatarRenderers[i];

                newSetting.rendererNameForUserInterface = newSetting.renderer.name;
                        
                if(newSetting.renderer && newSetting.renderer.sharedMaterial)
                    newSetting.originalMaterial = newSetting.renderer.sharedMaterial;
    
                
                newSetting.originalMaterialMainTexture = newSetting.renderer.sharedMaterial.mainTexture;
                newSetting.originalMaterialUsingTexture = true;

                newSetting.activeMaterial = newSetting.originalMaterial;
                newSetting.activeMaterialMainTexture = newSetting.originalMaterialMainTexture;
                newSetting.activeMaterialUsingTexture = newSetting.originalMaterialUsingTexture;
                        
                newSetting.color = newSetting.renderer.sharedMaterial.color;

                customSettingsArray[i] = newSetting;
            }
        }


        
        for (int i = 0; i < customSettingsArray.Length; i++)
        {
            bool alreadyExists = false;
            
            for (int j = 0; j < playerAvatarScript.customMaterialSettings.Count; j++)
            {
                if (customSettingsArray[i].renderer == playerAvatarScript.customMaterialSettings[j].renderer)
                {
                    alreadyExists = true;
                }
            }
            
            if(alreadyExists == false)
                playerAvatarScript.customMaterialSettings.Add(customSettingsArray[i]);
        }
        
        CustomMaterialSettingsComplete = true;
    }


    private void ClearCustomMaterialSettings()
    {
        if(playerAvatarScript)
            playerAvatarScript.customMaterialSettings.Clear();
        
        CustomMaterialSettingsComplete = false;
    }


    private void EnableDebugRenderers()
    {
        if (avatarModel)
            playerAvatarScript = avatarModel.GetComponent<PlayerAvatar>();
        
        if(playerAvatarScript.fingerTips != null)
        {
            foreach (var tip in playerAvatarScript.fingerTips)
            {
                if (tip.GetComponent<MeshRenderer>())
                {
                    tip.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        if (playerAvatarScript.leftPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.leftPalm)
            {
                childTransform.gameObject.SetActive(true);
            }

        }
        
        if (playerAvatarScript.rightPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.rightPalm)
            {
                childTransform.gameObject.SetActive(true);
            }
        }
        
        if (playerAvatarScript.avatarEyes)
        {
            playerAvatarScript.avatarEyes.gameObject.SetActive(true);
            
            foreach (Transform childTransform in playerAvatarScript.avatarEyes)
            {
                childTransform.gameObject.SetActive(true);
            }
        }
    }

    private void DisableDebugRenderers()
    {
        if (avatarModel)
            playerAvatarScript = avatarModel.GetComponent<PlayerAvatar>();
        
        if(playerAvatarScript.fingerTips != null)
        {
            foreach (var tip in playerAvatarScript.fingerTips)
            {
                if (tip.GetComponent<MeshRenderer>())
                {
                    tip.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        if (playerAvatarScript.leftPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.leftPalm)
            {
                childTransform.gameObject.SetActive(false);
            }

        }
        
        if (playerAvatarScript.rightPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.rightPalm)
            {
                childTransform.gameObject.SetActive(false);
            }
        }
        
        if (playerAvatarScript.avatarEyes)
        {
            playerAvatarScript.avatarEyes.gameObject.SetActive(true);
            
            foreach (Transform childTransform in playerAvatarScript.avatarEyes)
            {
                childTransform.gameObject.SetActive(false);
            }
        }

        if (handPoseCopierScript)
        {
            if(handPoseCopierScript.leftHandWeaponShapes != null)
            {
                foreach (var shape in handPoseCopierScript.leftHandWeaponShapes)
                {
                    if(shape)
                        shape.SetActive(false);
                }
            }
            
            if(handPoseCopierScript.rightHandWeaponShapes != null)
            {
                foreach (var shape in handPoseCopierScript.rightHandWeaponShapes)
                {
                    if(shape)
                        shape.SetActive(false);
                }
            }
        }
    }

    private void ExportSettings() 
    {
        basePath = FormatForFileExplorer(Application.persistentDataPath + "/mod.io/04747/data/mods");
        exportPath = FormatForFileExplorer(basePath + "/" + EditorUserBuildSettings.selectedStandaloneTarget);

        if (GUILayout.Button("Open Export Folder"))
            EditorUtility.RevealInFinder(basePath + "/");

        EditorGUIUtility.labelWidth = 200;
        openAfterExport = EditorGUILayout.Toggle("Open Export Folder On Complete", openAfterExport);
        EditorPrefs.SetBool("OpenAfterExport", openAfterExport);

        GUILayout.Space(10);
        GUILayout.Label("Export path: " + exportPath, EditorStyles.label);
    }


    private void DisplayError(string title, string error) 
    {
        EditorUtility.DisplayDialog(title, error, "Ok", "");
    }

    private bool DisplayWarning(string title, string warning) 
    {
        return EditorUtility.DisplayDialog(title, warning, "Continue", "Cancel");
    }
    
    void ExportWindows() 
    {
        buildTarget = BuildTarget.StandaloneWindows64;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
        BuildAddressable(BuildTarget.StandaloneWindows64);
    }

    void ExportAndroid() 
    {
        buildTarget = BuildTarget.Android;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;
        BuildAddressable(BuildTarget.Android);
    }
    
    
    private void BuildAddressable(BuildTarget buildTarget) 
    {
        if (Directory.Exists(exportPath))
        {
            //Debug.Log("Deleting old export folder at " + exportPath);
            //DeleteFolder(exportPath);
        }


        string prefabRelativeToProjectPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(avatarModel);
        
        Debug.Log("Prefab relative to project path: " + prefabRelativeToProjectPath);
        
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Default Local Group");
        var guid = AssetDatabase.AssetPathToGUID(prefabRelativeToProjectPath);
        
        
        // Set the bundles' naming style to custom, and make the name as unique as possible to avoid cross-mod conflicts.
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleNaming = ShaderBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleCustomNaming = avatarName + "Shaders" + DateTime.Now.Minute + DateTime.Now.Second;
        
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleCustomNaming = avatarName + "Mono" + DateTime.Now.Minute + DateTime.Now.Second;


        if (group == null || guid == null)
            return;

        foreach (AddressableAssetEntry entry in group.entries.ToList())
            group.RemoveAssetEntry(entry);

        var e = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, false);
        var entriesAdded = new List<AddressableAssetEntry> { e };
        e.SetLabel("Player Avatar", true, true, false);

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
        AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

        string windowsbuildPath = "C:/Users/Public/mod.io/4747/mods/{LOCAL_FILE_NAME}/";
        string editorPath = "{UnityEngine.Application.persistentDataPath}/mod.io/04747/data/mods/{LOCAL_FILE_NAME}/";

        Debug.Log("Build target: " + buildTarget);

        // We can dynamically change the LOAD path here, and replace the LOCAL_FILE_NAME with: MOD-ID/BUILD TARGET/AVATAR NAME
        if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            Debug.Log("Setting load path for windows");
            AddressableAssetSettingsDefaultObject.Settings.profileSettings
                .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "LocalLoadPath", windowsbuildPath); 
            
        }
        else if (buildTarget == BuildTarget.Android)
        {
            Debug.Log("Setting load path for android");
            AddressableAssetSettingsDefaultObject.Settings.profileSettings
                .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "LocalLoadPath", "{UnityEngine.Application.persistentDataPath}/mod.io/4747/mods/{LOCAL_FILE_NAME}/");
            
        }


        AddressableAssetSettingsDefaultObject.Settings.profileSettings
            .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "LocalBuildPath", "[UnityEngine.Application.persistentDataPath]" + "/" + "mod.io/04747/data/mods/");

        
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
    }
    
    
    

    private string FormatPath(string path)
    {
        return path.Replace(" ", "").Replace(@"\", "/");
    }
    
    private string FormatPathKeepSpaces(string path)
    {
        return path.Replace(@"\", "/");
    }
    
    private string FormatForFileExplorer(string path)
    {
        return path.Replace("/", @"\");
    }
    
    
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10) 
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
    
}
#endif
