using UnityEngine;

public static class GenerateObjectId
{
    public static string GenerateSourceObjectId(SourceObject so)
    {
        return $"{so.environmentListIndex}_" +
                $"{so.transform.position.x}_" +
                $"{so.transform.position.y}_" +
                $"{so.transform.position.z}_" +
                $"{so.transform.rotation.eulerAngles.y:F2}_";
    }

    public static string GenerateItemId(Item item)
    {
        return $"{item.itemListIndex}_" +
                $"{item.transform.position.x}_" +
                $"{item.transform.position.y}_" +
                $"{item.transform.position.z}_" +
                $"{item.transform.rotation.eulerAngles.y}_";
    }
}
