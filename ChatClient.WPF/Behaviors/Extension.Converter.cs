using System.Collections.ObjectModel;

namespace ChatClient.WPF.Behaviors;

public static partial class Extension
{
    public static string ConvertDay(this int value)
    {
        var valStr = value.ToString();
        string str;
        var newVal = value;
        if (valStr.Length > 1) newVal = int.Parse(valStr[valStr.Length - 1].ToString());

        if (value == 14) str = $"{value} дней";
        else if (newVal == 1) str = $"{value} день";
        else if (newVal > 0 && newVal < 5) str = $"{value} дня";
        else str = $"{value} дней";
            
        return str;
    }
        
    public static string ConvertFileSizeToString(this long fileSizeInBytes)
    {
        if (fileSizeInBytes < 0) return "Invalid size";

        string[] sizes = { "Байт", "КБ", "МБ", "ГБ", "ТБ", "ПБ" };
        var order = 0;

        while (fileSizeInBytes >= 1024 && order < sizes.Length - 1)
        {
            fileSizeInBytes /= 1024;
            order++;
        }

        return $"{(fileSizeInBytes == 0 ? "0" : fileSizeInBytes.ToString("#.#"))} {sizes[order]}";
    }
        
    public static IEnumerable<string> SplitStringToObservableCollection(this string text)
    {
        var items = new ObservableCollection<string>();
        if (string.IsNullOrEmpty(text)) return items;
        var arr = text.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in arr) items.Add(item.Trim());
            
        return items;
    }
}