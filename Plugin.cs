using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements.Experimental;

namespace RealisticBlackHoles
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony harmony = new Harmony("dev.talkingpanda.realistichole");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        public static string ChangeOutfitColor(Outfit[] outfits)
        {
            for (int i = 0; i < outfits.Length; i++)
            {
                Outfit outfit = outfits[i];
                if (outfit.outfitType == Outfit.OutfitType.Skin)
                    return outfits.ToString();
            }
            return "";
        }
    }



    [HarmonyPatch(typeof(Blackhole))]
    [HarmonyPatch(nameof(Blackhole.suckInCharacter))]
    class Patch01
    {
        static AccessTools.FieldRef<Blackhole, List<Character>> currentlySuckingRef = AccessTools.FieldRefAccess<Blackhole, List<Character>>("currentlySucking");


        // Disable the original function
        static bool Prefix()
        {
            return false;
        }
        static IEnumerator Postfix(IEnumerator values, Blackhole __instance, Character chr)
        {
            if (!chr.InBlackHole && !chr.Success)
            {
                const float redShiftTime = 15;
                const float shrinkTime = 1;
                float timer = 0.01f;
                Vector3 initalPositon = chr.transform.position;
                Vector3 initalLocalPositon = chr.transform.localPosition;
                Color initialColor = chr.SpriteRenderer.material.color;
                Vector3 initialLocalScale = chr.transform.localScale;


                chr.InBlackHole = true;
                Vector3 initialScale = chr.transform.localScale;
                Vector3 targetScale = new Vector3(0.999f, 0.999f, 0.999f);
                GameObject tempTrackingObject = new GameObject("Character Suck In Target");
                tempTrackingObject.transform.SetParent(__instance.transform);
                tempTrackingObject.transform.position = chr.transform.position;
                tempTrackingObject.transform.rotation = chr.transform.rotation;
                Vector3 initialPosition = tempTrackingObject.transform.localPosition;
                Quaternion initialRotation = chr.transform.rotation;
                Color redShiftedColor = initialColor;
                Color targetColor = new(r: 0.7f, g: 0, b: 0, a: 0);

                do
                {

                    timer += Time.deltaTime;
                    chr.transform.position = initalPositon;
                    if (timer < shrinkTime)
                    {
                        Vector3 localScale = Vector3.Lerp(initialScale, targetScale, timer / shrinkTime);
                        chr.transform.localScale = localScale;
                        chr.transform.Rotate(new Vector3(0f, 0f, 5 * Time.deltaTime));

                    }
                    else
                    {
                        redShiftedColor = Color.LerpUnclamped(initialColor, targetColor, timer / redShiftTime);
                        chr.SpriteRenderer.material.color = redShiftedColor;
                    }


                    yield return null;
                } while (timer < redShiftTime && (Object)(object)chr != null && chr.Dying && __instance.Active);
                Object.Destroy(tempTrackingObject);
                while (__instance.Active && chr != null && chr.Dying && chr.InBlackHole)
                {
                    chr.transform.position = initalPositon;
                    yield return null;
                }
                if ((Object)(object)chr != null)
                {
                    chr.RefreshScale();
                    chr.SpriteRenderer.material.color = initialColor;
                    chr.transform.rotation = initialRotation;
                    chr.InBlackHole = false;
                    currentlySuckingRef(__instance).Remove(chr);
                }
            }

        }
    }
}