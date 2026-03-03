using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"Framework.dll",
		"Google.Protobuf.dll",
		"LitJson.dll",
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
	// DG.Tweening.Core.DOGetter<UnityEngine.Vector2>
	// DG.Tweening.Core.DOGetter<float>
	// DG.Tweening.Core.DOGetter<int>
	// DG.Tweening.Core.DOSetter<UnityEngine.Color>
	// DG.Tweening.Core.DOSetter<UnityEngine.Vector2>
	// DG.Tweening.Core.DOSetter<float>
	// DG.Tweening.Core.DOSetter<int>
	// Framework.Singleton<object>
	// Google.Protobuf.Collections.RepeatedField<object>
	// Google.Protobuf.IDeepCloneable<object>
	// Google.Protobuf.IMessage<object>
	// Google.Protobuf.MessageParser<object>
	// System.Action<byte,object>
	// System.Action<float>
	// System.Action<int,object>
	// System.Action<object,object,object>
	// System.Action<object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<Game.Data.OptionItemType,object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>
	// System.Collections.Generic.KeyValuePair<object,double>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Game.Data.DataManager.Languages>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,double>>
	// System.Collections.Generic.List<System.ValueTuple<System.ValueTuple<object,object,int,byte>,object,object>>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.Queue<object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>,object>
	// System.Func<int>
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
	// System.Nullable<System.ValueTuple<object,object,int,byte>>
	// System.Nullable<UnityEngine.Color>
	// System.Nullable<float>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.CallSite<object>
	// System.ValueTuple<Game.Data.TipType,object>
	// System.ValueTuple<Game.Presentation.UI.Tips,object>
	// System.ValueTuple<System.ValueTuple<object,object,int,byte>,object,object>
	// System.ValueTuple<int,int,object>
	// System.ValueTuple<object,object,byte,byte>
	// System.ValueTuple<object,object,int,byte>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<float>
	// UnityEngine.Events.UnityEvent<object>
	// }}

	public void RefMethods()
	{
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.OnUpdate<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// Google.Protobuf.FieldCodec<object> Google.Protobuf.FieldCodec.ForMessage<object>(uint,Google.Protobuf.MessageParser<object>)
		// object Google.Protobuf.ProtoPreconditions.CheckNotNull<object>(object,string)
		// object LitJson.JsonMapper.ToObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// int System.Array.IndexOf<int>(int[],int)
		// bool System.Enum.TryParse<Game.Data.DataManager.Languages>(string,Game.Data.DataManager.Languages&)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<Game.Data.DataManager.Languages> System.Linq.Enumerable.Cast<Game.Data.DataManager.Languages>(System.Collections.IEnumerable)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.KeyValuePair<object,int> System.Linq.Enumerable.ElementAt<System.Collections.Generic.KeyValuePair<object,int>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>,int)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// bool System.Linq.Enumerable.SequenceEqual<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.Dictionary<object,object> System.Linq.Enumerable.ToDictionary<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>,object,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>>,System.Func<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>,object>,System.Func<System.Collections.Generic.KeyValuePair<object,System.ValueTuple<int,int,object>>,object>)
		// System.Collections.Generic.List<Game.Data.DataManager.Languages> System.Linq.Enumerable.ToList<Game.Data.DataManager.Languages>(System.Collections.Generic.IEnumerable<Game.Data.DataManager.Languages>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
		// object UnityEngine.AndroidJavaObject.Call<object>(string,object[])
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponents<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.EventSystems.ExecuteEvents.Execute<object>(UnityEngine.GameObject,UnityEngine.EventSystems.BaseEventData,UnityEngine.EventSystems.ExecuteEvents.EventFunction<object>)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object[] UnityEngine.Object.FindObjectsOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Resources.GetBuiltinResource<object>(string)
		// string string.Join<int>(string,System.Collections.Generic.IEnumerable<int>)
	}
}