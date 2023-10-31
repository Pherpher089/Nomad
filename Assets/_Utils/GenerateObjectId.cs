using UnityEngine;

public static class GenerateObjectId
{
    public static string GenerateSourceObjectId(SourceObject so)
    {
        return $"{so.itemIndex}_{(int)so.transform.localPosition.x}_{(int)so.transform.localPosition.y}_{(int)so.transform.localPosition.z}_{(int)so.transform.localRotation.eulerAngles.y}_"; ;
    }
    public static string GenerateObjId(int itemIndex, Vector3Int position, int rotation, string state)
    {
        return $"{itemIndex}_{position.x}_{position.y}_{position.z}_{rotation}_{state}"; ;
    }
}
