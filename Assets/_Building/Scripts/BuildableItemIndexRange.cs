using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuildableItemIndexRange", menuName = "BuildableItemIndexRange")]
public class BuildableItemIndexRange : ScriptableObject
{
    public BuildingMaterial buildingMaterial;
    public Vector2 buildableItemIndexRange;
}
