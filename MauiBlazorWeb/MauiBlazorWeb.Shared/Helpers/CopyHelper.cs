using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MauiBlazorWeb.Shared.Helpers
{
	/// <summary>
	/// Helper class for copying objects and resetting IDs.
	/// </summary>
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

		/// <summary>
		/// Recursively sets the Id property to default for all objects implementing IEntityWithId and their associations.
		/// </summary>
		public static void SetIdsToDefault(object obj)
		{
			var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
			SetIdsToDefaultInternal(obj, visited);
		}

		private static void SetIdsToDefaultInternal(object obj, HashSet<object> visited)
		{
			if (obj == null) return;

			// Prevent infinite loops from circular references
			if (!visited.Add(obj)) return;

			var type = obj.GetType();

			// Skip primitives and strings
			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
			    type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(TimeOnly))
				return;

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

				// Skip already processed types
				if (value is string || value.GetType().IsPrimitive)
					continue;

				// If it's a collection, recurse for each item
				if (value is IEnumerable enumerable && !(value is IDictionary))
				{
					foreach (var item in enumerable)
					{
						if (item != null)
							SetIdsToDefaultInternal(item, visited);
					}
				}
				else if (!value.GetType().IsValueType)
				{
					// Recurse for nested objects (only reference types)
					SetIdsToDefaultInternal(value, visited);
				}
			}
		}

		private class ReferenceEqualityComparer : IEqualityComparer<object>
		{
			public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

			public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
			public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
		}
	}
}
