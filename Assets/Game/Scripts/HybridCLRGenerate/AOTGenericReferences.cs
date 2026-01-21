using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"Framework.dll",
		"Google.Protobuf.dll",
		"Newtonsoft.Json.dll",
		"System.Core.dll",
		"UnityEngine.AndroidJNIModule.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.UI.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DG.Tweening.Core.DOGetter<UnityEngine.Color>
	// DG.Tweening.Core.DOGetter<UnityEngine.Quaternion>
	// DG.Tweening.Core.DOGetter<UnityEngine.Vector2>
	// DG.Tweening.Core.DOGetter<UnityEngine.Vector3>
	// DG.Tweening.Core.DOGetter<float>
	// DG.Tweening.Core.DOGetter<int>
	// DG.Tweening.Core.DOGetter<object>
	// DG.Tweening.Core.DOSetter<UnityEngine.Color>
	// DG.Tweening.Core.DOSetter<UnityEngine.Quaternion>
	// DG.Tweening.Core.DOSetter<UnityEngine.Vector2>
	// DG.Tweening.Core.DOSetter<UnityEngine.Vector3>
	// DG.Tweening.Core.DOSetter<float>
	// DG.Tweening.Core.DOSetter<int>
	// DG.Tweening.Core.DOSetter<object>
	// DG.Tweening.Core.TweenerCore<UnityEngine.Quaternion,UnityEngine.Vector3,DG.Tweening.Plugins.Options.QuaternionOptions>
	// DG.Tweening.Core.TweenerCore<UnityEngine.Vector3,object,DG.Tweening.Plugins.Options.PathOptions>
	// Google.Protobuf.Collections.RepeatedField<object>
	// Google.Protobuf.IDeepCloneable<object>
	// Google.Protobuf.IMessage<object>
	// Google.Protobuf.MessageParser<object>
	// Singleton<object>
	// System.Action<DG.Tweening.Plugins.Options.PathOptions,object,UnityEngine.Quaternion,object>
	// System.Action<Game.Flow.StartStep>
	// System.Action<byte,float,object>
	// System.Action<byte,object>
	// System.Action<float>
	// System.Action<int,object>
	// System.Action<int>
	// System.Action<object,object,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.Dictionary.Enumerator<object,System.ValueTuple<int,int,object>>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<Game.Flow.StartStep,object>
	// System.Collections.Generic.Dictionary<Game.Protocol.Option.Item.Type,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,LitJson.ArrayMetadata>
	// System.Collections.Generic.Dictionary<object,LitJson.ObjectMetadata>
	// System.Collections.Generic.Dictionary<object,LitJson.PropertyMetadata>
	// System.Collections.Generic.Dictionary<object,System.ValueTuple<int,int,object>>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<LitJson.PropertyMetadata>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IDictionary<object,LitJson.ArrayMetadata>
	// System.Collections.Generic.IDictionary<object,LitJson.ObjectMetadata>
	// System.Collections.Generic.IDictionary<object,LitJson.PropertyMetadata>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<LitJson.PropertyMetadata>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<LitJson.PropertyMetadata>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>
	// System.Collections.Generic.KeyValuePair<object,double>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<Framework.Loom.DelayedQueueItem>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Framework.Loom.DelayedQueueItem>
	// System.Collections.Generic.List<LitJson.PropertyMetadata>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,double>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<System.ValueTuple<System.ValueTuple<object,object,int,byte>,object,object>>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack<int>
	// System.Collections.Generic.Stack<object>
	// System.Func<Framework.Loom.DelayedQueueItem,byte>
	// System.Func<int,int,object>
	// System.Func<object,byte>
	// System.Func<object,int,int,int,object>
	// System.Func<object,int,object>
	// System.Func<object,int>
	// System.Func<object,object,object,object>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.IEquatable<object>
	// System.Nullable<System.ValueTuple<object,object,byte,byte>>
	// System.Nullable<UnityEngine.Color>
	// System.Nullable<UnityEngine.Vector3>
	// System.Nullable<float>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.CallSite<object>
	// System.ValueTuple<Game.UI.Tips,object>
	// System.ValueTuple<System.ValueTuple<object,object,int,byte>,object,object>
	// System.ValueTuple<float,float,float,float>
	// System.ValueTuple<int,int,object>
	// System.ValueTuple<object,object,byte,byte>
	// System.ValueTuple<object,object,int,byte>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityAction<object,object>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<UnityEngine.Vector2>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<float>
	// UnityEngine.Events.UnityEvent<object,object>
	// UnityEngine.Events.UnityEvent<object>
	// }}

	public void RefMethods()
	{
		// DG.Tweening.Core.TweenerCore<UnityEngine.Color,UnityEngine.Color,DG.Tweening.Plugins.Options.ColorOptions> DG.Tweening.Core.Extensions.Blendable<UnityEngine.Color,UnityEngine.Color,DG.Tweening.Plugins.Options.ColorOptions>(DG.Tweening.Core.TweenerCore<UnityEngine.Color,UnityEngine.Color,DG.Tweening.Plugins.Options.ColorOptions>)
		// object DG.Tweening.Core.Extensions.SetSpecialStartupMode<object>(object,DG.Tweening.Core.Enums.SpecialStartupMode)
		// DG.Tweening.Core.TweenerCore<UnityEngine.Vector2,UnityEngine.Vector2,DG.Tweening.Plugins.CircleOptions> DG.Tweening.DOTween.To<UnityEngine.Vector2,UnityEngine.Vector2,DG.Tweening.Plugins.CircleOptions>(DG.Tweening.Plugins.Core.ABSTweenPlugin<UnityEngine.Vector2,UnityEngine.Vector2,DG.Tweening.Plugins.CircleOptions>,DG.Tweening.Core.DOGetter<UnityEngine.Vector2>,DG.Tweening.Core.DOSetter<UnityEngine.Vector2>,UnityEngine.Vector2,float)
		// DG.Tweening.Core.TweenerCore<UnityEngine.Vector3,object,DG.Tweening.Plugins.Options.PathOptions> DG.Tweening.DOTween.To<UnityEngine.Vector3,object,DG.Tweening.Plugins.Options.PathOptions>(DG.Tweening.Plugins.Core.ABSTweenPlugin<UnityEngine.Vector3,object,DG.Tweening.Plugins.Options.PathOptions>,DG.Tweening.Core.DOGetter<UnityEngine.Vector3>,DG.Tweening.Core.DOSetter<UnityEngine.Vector3>,object,float)
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.OnStart<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.OnUpdate<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// object DG.Tweening.TweenSettingsExtensions.SetRelative<object>(object)
		// object DG.Tweening.TweenSettingsExtensions.SetTarget<object>(object,object)
		// object DG.Tweening.TweenSettingsExtensions.SetUpdate<object>(object,DG.Tweening.UpdateType)
		// Google.Protobuf.FieldCodec<object> Google.Protobuf.FieldCodec.ForMessage<object>(uint,Google.Protobuf.MessageParser<object>)
		// object Google.Protobuf.ProtoPreconditions.CheckNotNull<object>(object,string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// bool System.Enum.TryParse<Game.Data.Languages>(string,Game.Data.Languages&)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.KeyValuePair<object,int> System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<object,int>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,int)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// bool System.Linq.Enumerable.SequenceEqual<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<Framework.Loom.DelayedQueueItem> System.Linq.Enumerable.Where<Framework.Loom.DelayedQueueItem>(System.Collections.Generic.IEnumerable<Framework.Loom.DelayedQueueItem>,System.Func<Framework.Loom.DelayedQueueItem,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForCompletion>d__10>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForCompletion>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForElapsedLoops>d__13>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForElapsedLoops>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForKill>d__12>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForKill>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForPosition>d__14>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForPosition>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForRewind>d__11>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForRewind>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForStart>d__15>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForStart>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForCompletion>d__10>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForCompletion>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForElapsedLoops>d__13>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForElapsedLoops>d__13&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForKill>d__12>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForKill>d__12&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForPosition>d__14>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForPosition>d__14&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForRewind>d__11>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForRewind>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForStart>d__15>(DG.Tweening.DOTweenModuleUnityVersion.<AsyncWaitForStart>d__15&)
		// object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
		// object UnityEngine.AndroidJavaObject.Call<object>(string,object[])
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.EventSystems.ExecuteEvents.Execute<object>(UnityEngine.GameObject,UnityEngine.EventSystems.BaseEventData,UnityEngine.EventSystems.ExecuteEvents.EventFunction<object>)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Transform)
		// object UnityEngine.Resources.GetBuiltinResource<object>(string)
	}
}