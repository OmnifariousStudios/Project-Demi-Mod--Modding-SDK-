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
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ProjectDemiModMapExporter : EditorWindow
{
    string exportPath = "";
    string basePath = "";

    bool showExportSettings = true;
    private bool openAfterExport = true;

    public bool FolderSetupComplete = false;

    public Scene currentScene;
    public string currentSceneName;

    public SceneController currentSceneController;
    public GameObject currentPlayerSpawnpoint;
    [FormerlySerializedAs("currentEnemySpawnpoint")] public GameObject currentEnemySpawnpointsHolder;


    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;



    [MenuItem("Project Demigod/Map Mod Exporter")]
    public static void ShowMapWindow()
    {
        GetWindow<ProjectDemiModMapExporter>("Map Mod Exporter");
    }

    private void Awake()
    {
        if (buildTarget == BuildTarget.NoTarget)
            buildTarget = BuildTarget.StandaloneWindows64;
    }


    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80;
        GUILayout.Label("Project Demigod Mod Exporter", EditorStyles.largeLabel);
        GUILayout.Space(10);


        GUILayoutOption[] options = { GUILayout.MaxWidth(1000), GUILayout.MinWidth(250) };
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);




        #region SwitchPlatforms

        EditorGUILayout.HelpBox("Current Target: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString(), MessageType.Info);

        GUILayout.BeginHorizontal("Switch Platforms", GUI.skin.window);

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.StandaloneWindows64))
        {
            //EditorGUILayout.HelpBox("Current Target: Android", MessageType.Info);
            if (GUILayout.Button("Switch to Windows"))
            {
                buildTarget = BuildTarget.StandaloneWindows64;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
            }
        }

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.Android))
        {
            //EditorGUILayout.HelpBox("Current Target: Windows", MessageType.Info);
            if (GUILayout.Button("Switch to Android"))
            {
                buildTarget = BuildTarget.Android;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;
            }
        }

        GUILayout.EndHorizontal();

        #endregion

        #region Scene Setup


        if (GUILayout.Button("Setup Current Scene"))
        {
            GetCurrentScene();
            AddSceneController();
            AddPlayerSpawnPoint();
            AddEnemySpawnPointHolder();
            AddInterfaceSpawnPoints();
            AddShieldWalls();
            DestroyCamera();
            GetSceneLights();
        }
        
        if (GUILayout.Button("Add Enemy Spawnpoint"))
        {
            CreateEnemySpawnPoint();
        }

        
        if (GUILayout.Button("Bake Nav Mesh"))
        {
            NavMeshBuilder.BuildNavMesh();
        }
        
        if (GUILayout.Button("Bake Lighting"))
        {
            Lightmapping.Bake();
        }
        
        if (GUILayout.Button("Bake Occlusion Culling"))
        {
            StaticOcclusionCulling.Compute();
        }

        #endregion

        #region Setup Folders

        using (new EditorGUI.DisabledScope(currentSceneController == null))
        {
            if (GUILayout.Button("Setup Folder Structure", GUILayout.Height(20)))
            {
                CheckForMapModPath();
            }
        }

        #endregion

        #region Shield Wall Renderers

        if(GUILayout.Button("Enable Shield Wall Renderers"))
        {
            EnableShieldWallRenderers();
        }
        
        if (GUILayout.Button("Disable Shield Wall Renderers"))
        {
            DisableShieldWallRenderers();
        }

        #endregion


        #region Build Addressables

        bool canBuild = currentSceneController != null;


        using (new EditorGUI.DisabledScope(!canBuild))
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                DisableInterfaceSpawnPoints();

                DisableShieldWallRenderers();

                ExportWindows();

                CheckForMapModPath();

                basePath = FormatForFileExplorer(Application.persistentDataPath + "/mod.io/04747/data/mods");
                exportPath = FormatForFileExplorer(basePath + "/" + EditorUserBuildSettings.selectedStandaloneTarget);

                if (openAfterExport)
                    EditorUtility.RevealInFinder(exportPath);


            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                DisableInterfaceSpawnPoints();

                DisableShieldWallRenderers();

                ExportAndroid();

                CheckForMapModPath();

                basePath = FormatForFileExplorer(Application.persistentDataPath + "/mod.io/04747/data/mods");
                exportPath = FormatForFileExplorer(basePath + "/" + EditorUserBuildSettings.selectedStandaloneTarget);

                if (openAfterExport)
                    EditorUtility.RevealInFinder(exportPath);


            }

            GUILayout.EndHorizontal();
        }


        AddLineAndSpace();

        #endregion


        #region Finish Setup

        EditorGUILayout.HelpBox(" Use this button to Finish Setup AFTER building the Addressable.", MessageType.Info);
        if (GUILayout.Button("Finish Setup", GUILayout.Height(20)))
        {

            DisableInterfaceSpawnPoints();
            DisableShieldWallRenderers();
            DisableEnemySpawnpointShapes();
            
            string mapModFolderPath = CheckForMapModPath();

            if (mapModFolderPath == "")
                return;


            string builtModsFolderPath = Path.Combine(Application.persistentDataPath, "mod.io/04747/data/mods/");

            DirectoryInfo dirInfo = new DirectoryInfo(builtModsFolderPath);

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                Debug.Log("File: " + fileInfo.Name);

                if (fileInfo.Name == "StandaloneWindows64.zip" || fileInfo.Name == "Android.zip")
                    continue;

                File.Move(fileInfo.FullName, Path.Combine(mapModFolderPath, fileInfo.Name));

                Debug.Log("Moved File: " + fileInfo.Name + " to: " + Path.Combine(mapModFolderPath, fileInfo.Name));

                //FileUtil.CopyFileOrDirectory(fileInfo.FullName, Path.Combine(avatarModFolderPath, fileInfo.Name));

            }
        }

        #endregion


        EditorGUILayout.EndScrollView();
    }


    public void GetCurrentScene()
    {
        currentScene = EditorSceneManager.GetActiveScene();
        currentSceneName = currentScene.name;

        Debug.Log("Current Scene: " + currentSceneName);
    }

    void AddSceneController()
    {
        GameObject sceneController = GameObject.Find("Scene Controller");

        if (sceneController == null)
        {
            GameObject newSceneController = new GameObject("Scene Controller");
            currentSceneController = newSceneController.AddComponent<SceneController>();
            currentSceneController.thisSceneUsesUmbraOcclusionCulling = true;
        }
        else
        {
            currentSceneController = sceneController.GetComponent<SceneController>();
        }
    }

    void AddPlayerSpawnPoint()
    {
        GameObject playerSpawnPoint = GameObject.Find("Player Spawnpoint");

        if (playerSpawnPoint == null)
        {
            GameObject newPlayerSpawnPoint = new GameObject("Player Spawnpoint");
            newPlayerSpawnPoint.transform.position = Vector3.zero;
            newPlayerSpawnPoint.transform.rotation = Quaternion.identity;

            currentPlayerSpawnpoint = newPlayerSpawnPoint;

            if (currentSceneController)
            {
                currentSceneController.playerStartPoint = currentPlayerSpawnpoint;
            }
        }
        else
        {
            currentPlayerSpawnpoint = playerSpawnPoint;

            if (currentSceneController)
            {
                currentSceneController.playerStartPoint = currentPlayerSpawnpoint;
            }
        }
    }

    void AddEnemySpawnPointHolder()
    {
        GameObject newEnemySpawnPointsHolder = GameObject.Find("Enemy Spawnpoints Holder");

        if (newEnemySpawnPointsHolder == null)
        {
            newEnemySpawnPointsHolder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            
            newEnemySpawnPointsHolder.GetComponent<CapsuleCollider>().enabled = false;
            
            newEnemySpawnPointsHolder.name = "Enemy Spawnpoints Holder";
            
            newEnemySpawnPointsHolder.transform.position = Vector3.zero;
            newEnemySpawnPointsHolder.transform.rotation = Quaternion.identity;

            currentEnemySpawnpointsHolder = newEnemySpawnPointsHolder;

            if (currentSceneController)
            {
                currentSceneController.enemySpawnpointsHolder = currentEnemySpawnpointsHolder;
            }
            
            CreateEnemySpawnPoint();
        }
        else
        {
            currentEnemySpawnpointsHolder = newEnemySpawnPointsHolder;

            if (currentSceneController)
            {
                currentSceneController.enemySpawnpointsHolder = currentEnemySpawnpointsHolder;
            }
        }
    }

    void CreateEnemySpawnPoint()
    {
        if(currentSceneController == null || currentSceneController.enemySpawnpointsHolder == null)
        {
            GetCurrentScene();
            AddSceneController();
            AddPlayerSpawnPoint();
            AddEnemySpawnPointHolder();
        }
        
        GameObject newEnemySpawnPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        newEnemySpawnPoint.GetComponent<BoxCollider>().enabled = false;
        newEnemySpawnPoint.name = "Enemy Spawnpoint";
        newEnemySpawnPoint.transform.parent = currentEnemySpawnpointsHolder.transform;
        newEnemySpawnPoint.transform.localPosition = Vector3.zero;
        newEnemySpawnPoint.transform.localRotation = Quaternion.identity;
    }


    void AddInterfaceSpawnPoints()
    {
        GameObject avatarCalibratorShape = GameObject.Find("Avatar Calibrator Shape");
        
        GameObject levelSelectShape = GameObject.Find("Level Select Shape");
        
        GameObject playerArmoryShape = GameObject.Find("Player Armory Shape");
        
        GameObject enemySpawnerShape = GameObject.Find("Enemy Spawn Controller Shape");
        
        if(!avatarCalibratorShape)
        {
            avatarCalibratorShape = Instantiate(Resources.Load("Avatar Calibrator Shape", typeof(GameObject))) as GameObject;
            avatarCalibratorShape.name = "Avatar Calibrator Shape";
        }
        
        if(!levelSelectShape)
        {
            levelSelectShape = Instantiate(Resources.Load("Level Select Shape", typeof(GameObject))) as GameObject;
            levelSelectShape.name = "Level Select Shape";
        }
    
        if(!playerArmoryShape)
        {
            playerArmoryShape = Instantiate(Resources.Load("Player Armory Shape", typeof(GameObject))) as GameObject;
            playerArmoryShape.name = "Player Armory Shape";
        }
        
        if(!enemySpawnerShape)
        {
            enemySpawnerShape = Instantiate(Resources.Load("Enemy Spawn Controller Shape", typeof(GameObject))) as GameObject;
            enemySpawnerShape.name = "Enemy Spawn Controller Shape";
        }

        if (!currentSceneController)
            AddSceneController();

        if (currentSceneController)
        {
            if (avatarCalibratorShape)
                currentSceneController.avatarCalibratorTransform = avatarCalibratorShape.transform;

            if (levelSelectShape)
                currentSceneController.levelSelectTransform = levelSelectShape.transform;

            if (playerArmoryShape)
                currentSceneController.playerArmoryTransform = playerArmoryShape.transform;

            if (enemySpawnerShape)
                currentSceneController.enemySpawnerTransform = enemySpawnerShape.transform;
        }
    }

    void DisableInterfaceSpawnPoints()
    {
        if (currentSceneController)
        {
            if (currentSceneController.avatarCalibratorTransform)
                currentSceneController.avatarCalibratorTransform.gameObject.SetActive(false);

            if (currentSceneController.levelSelectTransform)
                currentSceneController.levelSelectTransform.gameObject.SetActive(false);

            if (currentSceneController.playerArmoryTransform)
                currentSceneController.playerArmoryTransform.gameObject.SetActive(false);

            if (currentSceneController.enemySpawnerTransform)
                currentSceneController.enemySpawnerTransform.gameObject.SetActive(false);
        }
    }

    void AddShieldWalls()
    {
        GameObject shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }

        if (shieldWalls == null)
        {
            GameObject newShieldWalls = Instantiate(Resources.Load("Shield Walls", typeof(GameObject))) as GameObject;
            newShieldWalls.name = "Shield Walls";
            newShieldWalls.transform.position = Vector3.zero;
            newShieldWalls.transform.rotation = Quaternion.identity;
        }
    }

    void EnableShieldWallRenderers()
    {
        GameObject shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }
        
        if (shieldWalls)
        {
            foreach (var VARIABLE in shieldWalls.GetComponentsInChildren<Renderer>())
            {
                VARIABLE.enabled = true;
            }
        }
    }

    private void DisableShieldWallRenderers()
    {
        GameObject shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }
        
        if (shieldWalls)
        {
            foreach (var VARIABLE in shieldWalls.GetComponentsInChildren<Renderer>())
            {
                VARIABLE.enabled = false;
            }
        }
    }

    private void DisableEnemySpawnpointShapes()
    {
        if (currentSceneController == null || currentSceneController.enemySpawnpointsHolder == null)
        {
            return;
        }
        else
        {
            currentSceneController.enemySpawnpointsHolder.GetComponent<MeshRenderer>().enabled = false;

            foreach (var childMesh in currentSceneController.enemySpawnpointsHolder.GetComponentsInChildren<MeshRenderer>())
            {
                childMesh.enabled = false;
            }
        }
    }


    void DestroyCamera()
    {
        if (Camera.main)
        {
            DestroyImmediate(Camera.main.gameObject);
        }
        else
        {
            GameObject camera = GameObject.Find("Camera");
            
            if(camera)
                DestroyImmediate(camera);
        }
    }

    void GetSceneLights()
    {
        if (currentSceneController)
        {
            currentSceneController.sceneLights = FindObjectsOfType<Light>().ToList();
        }
    }
    
    
    [ContextMenu("Check For Avatar Mod Path")]
    private string CheckForMapModPath()
    {
        if (currentSceneController == null)
        {
            AddSceneController();
        }
        
        if(currentSceneController == null)
        {
            return "";
        }
        
        string builtModsFolderPath = Path.Combine(Application.persistentDataPath, "mod.io/04747/data/mods/");

        Debug.Log("Checking current scene: " + currentSceneName);
        
        Debug.Log("Setting path to StandaloneTarget: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString());
        string avatarModStandaloneWindows64FolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.StandaloneWindows64.ToString(), currentSceneName));
        string avatarModAndroidFolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.Android.ToString(), currentSceneName));

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

        if(currentSceneName == "" || currentSceneName == null)
            GetCurrentScene();

        string sceneRelativeToProjectPath = currentScene.path;
        
        Debug.Log("Prefab relative to project path: " + sceneRelativeToProjectPath);
        
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Default Local Group");
        var guid = AssetDatabase.AssetPathToGUID(sceneRelativeToProjectPath);
        
        
        // Set the bundles' naming style to custom, and make the name as unique as possible to avoid cross-mod conflicts.
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleNaming = ShaderBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleCustomNaming = currentSceneName + "Shaders" + DateTime.Now.Minute + DateTime.Now.Second;
        
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleCustomNaming = currentSceneName + "Mono" + DateTime.Now.Minute + DateTime.Now.Second;


        if (group == null || guid == null)
            return;

        foreach (AddressableAssetEntry entry in group.entries.ToList())
            group.RemoveAssetEntry(entry);

        var e = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, false);
        var entriesAdded = new List<AddressableAssetEntry> { e };
        e.SetLabel("Map", true, true, false);

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
        AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);

        string windowsbuildPath = "C:/Users/Public/mod.io/4747/mods/{LOCAL_FILE_NAME}/";
        //string editorPath = "{UnityEngine.Application.persistentDataPath}/mod.io/04747/data/mods/{LOCAL_FILE_NAME}/";

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
    
    
    private void AddLineAndSpace()
    {
        GUILayout.Space(10);
        DrawUILine(Color.blue);
        GUILayout.Space(10);
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