using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Custom/ColorData", order = 1)]
public class ColorData : ScriptableObject
{
    public ColorName[] colorOption;

    [System.Serializable]
    public class ColorName
    {
        public string displayName;
        public Material humanMaterial;
        public Material carMaterial;
    }
}