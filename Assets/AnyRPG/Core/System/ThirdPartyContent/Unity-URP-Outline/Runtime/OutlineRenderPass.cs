using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The outline render pass.
/// </summary>
public sealed class OutlineRenderPass : ScriptableRenderPass
{
	#region Definitions

	/// <summary>
	/// The subpasses the outline render pass is made of.
	/// </summary>
	private enum PassStage : byte
	{
		RenderObjects,
		CombineMasks,
		HorizontalBlur,
		VerticalBlur,
		Resolve,
	}

	/// <summary>
	/// Holds the data needed by the execution of the outline render pass subpasses.
	/// </summary>
	private class PassData
	{
		public PassStage stage;

		public TextureHandle target;
		public TextureHandle source;

		public Material material;
		public int materialPassIndex;

		public UniversalCameraData cameraData;

		public RendererListHandle rendererListHandleR;
		public RendererListHandle rendererListHandleG;
		public RendererListHandle rendererListHandleB;
		public RendererListHandle rendererListHandleA;

		public TextureHandle nonBlurredCombinedMask;
		public TextureHandle blurredRenderObjectsTarget;
		public TextureHandle resolveTarget;
	}

	#endregion

	#region Private Attributes

	private static readonly ShaderTagId[] ShaderTagIds = { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly") };

	private static readonly uint RenderingLayerMaskR = RenderingLayerMask.GetMask("Outline_1");
	private static readonly uint RenderingLayerMaskG = RenderingLayerMask.GetMask("Outline_2");
	private static readonly uint RenderingLayerMaskB = RenderingLayerMask.GetMask("Outline_3");
	private static readonly uint RenderingLayerMaskA = RenderingLayerMask.GetMask("Outline_4");

	private static readonly int BlurKernelRadiusId = Shader.PropertyToID("_BlurKernelRadius");
	private static readonly int BlurStandardDeviationId = Shader.PropertyToID("_BlurStandardDeviation");

	private static readonly int OutlineMaskColorId = Shader.PropertyToID("_OutlineMaskColor");
	private static readonly int OutlineColorsId = Shader.PropertyToID("_OutlineColors");
	private static readonly int OutlineFallOffsId = Shader.PropertyToID("_OutlineFallOffs");
	private static readonly int FillAlphasId = Shader.PropertyToID("_FillAlphas");

	private static readonly int RenderObjectsTargetId = Shader.PropertyToID("_OutlineRenderedObjectsMaskTexture");
	private static readonly int BlurredRenderObjectsTargetId = Shader.PropertyToID("_OutlineBlurredRenderedObjectsMaskTexture");

	private static readonly List<Color> ColorsList = new List<Color>(4);

	private Material outlineMaterial;

	#endregion

	#region Initialization Methods

	public OutlineRenderPass(Material outlineMaterial) : base()
	{
		profilingSampler = new ProfilingSampler("Outline");
		renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		requiresIntermediateTexture = false;

		this.outlineMaterial = outlineMaterial;
	}

	#endregion

	#region Scriptable Render Pass Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="renderGraph"></param>
	/// <param name="frameData"></param>
	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
		UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

		CreateRenderGraphTextures(renderGraph, cameraData, out TextureHandle outlineCombinedMask, out TextureHandle horizontalBlurTarget, out TextureHandle verticalBlurTarget, out TextureHandle resolveTarget);

		using (IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass("Outline Render Objects Pass", out PassData passData, profilingSampler))
		{
			passData.stage = PassStage.RenderObjects;
			passData.target = verticalBlurTarget; // < The target is where the combined R,G,B,A channels are going to be combined. We reuse the second directional blur pass texture, although rendergraph may be doing that already.
			passData.cameraData = cameraData;
			passData.nonBlurredCombinedMask = outlineCombinedMask;
			passData.rendererListHandleR = CreateRendererList(renderGraph, renderingData, cameraData, RenderingLayerMaskR, 0);
			passData.rendererListHandleG = CreateRendererList(renderGraph, renderingData, cameraData, RenderingLayerMaskG, 0);
			passData.rendererListHandleB = CreateRendererList(renderGraph, renderingData, cameraData, RenderingLayerMaskB, 0);
			passData.rendererListHandleA = CreateRendererList(renderGraph, renderingData, cameraData, RenderingLayerMaskA, 0);
			passData.material = outlineMaterial;

			builder.UseTexture(passData.nonBlurredCombinedMask, AccessFlags.WriteAll);
			builder.UseTexture(passData.target, AccessFlags.Write);
			builder.UseRendererList(passData.rendererListHandleR);
			builder.UseRendererList(passData.rendererListHandleG);
			builder.UseRendererList(passData.rendererListHandleB);
			builder.UseRendererList(passData.rendererListHandleA);
			builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecuteUnsafeRenderOutlinePass(data, context));
		}

