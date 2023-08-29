using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject playerStartPoint;
    public GameObject enemySpawnpointsHolder;
    
    public Transform avatarCalibratorTransform;
    public Transform levelSelectTransform;
    public Transform playerArmoryTransform;
    public Transform enemySpawnerTransform;
    
    public bool thisSceneUsesUmbraOcclusionCulling = true;
    
    public List<Light> sceneLights;
}
