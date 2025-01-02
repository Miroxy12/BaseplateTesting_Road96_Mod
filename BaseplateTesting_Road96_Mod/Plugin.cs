using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace BaseplateTesting
{
    [BepInEx.BepInPlugin(mod_guid, "BaseplateTesting", version)]
    [BepInEx.BepInProcess("Road 96.exe")]
    public class BaseplateTestingMod : BasePlugin
    {
        private const string mod_guid = "miroxy12.baseplatetesting";
        private const string version = "1.0";
        private readonly Harmony harmony = new Harmony(mod_guid);
        internal static new ManualLogSource Log;
        public static bool madebtn = false;
        public static GameObject baseplatetesting_int;
        public static GameObject playerobj;
        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo(mod_guid + " started, version: " + version);
            harmony.PatchAll(typeof(OnInteractionDoneHook));
            AddComponent<ModMain>();
        }
    }
    public class ModMain : MonoBehaviour
    {
        void Awake()
        {
            BaseplateTestingMod.Log.LogInfo("loading BaseplateTesting");
        }
        void OnEnable()
        {
            BaseplateTestingMod.Log.LogInfo("enabled BaseplateTesting");
        }
        void Update()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("Profiler") && !BaseplateTestingMod.madebtn) {
                GameObject options_int = GameObject.Find("INT_Options");
                if (options_int != null) {
                    BaseplateTestingMod.baseplatetesting_int = GameObject.Instantiate(options_int);
                    BaseplateTestingMod.baseplatetesting_int.name = "INT_BaseplateTesting";
                    BaseplateTestingMod.baseplatetesting_int.GetComponent<BlueEyes.Interactions.Interaction>()._content = "Spawn on the epic baseplate";
                }
                BaseplateTestingMod.madebtn = true;
            }
            if (UnityEngine.SceneManagement.SceneManager.GetSceneByName("Graph_LOC_GEN_DINNER_ForestDay").isLoaded) {
                GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Graph_LOC_GEN_DINNER_ForestDay").GetRootGameObjects();
                foreach (var go in gos) {
                    if (go.name.Equals("GRP_Volumes") || go.name.Equals("GRP_Lighting")) {
                        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, UnityEngine.SceneManagement.SceneManager.GetSceneByName("Loc_Gen_Dinner_Logic"));
                    } else {
                        go.SetActive(false); // this is better than unloading the scene for trash pc
                    }
                }
            }
           // UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Graph_LOC_GEN_DINNER_ForestDay");
            if (UnityEngine.SceneManagement.SceneManager.GetSceneByName("Loc_Gen_Dinner_Logic").isLoaded && BaseplateTestingMod.playerobj == null) {
                GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Loc_Gen_Dinner_Logic").GetRootGameObjects();
                foreach (var go in gos) {
                    if (go.name.Equals("01_MainLogic")) {
                        for (int i = 0; i < go.transform.childCount; i++) {
                            if (go.transform.GetChild(i).gameObject.name.Equals("Player")) {
                                BaseplateTestingMod.playerobj = go.transform.GetChild(i).gameObject;
                                ClassInjector.RegisterTypeInIl2Cpp<SetupBaseplate>();
                                GameObject baseplateobj = new GameObject("Baseplate");
                                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(baseplateobj, UnityEngine.SceneManagement.SceneManager.GetSceneByName("Loc_Gen_Dinner_Logic"));
                                baseplateobj.AddComponent<SetupBaseplate>();
                                break;
                            }
                        }
                    } else if (!go.name.Equals("GRP_Volumes") || !go.name.Equals("GRP_Lighting") || !go.name.Equals("Baseplate")) {
                        go.SetActive(false);
                    }
                }
            }
        }
    }
    public class SetupBaseplate : MonoBehaviour
    {
        void Awake()
        {
            BaseplateTestingMod.Log.LogInfo("Spawning on the baseplate..");
        }
        void OnEnable()
        {
            if (BaseplateTestingMod.playerobj == null) { BaseplateTestingMod.Log.LogError("No player object found!"); return; }
            BaseplateTestingMod.Log.LogInfo("spawned!");
        }
        void Start()
        {
            MeshFilter meshfilter = gameObject.AddComponent<MeshFilter>();
            BoxCollider boxcollider = gameObject.AddComponent<BoxCollider>();
            meshfilter.mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
            MeshRenderer meshrenderer = gameObject.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("HDRP/Lit"));

            material.color = Color.green;
            material.enableInstancing = true;
            meshrenderer.material = material;
            gameObject.transform.localScale = new Vector3(200f, 0.2f, 200f);
            gameObject.transform.position = new Vector3(0, -5, 0);
            boxcollider.size = new Vector3(200f, 0.2f, 200f);
            boxcollider.center = new Vector3(0, 0, 0);
        }
    }
    [HarmonyPatch(typeof(BlueEyes.Interactions.Interaction), "OnInteractionDone")]
    public class OnInteractionDoneHook
    {
        static void Prefix(BlueEyes.Interactions.Interaction __instance)
        {
            if (__instance.name.Equals("INT_BaseplateTesting")) {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Profiler");
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Loc_Gen_Dinner_Logic", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Graph_LOC_GEN_DINNER_ForestDay", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
    }
}
