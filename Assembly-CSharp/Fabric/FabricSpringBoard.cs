using UnityEngine;

namespace Fabric
{
    [ExecuteInEditMode]
    public class FabricSpringBoard : MonoBehaviour
    {
        public string _fabricManagerPrefabPath;
        public static bool _isPresent;

        public FabricSpringBoard()
        {
            _isPresent = true;
        }

        private void OnEnable()
        {
            _isPresent = true;
        }

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            if (GetFabricManagerInEditor())
            {
                return;
            }

            GameObject prefab = Resources.Load(_fabricManagerPrefabPath, typeof(GameObject)) as GameObject;
            if (prefab)
            {
                Instantiate(prefab);
            }
        }

        public static FabricManager GetFabricManagerInEditor()
        {
            FabricManager[] array = Resources.FindObjectsOfTypeAll(typeof(FabricManager)) as FabricManager[];
            foreach (FabricManager manager in array)
            {
                if (manager.gameObject != null && manager.hideFlags != HideFlags.HideInHierarchy)
                {
                    return manager;
                }
            }

            return null;
        }
    }
}