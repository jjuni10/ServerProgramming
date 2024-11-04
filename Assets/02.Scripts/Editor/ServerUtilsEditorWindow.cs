#if UNITY_EDITOR
using Codice.Client.Common;
using MessagePack.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ServerUtilsEditorWindow : EditorWindow
{
    static ServerUtilsEditorWindow window;

    static string generatedMessagePackCodePath = "Assets/02.Scripts/Generated/Generated.cs";
    static string packetCodePath = "Assets/02.Scripts/Network/Packet.cs";
    static string startupCodePath = "Assets/02.Scripts/Startup.cs";

    static string startupCode = @"
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
}";

    static StringBuilder builder = new StringBuilder();
    static string isGeneratingKey = "ServerUtils_Generating";

    [MenuItem("Window/Server Utils")]
    static void Init()
    {
        if (window != null)
        {
            window.Close();
        }

        GetWindow<ServerUtilsEditorWindow>("Server Utils").Show();
    }


    [InitializeOnLoadMethod]
    static async void OnProjectLoadedInEditor()
    {
        if (EditorPrefs.GetBool(isGeneratingKey, false) == true)
        {
            EditorPrefs.SetBool(isGeneratingKey, false);
            await GeneratePacketSourceAsync();
        }
    }

    void OnEnable()
    {
        window = this;
    }

    async void OnGUI()
    {
        GUILayout.Label("Packet Code Generation", EditorStyles.boldLabel);

        bool isGenerating = EditorPrefs.GetBool(isGeneratingKey, false);
        EditorGUI.BeginDisabledGroup(isGenerating);
        if (GUILayout.Button("Generate"))
        {
            if (File.Exists(generatedMessagePackCodePath))
            {
                AssetDatabase.DeleteAsset(generatedMessagePackCodePath);
                AssetDatabase.DeleteAsset(startupCodePath);

                EditorPrefs.SetBool(isGeneratingKey, true);
                EditorUtility.RequestScriptReload();
            }
            else
            {
                await GeneratePacketSourceAsync();
                EditorPrefs.SetBool(isGeneratingKey, false);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    static async Task GeneratePacketSourceAsync()
    {

        /*
         * 'Assembly-CSharp' 어셈블리에서 Packet을 상속받는 모든 자식 클래스를 가져와 Packet.cs 작성
         */
        Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "Assembly-CSharp");

        var basePacketType = gameAssembly.GetTypes().SingleOrDefault(type => type.Name == "Packet");
        if (basePacketType == null)
        {
            Debug.LogError("Cannot find type 'Packet'!");
            return;
        }

        int counter = 0;
        var packetTypes = gameAssembly.GetTypes().Where(type => type.IsSubclassOf(basePacketType));

        builder.Clear();
        builder.AppendLine(@"using MessagePack;
");

        foreach (var packetType in packetTypes)
        {
            builder.AppendFormat("[Union({0}, typeof({1}))]", counter, packetType.Name);
            builder.AppendLine();
            counter++;
        }

        builder.AppendLine(@"
[MessagePackObject]
public abstract partial class Packet
{
}");

        string packetCode = builder.ToString();
        Debug.LogFormat("[Server Utils] Writing Packet.cs to \"{0}\"", packetCodePath);
        File.WriteAllText(packetCodePath, packetCode);


        /*
        * MessagePack의 Code Generator 실행
        */
        MpcArgument mpcArgument = new MpcArgument();
        mpcArgument.Input = "../Assembly-CSharp.csproj";
        mpcArgument.Output = "../" + generatedMessagePackCodePath;

        Debug.Log("[Server Utils] Running code generation..");
        try
        {
            var log = await ProcessHelper.InvokeProcessStartAsync("mpc", mpcArgument.ToString());
            Debug.Log(log);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        finally
        {
            File.WriteAllText(startupCodePath, startupCode);
            AssetDatabase.Refresh();
            Debug.Log("[Server Utils] Packet.cs generation complete.");
        }
    }
}
#endif