		using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Outline Horizontal Blur Pass", out PassData passData, profilingSampler))
		{
			passData.stage = PassStage.HorizontalBlur;
			passData.target = horizontalBlurTarget;
			passData.source = verticalBlurTarget;
			passData.material = outlineMaterial;
			passData.materialPassIndex = 1;

			builder.SetRenderAttachment(passData.target, 0);
			builder.UseTexture(passData.source);
			builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
		}

		using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Outline Vertical Blur Pass", out PassData passData, profilingSampler))
		{
			passData.stage = PassStage.VerticalBlur;
			passData.target = verticalBlurTarget;
			passData.source = horizontalBlurTarget;
			passData.material = outlineMaterial;
			passData.materialPassIndex = 2;

			builder.SetRenderAttachment(passData.target, 0);
			builder.UseTexture(passData.source);
			builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
		}

		using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Outline Resolve Pass", out PassData passData, profilingSampler))
		{
			passData.stage = PassStage.Resolve;
			passData.target = resolveTarget;
			passData.source = resourceData.cameraColor;
			passData.material = outlineMaterial;
			passData.materialPassIndex = 3;
			passData.nonBlurredCombinedMask = outlineCombinedMask;
			passData.blurredRenderObjectsTarget = verticalBlurTarget;

			builder.SetRenderAttachment(passData.target, 0);
			builder.UseTexture(passData.source);
			builder.UseTexture(passData.nonBlurredCombinedMask);
			builder.UseTexture(passData.blurredRenderObjectsTarget);
			builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
		}

		resourceData.cameraColor = resolveTarget;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates and returns all the necessary render graph textures.
	/// </summary>
	/// <param name="renderGraph"></param>
	/// <param name="cameraData"></param>
	/// <param name="nonBlurredCombinedMask"></param>
	/// <param name="horizontalBlurTarget"></param>
	/// <param name="verticalBlurTarget"></param>
	/// <param name="resolveTarget"></param>
	private void CreateRenderGraphTextures(RenderGraph renderGraph, UniversalCameraData cameraData, out TextureHandle nonBlurredCombinedMask, out TextureHandle horizontalBlurTarget, out TextureHandle verticalBlurTarget, out TextureHandle resolveTarget)
	{
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.depthBufferBits = (int)DepthBits.None;
		resolveTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_OutlineResolve", false);

		cameraTargetDescriptor.colorFormat = RenderTextureFormat.ARGB32;
		nonBlurredCombinedMask = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_OutlineCombinedMask", false, FilterMode.Point);
		horizontalBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_OutlineHorizontalBlur", false, FilterMode.Point);
		verticalBlurTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_OutlineVerticalBlur", false, FilterMode.Point);
	}

	/// <summary>
	/// Creates and returns the renderer list used to render the objects in the outline rendering layer.
	/// </summary>
	/// <param name="renderGraph"></param>
	/// <param name="renderingData"></param>
	/// <param name="cameraData"></param>
	/// <param name="renderingLayerMask"></param>
	/// <param name="passIndex"></param>
	/// <returns></returns>
	private RendererListHandle CreateRendererList(RenderGraph renderGraph, UniversalRenderingData renderingData, UniversalCameraData cameraData, uint renderingLayerMask, int passIndex)
	{
		RendererListDesc rendererListDesc = new RendererListDesc(ShaderTagIds, renderingData.cullResults, cameraData.camera);

		rendererListDesc.layerMask = ~0;
		rendererListDesc.renderingLayerMask = renderingLayerMask;
		rendererListDesc.overrideShader = null;
		rendererListDesc.overrideShaderPassIndex = -1;
		rendererListDesc.overrideMaterial = outlineMaterial;
		rendererListDesc.overrideMaterialPassIndex = passIndex;
		rendererListDesc.renderQueueRange = RenderQueueRange.all;
		rendererListDesc.stateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		rendererListDesc.sortingCriteria = SortingCriteria.None;
		rendererListDesc.rendererConfiguration = PerObjectData.None;
		rendererListDesc.excludeObjectMotionVectors = false;

		return renderGraph.CreateRendererList(rendererListDesc);
	}

