using UnityEngine;

public enum CargoRotation { Up = 0, Right = 1, Down = 2, Left = 3 }
public class CargoItem : MonoBehaviour
{
    public Vector2Int size;
    public CargoRotation rotation;
    public Sprite cargoIconUnpacked;
    public Sprite cargoIconPacked;
    public bool unpackable;

    public CargoItem(CargoItem item)
    {
        size = item.size;
        rotation = item.rotation;
        cargoIconUnpacked = item.cargoIconUnpacked;
        cargoIconPacked = item.cargoIconPacked;
        unpackable = item.unpackable;
    }
}