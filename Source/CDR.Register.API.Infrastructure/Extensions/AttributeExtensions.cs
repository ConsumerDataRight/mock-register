using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CDR.Register.API.Infrastructure
{
    public static class AttributeExtensions
    {
        public static IEnumerable<object> GetAttributes(List<Type> types, MethodInfo info, bool inherit)
        {
            var actionAttributes = info.GetCustomAttributes(inherit);

            IEnumerable<Object> controllerAttributes = [];

            if (info.DeclaringType != null)
            {
                controllerAttributes = info.DeclaringType.GetTypeInfo().GetCustomAttributes(inherit);
            }
            var actionAndControllerAttributes = actionAttributes.Union(controllerAttributes);

            return actionAndControllerAttributes.Where(attr => types.Contains(attr.GetType()));
        }

        public static T? GetAttribute<T>(MethodInfo info, bool inherit)
        {
            var actionAttributes = info.GetCustomAttributes(inherit);

            IEnumerable<Object> controllerAttributes = [];

            if (info.DeclaringType != null)
            {
                controllerAttributes = info.DeclaringType.GetTypeInfo().GetCustomAttributes(inherit);
            }
            var actionAndControllerAttributes = actionAttributes.Union(controllerAttributes);

            return (T?)actionAndControllerAttributes.SingleOrDefault(attr => attr.GetType() == typeof(T));
        }

        public static bool HasAttribute(MethodInfo info, Type type, bool inherit)
        {
            var actionAttributes = info.GetCustomAttributes(inherit);

            IEnumerable<Object> controllerAttributes = [];

            if (info.DeclaringType != null)
            {
                controllerAttributes = info.DeclaringType.GetTypeInfo().GetCustomAttributes(inherit);
            }
            var actionAndControllerAttributes = actionAttributes.Union(controllerAttributes);

            return actionAndControllerAttributes.Any(attr => attr.GetType() == type);
        }
    }
}