	/// <summary>
	/// Updates the material properties that are needed to render the outline.
	/// </summary>
	/// <param name="passData"></param>
	private static void UpdateOutlineMaterialProperties(PassData passData)
	{
		PassStage stage = passData.stage;

		if (stage == PassStage.HorizontalBlur)
		{
			OutlineVolumeComponent outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
			Material outlineMaterial = passData.material;

			outlineMaterial.SetFloat(BlurKernelRadiusId, outlineVolume.blurRadius.value);
			outlineMaterial.SetFloat(BlurStandardDeviationId, Mathf.Floor((float)outlineVolume.blurRadius * 0.5f));
		}
		else if (stage == PassStage.Resolve)
		{
			OutlineVolumeComponent outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
			Material outlineMaterial = passData.material;

			while (ColorsList.Count < 4)
				ColorsList.Add(new Color(1.0f, 1.0f, 1.0f, 1.0f));

			ColorsList[0] = outlineVolume.color1.value;
			ColorsList[1] = outlineVolume.color2.value;
			ColorsList[2] = outlineVolume.color3.value;
			ColorsList[3] = outlineVolume.color4.value;

			outlineMaterial.SetColorArray(OutlineColorsId, ColorsList);

			Vector4 fallOffs = new Vector4(outlineVolume.fallOff1.value, outlineVolume.fallOff2.value, outlineVolume.fallOff3.value, outlineVolume.fallOff4.value);
			outlineMaterial.SetVector(OutlineFallOffsId, fallOffs);
			Vector4 fillAlphas = new Vector4(outlineVolume.fillAlpha1.value, outlineVolume.fillAlpha2.value, outlineVolume.fillAlpha3.value, outlineVolume.fillAlpha4.value);
			outlineMaterial.SetVector(FillAlphasId, fillAlphas);

			outlineMaterial.SetTexture(RenderObjectsTargetId, passData.nonBlurredCombinedMask);
			outlineMaterial.SetTexture(BlurredRenderObjectsTargetId, passData.blurredRenderObjectsTarget);
		}
	}

	/// <summary>
	/// Executes the pass with the information from the pass data.
	/// </summary>
	/// <param name="passData"></param>
	/// <param name="context"></param>
	private static void ExecutePass(PassData passData, RasterGraphContext context)
	{
		UpdateOutlineMaterialProperties(passData);

		switch (passData.stage)
		{
			case PassStage.RenderObjects:
				break;
			default:
				Blitter.BlitTexture(context.cmd, passData.source, Vector2.one, passData.material, passData.materialPassIndex);
				break;
		}
	}

	/// <summary>
	/// Executes the unsafe pass which renders the 4 outline layers.
	/// </summary>
	/// <param name="passData"></param>
	/// <param name="context"></param>
	private static void ExecuteUnsafeRenderOutlinePass(PassData passData, UnsafeGraphContext context)
	{
		UniversalCameraData cameraData = passData.cameraData;

		context.cmd.SetRenderTarget(passData.target);
		context.cmd.ClearRenderTarget(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

		context.cmd.SetGlobalColor(OutlineMaskColorId, new Color(1.0f, 0.0f, 0.0f, 0.0f));
		context.cmd.DrawRendererList(passData.rendererListHandleR);

		context.cmd.SetGlobalColor(OutlineMaskColorId, new Color(0.0f, 1.0f, 0.0f, 0.0f));
		context.cmd.DrawRendererList(passData.rendererListHandleG);

		context.cmd.SetGlobalColor(OutlineMaskColorId, new Color(0.0f, 0.0f, 1.0f, 0.0f));
		context.cmd.DrawRendererList(passData.rendererListHandleB);

		context.cmd.SetGlobalColor(OutlineMaskColorId, new Color(0.0f, 0.0f, 0.0f, 1.0f));
		context.cmd.DrawRendererList(passData.rendererListHandleA);

		// save the non blurred mask because the previous one will be blurred
		CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		Blitter.BlitCameraTexture(cmd, passData.target, passData.nonBlurredCombinedMask);
	}

	#endregion
}