using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static System.Net.WebRequestMethods;
using System;
using static UnityEditor.Progress;

namespace UpgradeSystem
{

    [Serializable]
    public class GameData
    {
      public string update_type;
      public string min_version;
      public string max_version;
      public string message;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }

    public class NewUpdatesPopupUI : MonoBehaviour {

        [Header ( "## UI References :" )]
        [SerializeField] GameObject uiOptionalCanvas;
        [SerializeField] GameObject uiMandatoryCanvas;
        [SerializeField] Button uiNotNowButton;
        [SerializeField] Button uiUpdateButton;
        [SerializeField] TextMeshProUGUI uiMessageText;

        [Space ( 20f )]
        [Header ( "## Settings :" )]
        [SerializeField] [TextArea ( 1, 5 )] string jsonDataURL;
        //[SerializeField] [TextArea(1, 5)] string appURL;

        static bool isAlreadyCheckedForUpdates = false;
        static string appURL = "https://play.google.com/store/apps/details?id=com.OritSciencesPrivateLimited.EnglishGurukul.studentapp";

        GameData[] latestGameData = new GameData[2];

        void Start ( )
        {
            if ( !isAlreadyCheckedForUpdates )
            {
                StopAllCoroutines ( );
                Coroutine coroutine = StartCoroutine ( CheckForUpdates ( ) );
            }
        }

        IEnumerator CheckForUpdates ( )
        {
             UnityWebRequest www = UnityWebRequest.Get ( jsonDataURL );
             www.disposeDownloadHandlerOnDispose = true;
             www.timeout = 60;

             yield return www.SendWebRequest( );

            string fixJson(string value)
            {
                value = "{\"Items\":" + value + "}";
                return value;
            }

            if ( www.isDone )
            {
                isAlreadyCheckedForUpdates = true;
                string jsonStringServer = www.downloadHandler.text;
                string jsonString = fixJson(jsonStringServer);

                if (!(www.result == UnityWebRequest.Result.ConnectionError))
                {

                    GameData[] latestGameData = JsonHelper.FromJson<GameData>(jsonString);

                    int cmp = string.Compare(Application.version, latestGameData[0].max_version);
                    Debug.Log(Application.version);

                    if (cmp < 0)
                    {
                        // new update is available
                        int cmp1 = string.Compare(Application.version, latestGameData[0].min_version);
                        if (cmp1<0)
                        {
                            uiMessageText.text = latestGameData[1].message;
                            ShowPopupMandatory();
                        }
                        else
                        {
                            uiMessageText.text = latestGameData[0].message;
                            ShowPopupOptional();
                        }
                    }
                }
            }

         www.Dispose ( );
      }

        void ShowPopupMandatory ( )
        {
            uiMandatoryCanvas.SetActive(true);

            // Add buttons click listeners :
            uiUpdateButton.onClick.AddListener ( ( ) => {
                Application.OpenURL (appURL);
            });
        }

        void ShowPopupOptional()
        {
            uiOptionalCanvas.SetActive(true);

            // Add buttons click listeners :
            uiNotNowButton.onClick.AddListener(() => {
                HidePopup();
            });

            uiUpdateButton.onClick.AddListener(() => {
                Application.OpenURL(appURL);
                HidePopup();
            });
        }

        void HidePopup ( )
        {
            uiOptionalCanvas.SetActive(false);
            uiMandatoryCanvas.SetActive(false);

            // Remove buttons click listeners :
            uiNotNowButton.onClick.RemoveAllListeners ( );
            uiUpdateButton.onClick.RemoveAllListeners ( );
        }

        void OnDestroy ( )
        {
            StopAllCoroutines ( );
        }
	   
   }
}
