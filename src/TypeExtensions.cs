using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if an instance of <paramref name="givenType"/> can be assigned to a type <paramref name="genericType"/>.
        /// </summary>
        /// <param name="givenType">The type under test</param>
        /// <param name="genericType">The targeted type</param>
        /// <returns><c>true</c> if <paramref name="genericType"/> is an ancestor of <paramref name="givenType"/> and <c>false</c> otherwise.</returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
            => givenType is not null && genericType is not null
               && (givenType == genericType || givenType.MapsToGenericTypeDefinition(genericType)
                   || givenType.HasInterfaceThatMapsToGenericTypeDefinition(genericType)
                   || givenType.GetTypeInfo().BaseType.IsAssignableToGenericType(genericType));

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this Type givenType, Type genericType)
        => givenType is not null && genericType is not null && givenType
                .GetTypeInfo()
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3

                .ImplementedInterfaces
#else
            .GetInterfaces()
#endif
            .Where(it => it.GetTypeInfo().IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == genericType);

        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
            => givenType is not null
               && genericType?.GetTypeInfo().IsGenericTypeDefinition == true
               && givenType.GetTypeInfo().IsGenericType
               && givenType.GetGenericTypeDefinition() == genericType;

        /// <summary>
        /// Tests if <paramref name="type"/> is an anonymous type
        /// </summary>
        /// <param name="type">The type under test</param>
        /// <returns><c>true</c>if <paramref name="type"/> is an anonymous type and <c>false</c> otherwise</returns>
        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type?.GetTypeInfo()?.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false)?.Any() ?? false;
            bool nameContainsAnonymousType = type?.FullName.Contains("AnonymousType") ?? false;

            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }
    }
}