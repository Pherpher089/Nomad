using UnityEngine;

public static class GenerateObjectId
{
    public static string GenerateSourceObjectId(SourceObject so)
    {
        return $"{so.itemIndex}_" +
                $"{so.transform.localPosition.x.ToString("F2")}_" +
                $"{so.transform.localPosition.y.ToString("F2")}_" +
                $"{so.transform.localPosition.z.ToString("F2")}_" +
                $"{so.transform.localRotation.eulerAngles.y.ToString("F2")}_";
    }

    public static string GenerateItemId(Item item)
    {
        return $"{item.itemIndex}_" +
                $"{item.transform.localPosition.x.ToString("F2")}_" +
                $"{item.transform.localPosition.y.ToString("F2")}_" +
                $"{item.transform.localPosition.z.ToString("F2")}_" +
                $"{item.transform.localRotation.eulerAngles.y.ToString("F2")}_";
    }
}
