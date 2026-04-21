using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống Service Locator giúp gỡ bỏ Dependency Hell (God Object).
/// Thay vì GameManager giữ reference của tất cả mọi thứ, các Manager sẽ tự đăng ký vào đây.
/// Bất cứ Script nào cần dùng chỉ việc gọi CoreServices.Get<T>()
/// </summary>
public static class CoreServices
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (!services.ContainsKey(type))
        {
            services.Add(type, service);
        }
        else
        {
            services[type] = service; // Ghi đè nếu có phiên bản mới (ví dụ khi load lại scene)
        }
    }

    public static void Unregister<T>()
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            services.Remove(type);
        }
    }

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            return (T)services[type];
        }

        Debug.LogError($"[CoreServices] Không tìm thấy Service: {type.Name}. Hãy chắc chắn rằng nó đã gọi CoreServices.Register() trong Awake().");
        return default;
    }
}
