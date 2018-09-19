﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLightReciever : MonoBehaviour {
    List<Color> lightColors = new List<Color>();
    Renderer r;

    void Start () {
        r = GetComponent<Renderer>();
    }

    void Update() {

        if (lightColors.Count == 0) {
            r.material.color = Color.black;
        } else {
            var averageColor = Color.black;

            for (var i = 0; i < lightColors.Count; i++) {
                averageColor += lightColors[i];
            }

            //averageColor /= lightColors.Count;

            r.material.color = averageColor;
        }

        lightColors.Clear();
    }

    public void Illuminate(Color color) {
        var newColor = new Color(
                Mathf.Clamp(color.r, 0, 1f),
                Mathf.Clamp(color.g, 0, 1f),
                Mathf.Clamp(color.b, 0, 1f),
                1
            );

        lightColors.Add(newColor);
    }
}