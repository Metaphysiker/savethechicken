using System.Collections;
	/// <summary>
	/// Recursively sets the Id property to default for all objects implementing IEntityWithId and their associations.
	/// </summary>

using System.IO;
using System.Text.Json;

namespace MauiBlazorWeb.Shared.Helpers
{
	public class CopyHelper
	{
		/// <summary>
		/// Performs a deep copy of an object, including its associations, using JSON serialization.
		/// The object and all its associations must be serializable by System.Text.Json.
		/// </summary>
		public static T? DeepCopy<T>(T? obj)
		{
			if (obj == null)
			return default;

			var json = JsonSerializer.Serialize(obj);
			return JsonSerializer.Deserialize<T>(json);
		}

        	public static void SetIdsToDefault(object obj)
	{
		if (obj == null) return;

		var type = obj.GetType();

		// Check if the object implements IEntityWithId
		var implementsIEntityWithId = type.GetInterfaces().Any(i => i.Name == "IEntityWithId");
		if (implementsIEntityWithId)
		{
			var idProp = type.GetProperty("Id");
			if (idProp != null && idProp.CanWrite)
			{
				var idType = idProp.PropertyType;
				if (idType == typeof(int))
					idProp.SetValue(obj, 0);
				else if (idType == typeof(long))
					idProp.SetValue(obj, 0L);
				else if (idType == typeof(Guid))
					idProp.SetValue(obj, Guid.Empty);
				else if (idType.IsValueType)
					idProp.SetValue(obj, Activator.CreateInstance(idType));
				else
					idProp.SetValue(obj, null);
			}
		}

		// Go through all properties
		foreach (var prop in type.GetProperties())
		{
			if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
				continue;

			var value = prop.GetValue(obj);
			if (value == null)
				continue;

			// Skip string
			if (value is string)
				continue;

			// If it's a collection, recurse for each item
			if (value is IEnumerable enumerable && !(value is IDictionary))
			{
				foreach (var item in enumerable)
				{
					SetIdsToDefault(item);
				}
			}
			else
			{
				// Recurse for nested objects
				SetIdsToDefault(value);
			}
		}
	}
	}
}
