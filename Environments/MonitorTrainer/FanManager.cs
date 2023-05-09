using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace MonitorTrainer
{
    public class FanManager : MonoBehaviour
    {
        private const string COMIC_FAN = "41106a27-a3e8-4cdc-888b-44ada635c9a9";

        public void Initialise()
        {
            Scene sc = this.gameObject.scene;
            var list = sc.GetRootGameObjects().ToList();

            GameObject modelsRoot = list.FindLast(e => e.name == "Models"); 
            if (null != modelsRoot)
            {
                //////GameObject fan = modelsRoot.transform.Search("Misc_OfficeFan-Base_LOD0").gameObject;

                //////Core.Scene.SpawnObjectReplace(fan, COMIC_FAN, (replaced) =>
                //////{
                //////    Debug.LogError("replaced1");
                //////});


                //////GameObject secondFan = modelsRoot.transform.Search("Misc_OfficeFan-Base_LOD0 (1)").gameObject;
                //////Core.Scene.SpawnObjectReplace(secondFan, COMIC_FAN, (replaced) =>
                //////{
                //////    Debug.LogError("replaced2");
                //////});

            }
            else
            {
                Debug.LogError("Cannot find Models");
            }

        }

    }
}
