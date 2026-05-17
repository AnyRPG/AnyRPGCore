// Returns the gaussian weight given the standard deviation and 1D offset.
// See: https://en.wikipedia.org/wiki/Gaussian_blur
float Gaussian(float standardDeviation, float offset)
{
    float a = 1.0 / (sqrt(TWO_PI) * standardDeviation);
    
    float offsetSq = offset * offset;
    float standardDeviationSq = standardDeviation * standardDeviation;
    float b = exp(-(offsetSq / (2.0 * standardDeviationSq)));
    
    return a * b;
}

// Blurs the given texture using gaussian blur with the given kernel radius and standard deviation.
// Blurs all texture channels, including alpha.
float4 GaussianBlur(float2 uv, float2 dir, int kernelRadius, float standardDeviation, TEXTURE2D_X(textureToBlur), SAMPLER(sampler_TextureToBlur), float2 textureToBlurTexelSizeXy)
{
    float2 texelSizeTimesDir = textureToBlurTexelSizeXy * dir;
    float4 result = float4(0.0, 0.0, 0.0, 0.0);

    UNITY_LOOP
    for (int i = -kernelRadius; i <= kernelRadius; ++i)
    {
        float2 uvOffset = (float)i * texelSizeTimesDir;
        float2 uvSample = uv + uvOffset;
        float4 rgba = SAMPLE_TEXTURE2D_X(textureToBlur, sampler_TextureToBlur, uvSample);
        result += (rgba * Gaussian(standardDeviation, (float)i));
    }

    return result;
}