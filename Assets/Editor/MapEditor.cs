using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//keep the map in the updater updated
//copied from Sebastian Lague - https://www.youtube.com/watch?v=gIUVRYViG_g
[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();

        MapGenerator map = target as MapGenerator;

        map.GenerateMap ();
    }
	
}