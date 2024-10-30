using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

/*
 * 이 클래스는 유니티의 씬이 로드되기 전 MessagePack의 Resolver 목록을 초기화하기 위해 사용됩니다.
 */
public class Startup
{
    static bool serializerRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!serializerRegistered)
        {
            StaticCompositeResolver.Instance.Register(
                 MessagePack.Resolvers.GeneratedResolver.Instance,
                 MessagePack.Resolvers.StandardResolver.Instance
            );

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;
            serializerRegistered = true;
        }
    }

#if UNITY_EDITOR


    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInitialize()
    {
        Initialize();
    }

#endif
}