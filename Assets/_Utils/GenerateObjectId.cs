using UnityEngine;

public static class GenerateObjectId
{
    public static string GenerateSourceObjectId(SourceObject so)
    {
        return $"{so.itemIndex}_" +
                $"{so.transform.position.x.ToString("F2")}_" +
                $"{so.transform.position.y.ToString("F2")}_" +
                $"{so.transform.position.z.ToString("F2")}_" +
                $"{so.transform.rotation.eulerAngles.y.ToString("F2")}_";
    }

    public static string GenerateItemId(Item item)
    {
        return $"{item.itemIndex}_" +
                $"{item.transform.position.x.ToString()}_" +
                $"{item.transform.position.y.ToString()}_" +
                $"{item.transform.position.z.ToString()}_" +
                $"{item.transform.rotation.eulerAngles.y.ToString()}_";
    }
}
