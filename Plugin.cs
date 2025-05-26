using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

[BepInPlugin("com.Mxyfell.GuiliamNormalFace", "GuiliamAliveForever", "1.0.0")]
public class ChangeFaceModifier : BaseUnityPlugin
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Logger.LogInfo("ChangeFaceModifier plugin loaded");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ModifyObjects());
    }

    private IEnumerator ModifyObjects()
    {
        yield return null;
        
        // Обработка всех трех объектов
        ProcessPixelMuertoObject("Barra_De_Experiencia", "ChangeFace", disableImage: true);
        ProcessPixelMuertoObject("FirstPersonCharacter", "OnlineCube", enableColorOnline: true);
        ProcessPixelMuertoObject("ModelAnchor", "EspiralMenu", forceUpdate: true);
    }

    private void ProcessPixelMuertoObject(string parentName, string childName, 
                                        bool disableImage = false, 
                                        bool enableColorOnline = false,
                                        bool forceUpdate = false)
    {
        GameObject targetObject = FindDeepChild(parentName, childName);
        if (targetObject == null)
        {
            Logger.LogInfo($"{childName} not found under {parentName}");
            return;
        }

        Logger.LogInfo($"Processing {childName} object");

        // Дополнительные действия для конкретных объектов
        if (disableImage)
        {
            Image image = targetObject.GetComponent<Image>();
            if (image != null) image.enabled = false;
        }

        if (enableColorOnline)
        {
            Transform colorOnline = targetObject.transform.Find("ColorOnline");
            if (colorOnline != null) colorOnline.gameObject.SetActive(true);
        }

        // Модификация PixelMuerto
        Component pixelMuerto = targetObject.GetComponent("PixelMuerto");
        if (pixelMuerto == null)
        {
            Logger.LogInfo($"PixelMuerto not found in {childName}");
            return;
        }

        // Изменение поля
        FieldInfo field = pixelMuerto.GetType().GetField("ponerCaraNormal", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(pixelMuerto, true);
            Logger.LogInfo($"Changed ponerCaraNormal in {childName}");

            // Принудительное обновление
            if (forceUpdate)
            {
                MonoBehaviour mb = pixelMuerto as MonoBehaviour;
                if (mb != null)
                {
                    mb.StopAllCoroutines();
                    mb.Invoke("UpdateVisuals", 0.1f);
                    Logger.LogInfo($"Forced visuals update in {childName}");
                }
            }
        }
    }

    private GameObject FindDeepChild(string parentName, string childName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == childName)
            {
                Transform parent = obj.transform.parent;
                while (parent != null)
                {
                    if (parent.name == parentName)
                        return obj;
                    parent = parent.parent;
                }
            }
        }
        return null;
    }
}