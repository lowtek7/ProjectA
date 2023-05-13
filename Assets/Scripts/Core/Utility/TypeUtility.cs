using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assembly = System.Reflection.Assembly;

namespace Core.Utility
{
	public class TypeUtility
	{
		/// <summary>
		/// TypeCache.GetTypesWithAttribute()을 사용하거나 이것을 사용하거나 둘 중 하나
		/// 다만 TypeCache쪽은 UnityEditor에 속하기 때문에 런타임에서는 이것을 사용해야 한다.
		/// </summary>
		/// <param name="attributeType"></param>
		/// <returns></returns>
		public static Type[] GetTypesWithAttribute(Type attributeType)
		{
			var playerAssemblies = GetUserCreatedAssemblies(AppDomain.CurrentDomain);
			List<Type> results = new List<Type>();

			foreach (var playerAssembly in playerAssemblies)
			{
				var assembly = Assembly.Load(playerAssembly.FullName);
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					var result = type.GetCustomAttribute(attributeType);
					if (result != null)
					{
						results.Add(type);
					}
				}
			}

			return results.ToArray();
		}
		
		public static Type[] GetTypesWithInterface(Type interfaceType)
		{
			var playerAssemblies = GetUserCreatedAssemblies(AppDomain.CurrentDomain);
			List<Type> results = new List<Type>();

			foreach (var playerAssembly in playerAssemblies)
			{
				var assembly = Assembly.Load(playerAssembly.FullName);
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (interfaceType == type) continue;

					var interfaces = type.GetInterfaces();
					var resultIndex = Array.FindIndex(interfaces, t => t == interfaceType);
					if (resultIndex >= 0)
					{
						results.Add(type);
					}
				}
			}

