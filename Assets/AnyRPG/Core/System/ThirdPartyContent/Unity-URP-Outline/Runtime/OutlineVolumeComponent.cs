using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Volume component for the outline.
/// </summary>
[DisplayInfo(name = "Outline")]
[VolumeComponentMenu("Custom/Outline")]
[VolumeRequiresRendererFeatures(typeof(OutlineRendererFeature))]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class OutlineVolumeComponent : VolumeComponent, IPostProcessComponent
{
	#region Public Attributes

	public ClampedIntParameter blurRadius = new ClampedIntParameter(5, 2, 32);

	[Header("Colors")]
	public ColorParameter color1 = new ColorParameter(new Color(0.3f, 0.75f, 1.0f, 1.0f), true, true, true, true);
	public ColorParameter color2 = new ColorParameter(new Color(0.3f, 0.75f, 1.0f, 1.0f), true, true, true, true);
	public ColorParameter color3 = new ColorParameter(new Color(0.3f, 0.75f, 1.0f, 1.0f), true, true, true, true);
	public ColorParameter color4 = new ColorParameter(new Color(0.3f, 0.75f, 1.0f, 1.0f), true, true, true, true);

	[Header("Fall Offs")]
	public ClampedFloatParameter fallOff1 = new ClampedFloatParameter(0.015f, 0.0f, 1.0f);
	public ClampedFloatParameter fallOff2 = new ClampedFloatParameter(0.015f, 0.0f, 1.0f);
	public ClampedFloatParameter fallOff3 = new ClampedFloatParameter(0.015f, 0.0f, 1.0f);
	public ClampedFloatParameter fallOff4 = new ClampedFloatParameter(0.015f, 0.0f, 1.0f);

	[Header("Fill Alphas")]
	public ClampedFloatParameter fillAlpha1 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
	public ClampedFloatParameter fillAlpha2 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
	public ClampedFloatParameter fillAlpha3 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
	public ClampedFloatParameter fillAlpha4 = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

	#endregion

	#region Initialization Methods

	public OutlineVolumeComponent() : base()
	{
		//displayName = "Outline";
	}

	#endregion

	#region IPostProcessComponent Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <returns></returns>
	public bool IsActive()
	{
		bool active1A = color1.value.a > 0.0f || fillAlpha1.value > 0.0f;
		bool active2A = color2.value.a > 0.0f || fillAlpha2.value > 0.0f;
		bool active3A = color3.value.a > 0.0f || fillAlpha3.value > 0.0f;
		bool active4A = color4.value.a > 0.0f || fillAlpha4.value > 0.0f;

		return active1A || active2A || active3A || active4A;
	}

	#endregion
}