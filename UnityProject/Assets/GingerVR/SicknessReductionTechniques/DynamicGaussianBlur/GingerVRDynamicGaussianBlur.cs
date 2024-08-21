using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

    [Serializable]
    [PostProcess(typeof(GingerVRDynamicGaussianBlurRenderer), PostProcessEvent.AfterStack, "Custom/GingerVRGaussianBlur")]
    public sealed class GingerVRDynamicGaussianBlur : PostProcessEffectSettings
    {
     public float[] kernel = new float[25];
     public float kernelSum = 0;
     public float smoothness = 1;
    }


    public sealed class GingerVRDynamicGaussianBlurRenderer : PostProcessEffectRenderer<GingerVRDynamicGaussianBlur>
    {
        public override void Render(PostProcessRenderContext context)
        {
         CommandBuffer command = context.command;

        float[] kernel = settings.kernel;
        float kernelSum = settings.kernelSum;
        float smoothness = settings.smoothness;
        int rtW = context.width;
        int rtH = context.height;

        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Custom/GaussianBlur"));            

        sheet.properties.SetFloatArray("_kernel", kernel);
        sheet.properties.SetFloat("_kernelSum", kernelSum);
        sheet.properties.GetFloatArray("_kernel");

        RenderTexture renderTexture = RenderTexture.GetTemporary(rtW, rtH, 0, context.sourceFormat);
        context.command.Blit(context.source, renderTexture);

        for (int i = 0; i < smoothness; i++)
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(rtW, rtH, 0, context.sourceFormat);
            context.command.Blit(renderTexture, tempTexture);
            RenderTexture.ReleaseTemporary(renderTexture);
            renderTexture = tempTexture;
        }

        command.Blit(renderTexture, context.destination);
        RenderTexture.ReleaseTemporary(renderTexture);
    }
}
