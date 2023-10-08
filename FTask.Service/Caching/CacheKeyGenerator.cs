namespace FTask.Service.Caching
{
    public class CacheKeyGenerator
    {
        public static string GetKeyById(string objectType, string id)
        {
            return $"{objectType.ToLower()}:id:{id}";
        }

        public static string GetKeyByName(string objectType, string name)
        {
            return $"{objectType.ToLower()}:name:{name.ToLower()}";
        }

        public static string GetKeyByPageAndQuantity(string objectType, int pageNumber, int quantity)
        {
            return $"{objectType.ToLower()}:page:{pageNumber}:quantity:{quantity}";
        }

        public static string GetKeyByOtherId(string objectType, string otherName, string otherId)
        {
            return $"{objectType.ToLower()}:{otherName}:id:{otherId}";
        }

        public static string GetKeyByOtherName(string objectType, string otherName, string otherValue)
        {
            return $"{objectType.ToLower()}:{otherName}:name:{otherValue.ToLower()}";
        }
    }
}
