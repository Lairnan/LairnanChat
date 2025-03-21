using System.Reflection;

namespace ChatClient.WPF.Behaviors;

public static partial class Extension
{
    public static T Await<T>(this Task<T> task, Action<T> action)
    {
        task.ContinueWith(t => action(t.Result));
        if (!task.IsCompleted) task.ConfigureAwait(false).GetAwaiter().GetResult();
        return task.Result;
    }
    
    public static bool EqualsByProperties<T>(this T obj1, T obj2)
    {
        if (obj1 == null && obj2 == null) return true;
        if (obj1 == null || obj2 == null) return false;
        
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return properties.All(property => property.GetValue(obj1) == property.GetValue(obj2));
    }
}