			return results.ToArray();
		}

		public static Type[] GetTypesByFullNames(IEnumerable<string> typeNames)
		{
			var playerAssemblies = GetUserCreatedAssemblies(AppDomain.CurrentDomain);
			List<Type> results = new List<Type>();
			var typeNameList = typeNames.ToList();

			foreach (var playerAssembly in playerAssemblies)
			{
				var assembly = Assembly.Load(playerAssembly.FullName);
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (typeNameList.Count == 0)
					{
						break;
					}
					
					var index = typeNameList.FindIndex(x => x == type.FullName);

					if (index >= 0)
					{
						results.Add(type);
						typeNameList.RemoveAt(index);
					}
				}
			}

			return results.ToArray();
		}

		public static IEnumerable<Assembly> GetUserCreatedAssemblies(AppDomain appDomain)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.IsDynamic)
				{
					continue;
				}

				string assemblyName = assembly.GetName().Name;
				if (_internalAssemblyNames.Contains(assemblyName))
				{
					continue;
				}

				yield return assembly;
			}
		}

		private static readonly HashSet<string> _internalAssemblyNames = new HashSet<string>()
		{
			"mscorlib",
			"System",
			"System.Core",
			"System.Security.Cryptography.Algorithms",
			"System.Net.Http",
			"System.Data",
			"System.Runtime.Serialization",
			"System.Xml.Linq",
			"System.Numerics",
			"System.Xml",
			"System.Configuration",
			"ExCSS.Unity",
			"Unity.Cecil",
			"Unity.CompilationPipeline.Common",
			"Unity.SerializationLogic",
			"Unity.TestTools.CodeCoverage.Editor",
			"Unity.ScriptableBuildPipeline.Editor",
			"Unity.Addressables.Editor",
			"Unity.ScriptableBuildPipeline",
			"Unity.CollabProxy.Editor",
			"Unity.Timeline.Editor",
			"Unity.PerformanceTesting.Tests.Runtime",
			"Unity.Settings.Editor",
			"Unity.PerformanceTesting",
			"Unity.PerformanceTesting.Editor",
			"Unity.Rider.Editor",
			"Unity.ResourceManager",
			"Unity.TestTools.CodeCoverage.Editor.OpenCover.Mono.Reflection",
			"Unity.PerformanceTesting.Tests.Editor",
			"Unity.TextMeshPro",
			"Unity.Timeline",
			"Unity.Addressables",
			"Unity.TestTools.CodeCoverage.Editor.OpenCover.Model",
			"Unity.VisualStudio.Editor",
			"Unity.TextMeshPro.Editor",
			"Unity.VSCode.Editor",
			"UnityEditor",
			"UnityEditor.UI",
			"UnityEditor.TestRunner",
			"UnityEditor.CacheServer",
			"UnityEditor.WindowsStandalone.Extensions",
			"UnityEditor.Graphs",
			"UnityEditor.UnityConnectModule",
			"UnityEditor.UIServiceModule",
			"UnityEditor.UIElementsSamplesModule",
			"UnityEditor.UIElementsModule",
			"UnityEditor.SceneTemplateModule",
			"UnityEditor.PackageManagerUIModule",
			"UnityEditor.GraphViewModule",
			"UnityEditor.CoreModule",
			"UnityEngine",
			"UnityEngine.UI",
			"UnityEngine.XRModule",
			"UnityEngine.WindModule",
			"UnityEngine.VirtualTexturingModule",
			"UnityEngine.TestRunner",
			"UnityEngine.VideoModule",
			"UnityEngine.VehiclesModule",
			"UnityEngine.VRModule",
			"UnityEngine.VFXModule",
			"UnityEngine.UnityWebRequestWWWModule",
			"UnityEngine.UnityWebRequestTextureModule",
			"UnityEngine.UnityWebRequestAudioModule",
			"UnityEngine.UnityWebRequestAssetBundleModule",
			"UnityEngine.UnityWebRequestModule",
			"UnityEngine.UnityTestProtocolModule",
			"UnityEngine.UnityCurlModule",
			"UnityEngine.UnityConnectModule",
			"UnityEngine.UnityAnalyticsModule",
			"UnityEngine.UmbraModule",
			"UnityEngine.UNETModule",
			"UnityEngine.UIElementsNativeModule",
			"UnityEngine.UIElementsModule",
			"UnityEngine.UIModule",
			"UnityEngine.TilemapModule",
			"UnityEngine.TextRenderingModule",
			"UnityEngine.TextCoreModule",
			"UnityEngine.TerrainPhysicsModule",
			"UnityEngine.TerrainModule",
			"UnityEngine.TLSModule",
			"UnityEngine.SubsystemsModule",
			"UnityEngine.SubstanceModule",
			"UnityEngine.StreamingModule",
			"UnityEngine.SpriteShapeModule",
			"UnityEngine.SpriteMaskModule",
			"UnityEngine.SharedInternalsModule",
			"UnityEngine.ScreenCaptureModule",
			"UnityEngine.RuntimeInitializeOnLoadManagerInitializerModule",
			"UnityEngine.ProfilerModule",
			"UnityEngine.Physics2DModule",
			"UnityEngine.PhysicsModule",
			"UnityEngine.PerformanceReportingModule",
			"UnityEngine.ParticleSystemModule",
			"UnityEngine.LocalizationModule",
			"UnityEngine.JSONSerializeModule",
			"UnityEngine.InputLegacyModule",
			"UnityEngine.InputModule",
			"UnityEngine.ImageConversionModule",
			"UnityEngine.IMGUIModule",
			"UnityEngine.HotReloadModule",
			"UnityEngine.GridModule",
			"UnityEngine.GameCenterModule",
			"UnityEngine.GIModule",
			"UnityEngine.DirectorModule",
			"UnityEngine.DSPGraphModule",
			"UnityEngine.CrashReportingModule",
			"UnityEngine.CoreModule",
			"UnityEngine.ClusterRendererModule",
			"UnityEngine.ClusterInputModule",
			"UnityEngine.ClothModule",
			"UnityEngine.AudioModule",
			"UnityEngine.AssetBundleModule",
			"UnityEngine.AnimationModule",
			"UnityEngine.AndroidJNIModule",
			"UnityEngine.AccessibilityModule",
			"UnityEngine.ARModule",
			"UnityEngine.AIModule",
			"SyntaxTree.VisualStudio.Unity.Bridge",
			"nunit.framework",
			"Newtonsoft.Json",
			"ReportGeneratorMerged",
			"Unrelated",
			"netstandard",
			"SyntaxTree.VisualStudio.Unity.Messaging"
		};
	}
}
