using System;
using UnityEngine;
namespace NoSuchStudio.Common {
    public static class HSVColor {
        public static Color hue2rgb(float hue) {
            hue = hue - (int)hue; 
            float r = Mathf.Abs(hue * 6 - 3) - 1; 
            float g = 2 - Mathf.Abs(hue * 6 - 2); 
            float b = 2 - Mathf.Abs(hue * 6 - 4); 
            Color rgb = new Color(r, g, b); 
            rgb.r = Mathf.Clamp01(rgb.r);
            rgb.g = Mathf.Clamp01(rgb.g);
            rgb.b = Mathf.Clamp01(rgb.b);
            
            return rgb;
        }

        public static Color hsv2rgb(Vector3 hsv) {
            Color rgb = hue2rgb(hsv.x); 
            Vector3 rgbVec = new Vector3(rgb.r, rgb.g, rgb.b);
            rgbVec = Vector3.Lerp(Vector3.one, rgbVec, hsv.y); 
            rgbVec = rgbVec * hsv.z; 
            rgb.r = rgbVec.x;
            rgb.g = rgbVec.y;
            rgb.b = rgbVec.z;
            return rgb;
        }

        public static Vector3 rgb2hsv(Color rgb) {
            float maxComponent = Mathf.Max(rgb.r, Mathf.Max(rgb.g, rgb.b));
            float minComponent = Mathf.Min(rgb.r, Mathf.Min(rgb.g, rgb.b));
            float diff = maxComponent - minComponent;
            float hue = 0;
            if (maxComponent == rgb.r) {
                hue = 0 + (rgb.g - rgb.b) / diff;
            } else if (maxComponent == rgb.g) {
                hue = 2 + (rgb.b - rgb.r) / diff;
            } else if (maxComponent == rgb.b) {
                hue = 4 + (rgb.r - rgb.g) / diff;
            }
            hue = (hue / 6) - (int)(hue / 6);
            if (hue < 0) hue = hue + 1;
            float saturation = diff / maxComponent;
            float value = maxComponent;
            
            return new Vector3(hue, saturation, value);
        }
    }
}