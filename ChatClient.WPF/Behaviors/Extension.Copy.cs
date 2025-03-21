using System.Reflection;

namespace ChatClient.WPF.Behaviors;

public static partial class Extension
{
    /// <summary>
    /// Copies the properties from obj2 to obj1 of the specified type T. 
    /// </summary>
    /// <param name="obj1">The destination object to copy properties to.</param>
    /// <param name="obj2">The source object to copy properties from.</param>
    /// <typeparam name="T">The type of the objects involved.</typeparam>
    /// <returns>The destination object with properties copied from the source object.</returns>
    public static T CopyToObject<T>(this T obj1, T obj2) where T : notnull
    {
        // Get the type of T
        var type = typeof(T);

        // Get all public instance properties of the type
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Copy property values from obj2 to obj1
        foreach (var property in properties)
            property.SetValue(obj2, property.GetValue(obj1));

        return obj2;
    }
    
    public static T CopyLocal<T>(this T obj) where T : notnull 
    {
        var type = typeof(T);
        return type.IsInterface ? CopyInterface(type, obj) : CopyClass(type, obj);
    }

    private static T CopyClass<T>(Type type, T obj) where T : notnull
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        if (constructor == null) throw new Exception("No constructor found");
        var parameters = constructor.GetParameters();
        T local;
        
        if (parameters.Length == 0)
            local = (T)constructor.Invoke(null);
        else
        {
            var args = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var property = properties.FirstOrDefault(p => p.Name == parameters[i].Name);
                if (property != null)
                {
                    args[i] = property.GetValue(obj);
                    properties.Remove(property);
                }
                else
                    args[i] = parameters[i].DefaultValue;
            }

            local = (T)constructor.Invoke(args);
        }
        
        foreach (var property in properties)
            if (property.CanWrite) property.SetValue(local, property.GetValue(obj));
        
        return local;
    }

    private static T CopyInterface<T>(IReflect type, T obj) where T : notnull
    {
        var local = IoC.Resolve<T>();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
            property.SetValue(local, property.GetValue(obj));
            
        return local;
    }
}