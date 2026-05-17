using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The outline renderer feature.
/// </summary>
[Tooltip("Adds support to render an outline with optional fill for objects.")]
[DisallowMultipleRendererFeature("Outline")]
public sealed class OutlineRendererFeature : ScriptableRendererFeature
{
	#region Private Attributes

	[HideInInspector]
	[SerializeField] private Shader outlineShader;

	private Material outlineMaterial;
	private OutlineRenderPass outlineRenderPass;

	#endregion

	#region Scriptable Renderer Feature Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public override void Create()
	{
		ValidateResourcesForOutlineRenderPass(true);

		outlineRenderPass = new OutlineRenderPass(outlineMaterial);
	}

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="renderer"></param>
	/// <param name="renderingData"></param>
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		bool isPostProcessEnabled = renderingData.postProcessingEnabled && renderingData.cameraData.postProcessEnabled;
		bool shouldAddOutlineRenderPass = isPostProcessEnabled && ShouldAddOutlineRenderPass(renderingData.cameraData.cameraType);

		if (shouldAddOutlineRenderPass)
			renderer.EnqueuePass(outlineRenderPass);
	}

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="disposing"></param>
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		CoreUtils.Destroy(outlineMaterial);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Validates the resources used by the outline render pass.
	/// </summary>
	/// <param name="forceRefresh"></param>
	/// <returns></returns>
	private bool ValidateResourcesForOutlineRenderPass(bool forceRefresh)
	{
		if (forceRefresh)
		{
#if UNITY_EDITOR
			outlineShader = Shader.Find("Hidden/Outline");
#endif
			CoreUtils.Destroy(outlineMaterial);
			outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
		}

		return outlineShader != null && outlineMaterial != null;
	}

	/// <summary>
	/// Gets whether the outline render pass should be enqueued to the renderer.
	/// </summary>
	/// <param name="cameraType"></param>
	/// <returns></returns>
	private bool ShouldAddOutlineRenderPass(CameraType cameraType)
	{
		OutlineVolumeComponent outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();

		bool isVolumeOk = outlineVolume != null && outlineVolume.IsActive();
		bool isCameraOk = cameraType != CameraType.Preview && cameraType != CameraType.Reflection;
		bool areResourcesOk = ValidateResourcesForOutlineRenderPass(false);

		return isActive && isVolumeOk && isCameraOk && areResourcesOk;
	}

	#endregion
}